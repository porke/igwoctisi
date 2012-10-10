namespace Client.Network
{
    using System;
    using System.Net;
    using System.Threading;
    using Common;
    using Model;
    using System.Net.Sockets;
    using System.IO;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;
    using System.Diagnostics;

    public class WsaNetwork : INetwork
    {
        #region Public members

        /// <summary>
        /// Arguments: username, chat message, time
        /// </summary>
        public event Action<string, string, string> OnChatMessageReceived;
        
        /// <summary>
        /// Argument: disconnection reason (may be empty)
        /// </summary>
        public event Action<string> OnDisconnected;

        #endregion

        #region Internal connection handling

        enum PacketType
        {
            Header,
            Content,
            ContentAsResponse
        };

        enum MessageContentType
        {
            None,

            //Packet type:    Source:

            Login,          //Client
            GameList,       //Client,Server
            Logout,         //Client
            GameJoin,       //Client
            GameCreate,     //Client
            Chat,           //Client,Server
            GameInfo,       //Server
            GameLeave,      //Client
            GameKick,       //Server
            GameStart,      //Client
            GameStarted,    //Server
            RoundStart,     //Server
            Moves,          //Client
            RoundEnd,       //Server
            GameEnd,        //Server

            Ok,             //Client,Server
            LoginFailed,    //Server
            GameListEmpty,  //Server
            GameInvalidId,  //Server
            GameFull,       //Server
            GameCreateFailed//Server
        }

        bool IsConnectionSuccessful = false;
        Exception socketexception = null;
        ManualResetEvent TimeoutObject = new ManualResetEvent(false);
        NetworkStream networkStream;
        StreamWriter networkStreamWriter;
        Dictionary<int, Action<JObject>> messageResponses = new Dictionary<int, Action<JObject>>();
        int lastGeneratedId = 0;

        private const int TIMEOUT_MILLISECONDS = 3000;

        private void TcpConnectCallback(IAsyncResult tcpAsyncResult)
        {
            try
            {
                IsConnectionSuccessful = false;
                TcpClient tcpclient = tcpAsyncResult.AsyncState as TcpClient;
             
                if (tcpclient.Client != null)
                {
                    tcpclient.EndConnect(tcpAsyncResult);
                    IsConnectionSuccessful = true;
                }
            }
            catch (Exception ex)
            {
                IsConnectionSuccessful = false;
                socketexception = ex;
            }
            finally
            {
                TimeoutObject.Set();
            }
        }

        private void StartReceiving()
        {
            var thread = new Thread(new ThreadStart(ReceiveThread));
            thread.Start();
        }

        private void ReceiveThread()
        {
            var sr = new StreamReader(networkStream, new System.Text.UTF8Encoding(false));
            string jsonLine = null;
            PacketType nextPacketType = PacketType.Header;
            MessageContentType nextContentType = MessageContentType.None;
            int messageId = 0;
            Action<JObject> responseCallback = null;

            try
            {
                while ((jsonLine = sr.ReadLine()) != null)
                {
                    // Handle situation when jsonLine isn't actually Json
                    JObject jObject = null;
                    try
                    {
                        jObject = JObject.Parse(jsonLine);
                    }
                    catch (JsonReaderException ex)
                    {
                        // Ignore that packet and continue listening.
                        Debug.WriteLine("Bad Json format: " + jsonLine, "Network");
                        continue;
                    }

                    if (nextPacketType == PacketType.Header)
                    {
                        string typeStr = jObject["type"].Value<string>();
                        messageId = jObject["id"].Value<int>();

                        // Make first letter be an upper letter (for e.g. change gameStart to GameStart).
                        typeStr = char.ToUpper(typeStr[0]) + typeStr.Substring(1);
                        MessageContentType type = (MessageContentType)Enum.Parse(typeof(MessageContentType), typeStr);

                        // Incoming message may be a response to previous request
                        if (messageResponses.ContainsKey(messageId))
                        {
                            responseCallback = messageResponses[messageId];
                            messageResponses.Remove(messageId);

                            // Next packet will be content and then we will call callback,
                            // for e.g. callback given to BeginLogin.
                            nextPacketType = PacketType.ContentAsResponse;
                        }

                        // If this message isn't a response to the client's request then check what it is, actually.
                        else if (type == MessageContentType.Chat)
                        {
                            // Next packet should contain {username, message, time}
                            nextPacketType = PacketType.Content;
                        }
                        else if (type == MessageContentType.GameKick)
                        {
                            if (OnDisconnected != null)
                            {
                                OnDisconnected.Invoke("You were kicked from the server.");
                            }

                            throw new SocketException((int)SocketError.ConnectionAborted);
                        }
                        else if (type == MessageContentType.GameStart)
                        {
                            // TODO implement!
                        }
                        else if (type == MessageContentType.RoundStart)
                        {
                            // TODO implement!
                        }
                        else if (type == MessageContentType.RoundEnd)
                        {
                            // TODO implement!
                        }
                        else if (type == MessageContentType.GameEnd)
                        {
                            // TODO implement!
                        }

                        // This variable is going to be used only when
                        // nextPacketType is PacketType.Content or PacketType.ContentAsResponse.
                        nextContentType = type;
                    }
                    else if (nextPacketType == PacketType.Content)
                    {
                        if (nextContentType == MessageContentType.Chat)
                        {
                            string username = jObject.First.Value<string>();
                            string message = jObject.First.Next.Value<string>();
                            string timeStr = jObject.First.Next.Next.Value<string>();

                            if (OnChatMessageReceived != null)
                            {
                                OnChatMessageReceived.Invoke(username, message, timeStr);
                            }
                        }

                        // Next packet will be a header for some another message.
                        nextPacketType = PacketType.Header;
                    }
                    else if (nextPacketType == PacketType.ContentAsResponse)
                    {
                        responseCallback.Invoke(jObject);

                        // Next packet will be a header for some another message.
                        nextPacketType = PacketType.Header;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is IOException || ex is SocketException)
                {
                    // Connection may be forcibly closed while waiting for message.
                    // It also could be a kick from the server.
                    // We will just catch it and go away.
                }

                throw;
            }
            finally
            {
                if (OnDisconnected != null)
                {
                    OnDisconnected.Invoke("Connection ended.");
                }
            }
        }

        private void SendJson(string message)
        {
            networkStreamWriter.WriteLine(message);
            networkStreamWriter.Flush();
        }

        private void SendRequest(MessageContentType messageContentType, string jsonRequestContent, Action<JObject> responseCallback)
        {
            // Generate id of message
            int id = Interlocked.Increment(ref lastGeneratedId);

            // Save callback awaiting for response
            messageResponses.Add(id, responseCallback);

            // Send header and content
            string messageContentTypeStr = char.ToLower(messageContentType.ToString()[0]) + messageContentType.ToString().Substring(1);
            SendJson("{id:" + id + ",type:\"" + messageContentTypeStr + "\"}" + Environment.NewLine);
            SendJson(jsonRequestContent);
        }
    
        #endregion

        #region INetwork members

        public void Initialize(Client client)
        {
            Client = client;
        }
        public void Release()
        {
            Client = null;
        }
        public void Update(double delta, double time)
        {
        }

        /// <summary>
        /// Connects to the server and starts listening for messages from it.
        /// To receive messages register OnMessageReceived event.
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        /// <param name="asyncCallback"></param>
        /// <param name="asyncState"></param>
        /// <returns></returns>
        public IAsyncResult BeginConnect(string hostname, int port, AsyncCallback asyncCallback, object asyncState)
        {
            var ar = new AsyncResult<object>(asyncCallback, asyncState);
            var tcpClient = new TcpClient();

            tcpClient.BeginConnect(hostname, port, new AsyncCallback(TcpConnectCallback), tcpClient);

            ar.BeginInvoke(() =>
            {
                if (TimeoutObject.WaitOne(TIMEOUT_MILLISECONDS, false))
                {
                    if (IsConnectionSuccessful)
                    {
                        networkStream = tcpClient.GetStream();
                        networkStreamWriter = new StreamWriter(networkStream, new System.Text.UTF8Encoding(false));
                        StartReceiving();
                        return true;
                    }
                    else
                    {
                        throw socketexception;
                    }
                }
                else
                {
                    tcpClient.Close();
                    throw new TimeoutException("TimeOut Exception");
                }
            });

            return ar;
        }
        public bool EndConnect(IAsyncResult asyncResult)
        {
            var ar = (AsyncResult<object>)asyncResult;
            var res = ar.Result;
            ar.EndInvoke();

            return (bool)res;
        }
        public IAsyncResult BeginLogin(string login, string password, AsyncCallback asyncCallback, object asyncState)
        {
            var ar = new AsyncResult<GameState>(asyncCallback, asyncState);
            
            string requestContent = "{\"" + login + "\",\"" + password + "\"}";
            SendRequest(MessageContentType.Login, requestContent, jObject => {
                // TODO interpret jObject response from server
                //jObject
            });

            return ar;
        }
        public void EndLogin(IAsyncResult asyncResult)
        {
            var ar = (AsyncResult<object>)asyncResult;
        }
        public IAsyncResult BeginDisconnect(AsyncCallback asyncCallback, object asyncState)
        {
            var ar = new AsyncResult<object>(asyncCallback, asyncState);
            // simulate time consuming task
            ar.BeginInvoke(() => { Thread.Sleep(500); return null; });
            return ar;
        }
        public void EndDisconnect(IAsyncResult asyncResult)
        {
            var ar = (AsyncResult<object>)asyncResult;
            ar.EndInvoke();
        }

        public IAsyncResult BeginReceiveGameState(AsyncCallback asyncCallback, object asyncState)
        {
            var ar = new AsyncResult<GameState>(asyncCallback, asyncState);
            // simulate time consuming task
            ar.BeginInvoke(() => { Thread.Sleep(500); return null; });
            return ar;
        }
        public GameState EndReceiveGameState(IAsyncResult asyncResult)
        {
            var ar = (AsyncResult<GameState>)asyncResult;
            return ar.EndInvoke();
        }
        public IAsyncResult BeginSendCommands(UserCommands commands, AsyncCallback asyncCallback, object asyncState)
        {
            var ar = new AsyncResult<object>(asyncCallback, asyncState);
            // simulate time consuming task
            ar.BeginInvoke(() => { Thread.Sleep(500); return null; });
            return ar;
        }
        public void EndSendCommands(IAsyncResult asyncResult)
        {
            var ar = (AsyncResult<object>)asyncResult;
            ar.EndInvoke();
        }

        #endregion

        public Client Client { get; protected set; }
    }
}
