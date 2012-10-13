namespace Client.Network
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Sockets;
    using System.Threading;
    using Common;
    using Model;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class WsaNetwork : INetwork
    {
        #region Public members

        /// <summary>
        /// Arguments: username, chat message, time
        /// </summary>
        public event Action<ChatMessage> OnChatMessageReceived;
        
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

        TcpClient tcpClient;
        bool IsConnectionSuccessful = false;
        Exception socketexception = null;
        ManualResetEvent TimeoutObject = new ManualResetEvent(false);
        NetworkStream networkStream;
        StreamWriter networkStreamWriter;
        int lastGeneratedId = 0;

        /// <summary>
        /// Given Func should return true if listener is waiting for Content packet type.
        /// This is only considered when, given by argument, PacketType is Header.
        /// </summary> 
        Dictionary<int, Func<string, PacketType, MessageContentType, bool>> messageResponses
            = new Dictionary<int, Func<string, PacketType, MessageContentType, bool>>();

        private const int TIMEOUT_MILLISECONDS = 3000;

        private void TcpConnectCallback(IAsyncResult tcpAsyncResult)
        {
            try
            {
                IsConnectionSuccessful = false;
                tcpClient = tcpAsyncResult.AsyncState as TcpClient;
             
                if (tcpClient.Client != null)
                {
                    tcpClient.EndConnect(tcpAsyncResult);
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
            Func<string, PacketType, MessageContentType, bool> responseCallback = null;

            messageResponses.Clear();
            try
            {
                while ((jsonLine = sr.ReadLine()) != null)
                {
                    if (nextPacketType == PacketType.Header)
                    {
                        // Handle situation when jsonLine isn't actually Json
                        JObject jObject = null;
                        try
                        {
                            jObject = JObject.Parse(jsonLine);
                        }
                        catch (JsonReaderException)
                        {
                            // Ignore that packet and continue listening.
                            Debug.WriteLine("Bad Json format: " + jsonLine, "Network");
                            continue;
                        }

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

                            if (responseCallback.Invoke(jsonLine, PacketType.Header, type))
                            {
                                // Next packet will be content and then we will call callback,
                                // for e.g. callback given to BeginLogin.
                                nextPacketType = PacketType.ContentAsResponse;
                            }
                            else
                            {
                                // Don't listen for Content. It will not come.
                                nextPacketType = PacketType.Header;
                            }
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
                            if (OnChatMessageReceived != null)
                            {
                                var msg = JsonLowercaseSerializer.DeserializeObject<ChatMessage>(jsonLine);
                                OnChatMessageReceived.Invoke(msg);
                            }
                        }

                        // Next packet will be a header for some another message.
                        nextPacketType = PacketType.Header;
                    }
                    else if (nextPacketType == PacketType.ContentAsResponse)
                    {
                        responseCallback.Invoke(jsonLine, PacketType.Content, nextContentType);

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
                else
                {
                    // Throw unknown exceptions.
                    throw;
                }
            }
            finally
            {
                if (OnDisconnected != null)
                {
                    OnDisconnected.Invoke("Connection ended.");
                }

                try
                {
                    TimeoutObject.Reset();
                }
                catch { }
            }
        }

        private void SendJson(string message)
        {
            networkStreamWriter.WriteLine(message);
            networkStreamWriter.Flush();
        }

        private int GenerateMessageId()
        {
            return Interlocked.Increment(ref lastGeneratedId);
        }

        /// <summary>
        /// Sends request for response of given type. It calls responseCallback when response come.
        /// </summary>
        private void SendRequest(MessageContentType messageContentType, string jsonRequestContent,
            Func<string, PacketType, MessageContentType, bool> responseCallback)
        {
            // Generate id of message
            int id = GenerateMessageId();

            // Save callback awaiting for response
            messageResponses.Add(id, responseCallback);

            // Send header and content of message
            SendHeaderAndContent(id, messageContentType, jsonRequestContent);
        }

        /// <summary>
        /// Sends some information to the server and doesn't care about response.
        /// </summary>
        private void SendInfo(MessageContentType messageContentType, string jsonInfoContent)
        {
            // Generate id of message
            int id = GenerateMessageId();

            SendHeaderAndContent(id, messageContentType, jsonInfoContent);
        }

        /// <summary>
        /// Function used by SendInfo() and SendRequest().
        /// </summary>
        private void SendHeaderAndContent(int id, MessageContentType messageContentType, string jsonInfoContent)
        {
            // Send header
            string messageContentTypeStr = char.ToLower(messageContentType.ToString()[0]) + messageContentType.ToString().Substring(1);
            var header = JsonLowercaseSerializer.SerializeObject(new
            {
                Id = id,
                Type = messageContentTypeStr
            });

            SendJson(header);

            // Send message content if needed
            if (jsonInfoContent != null)
            {
                SendJson(jsonInfoContent);
            }
        }
    
        #endregion

        #region INetwork members

        public void Initialize(GameClient client)
        {
            Client = client;
        }
        public void Release()
        {
            Client = null;
            try
            {
                tcpClient.Close();
            }
            catch { }
            tcpClient = null;
        }
        public void Update(double delta, double time)
        {
        }

        /// <summary>
        /// Connects to the server and starts listening for messages from it.
        /// To receive messages register OnMessageReceived event.
        /// </summary>
        public IAsyncResult BeginConnect(string hostname, int port, AsyncCallback asyncCallback, object asyncState)
        {
            var ar = new AsyncResult<bool>(asyncCallback, asyncState);
            var tcpClient = new TcpClient();

            TimeoutObject.Reset();
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
            var ar = (AsyncResult<bool>)asyncResult;
            ar.EndInvoke();

            return ar.Result;
        }
        public IAsyncResult BeginLogin(string login, string password, AsyncCallback asyncCallback, object asyncState)
        {
            var ar = new AsyncResult<bool>(asyncCallback, asyncState);

            string requestContent = JsonLowercaseSerializer.SerializeObject(new
            {
                Login = login,
                Password = password
            });

            SendRequest(MessageContentType.Login, requestContent, (jsonStr, packetType, messageContentType) =>
            {
                // It always should should be Header.
                Debug.Assert(packetType == PacketType.Header);

                bool loggedIn = messageContentType == MessageContentType.Ok;
                ar.BeginInvoke(() =>
                {
                    if (loggedIn)
                        return true;
                    else
                        throw new Exception("Login failed due to the error: " + messageContentType.ToString());
                });

                // We don't want any Content packet.
                return false;
            });

            return ar;
        }
        public bool EndLogin(IAsyncResult asyncResult)
        {
            var ar = (AsyncResult<bool>)asyncResult;
            ar.EndInvoke();
            return (bool)ar.Result;
        }

        public IAsyncResult BeginLogout(AsyncCallback asyncCallback, object asyncState)
        {
            var ar = new AsyncResult<object>(asyncCallback, asyncState);

            // We're not interested what server would respond
            SendInfo(MessageContentType.Logout, null);
            ar.BeginInvoke(() => { return null; });

            return ar;
        }
        
        public void EndLogout(IAsyncResult asyncResult)
        {
            var ar = (AsyncResult<object>)asyncResult;
            ar.EndInvoke();
        }

        public IAsyncResult BeginJoinGameLobby(int lobbyId, AsyncCallback asyncCallback, object asyncState)
        {
            var ar = new AsyncResult<SpecificGameLobbyInfo>(asyncCallback, asyncState);

            string requestContent = JsonLowercaseSerializer.SerializeObject(new
            {
                LobbyId = lobbyId
            });

            SendRequest(MessageContentType.GameJoin, requestContent, (jsonStr, packetType, messageContentType) =>
            {
                if (packetType == PacketType.Header)
                {
                    if (messageContentType == MessageContentType.GameInfo)
                    {
                        return true;
                    }
                    else
                    {
                        string errorMessage = "Couldn't join game: " + messageContentType.ToString();

                        if (messageContentType == MessageContentType.GameFull)
                        {
                            errorMessage = "Couldn't join game. Lobby is full.";
                        }

                        ar.HandleException(new Exception(errorMessage), false);
                    }
                }
                else if (packetType == PacketType.Content)
                {
                    ar.BeginInvoke(() =>
                    {
                        return JsonLowercaseSerializer.DeserializeObject<SpecificGameLobbyInfo>(jsonStr);
                    });
                }
                
                return false;
            });

            return ar;
        }

        public SpecificGameLobbyInfo EndJoinGameLobby(IAsyncResult asyncResult)
        {
            var ar = (AsyncResult<SpecificGameLobbyInfo>)asyncResult;
            var res = ar.Result;
            ar.EndInvoke();

            return res;
        }

        public IAsyncResult BeginLeaveGame(AsyncCallback asyncCallback, object asyncState)
        {
            var ar = new AsyncResult<object>(asyncCallback, asyncState);

            // We're not interested what server would respond
            SendInfo(MessageContentType.GameLeave, null);
            ar.BeginInvoke(() => { return null; });

            return ar;
        }

        public void EndLeaveGame(IAsyncResult asyncResult)
        {
            var ar = (AsyncResult<object>)asyncResult;
            ar.EndInvoke();
        }

        public IAsyncResult BeginDisconnect(AsyncCallback asyncCallback, object asyncState)
        {
            var ar = new AsyncResult<object>(asyncCallback, asyncState);

            ar.BeginInvoke(() => {
                try
                {
                    tcpClient.Close();
                }
                catch { }
                tcpClient = null;
                return null;
            });
            return ar;
        }
        public void EndDisconnect(IAsyncResult asyncResult)
        {
            var ar = (AsyncResult<object>)asyncResult;
            ar.EndInvoke();
        }

        public IAsyncResult BeginCreateGame(string gameName, string mapJsonContent, AsyncCallback asyncCallback, object asyncState)
        {
            var ar = new AsyncResult<object>(asyncCallback, asyncState);

            ar.BeginInvoke(() =>
            {
                // TODO: parse GameSTate into Json here or elsewhere...? (and implement the time consuming operation)
                Thread.Sleep(500);
                return null;
            });
            return ar;
        }
        public void EndCreateGame(IAsyncResult asyncResult)
        {
            var ar = (AsyncResult<object>)asyncResult;
            ar.EndInvoke();
        }

        public IAsyncResult BeginGetGameList(AsyncCallback asyncCallback, object asyncState)
        {
            var ar = new AsyncResult<List<LobbyListInfo>>(asyncCallback, asyncState);

            SendRequest(MessageContentType.GameList, null, (jsonStr, packetType, messageContentType) =>
            {
                if (packetType == PacketType.Header)
                {
                    if (messageContentType == MessageContentType.GameList)
                    {
                        return true;
                    }
                    else if (messageContentType == MessageContentType.GameListEmpty)
                    {
                        return false;
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException("Waiting for game list and received " + messageContentType + '.');
                    }
                }
                else if (packetType == PacketType.Content)
                {
                    ar.BeginInvoke(() =>
                    {
                        return JsonLowercaseSerializer.DeserializeObject<List<LobbyListInfo>>(jsonStr);
                    });
                }

                return false;
            });
            
            return ar;
        }
        public List<LobbyListInfo> EndGetGameList(IAsyncResult asyncResult)
        {
            var ar = (AsyncResult<List<LobbyListInfo>>)asyncResult;
            ar.EndInvoke();

            return ar.Result;
        }

        public IAsyncResult BeginReceiveGameState(AsyncCallback asyncCallback, object asyncState)
        {
            var ar = new AsyncResult<Map>(asyncCallback, asyncState);
            // simulate time consuming task
            ar.BeginInvoke(() => { Thread.Sleep(500); return null; });
            return ar;
        }
        public Map EndReceiveGameState(IAsyncResult asyncResult)
        {
            var ar = (AsyncResult<Map>)asyncResult;
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

        public GameClient Client { get; protected set; }
    }
}
