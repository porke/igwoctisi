namespace Client.Network
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net.Sockets;
    using System.Threading;
    using Common;
    using Model;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NLog;

    public class WsaNetwork : INetwork
    {
        #region Public members

        public event Action<ChatMessage> OnChatMessageReceived;        
        public event Action<string, DateTime> OnOtherPlayerJoined;
        public event Action<string, DateTime> OnOtherPlayerLeft;
        public event Action<string, DateTime> OnOtherPlayerKicked;
        public event Action OnPlayerKicked;
        public event Action<Map> OnGameStarted;
        public event Func<NewRoundInfo, bool> OnRoundStarted;
        public event Func<List<SimulationResult>, bool> OnRoundEnded;
        public event Action OnGameEnded;
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
            GamePlayerKick, //Client(host)
            GameKick,       //Server
            GameStart,      //Client
            GameStarted,    //Server
            GamePlayerJoined,//Server
            GamePlayerLeft, //Server
            GamePlayerKicked,//Server
            RoundStart,     //Server
            Commands,       //Client
            RoundEnd,       //Server
			Ready,			//Client
            GameEnd,        //Server

            Ok,             //Client,Server
            Error           //Server
        }

        enum ErrorType
        {
            None,
            Unknown,
            
            //Packet type:    Source:

            LoginFailed,    //Server
            GameInvalidId,  //Server
            GameFull,       //Server
            GameCreateFailed,//Server
            GameInvalidCommand,//Server
			CommandsAlreadySent,//Server
        }

        TcpClient tcpClient;
        bool IsConnectionSuccessful = false;
        Exception socketexception = null;
        ManualResetEvent TimeoutObject = new ManualResetEvent(false);
        NetworkStream networkStream;
        StreamWriter networkStreamWriter;
        int lastGeneratedId = 0;
        
        Dictionary<int, Tuple<bool, Action<string, MessageContentType, ErrorType>>> messageResponses
            = new Dictionary<int, Tuple<bool, Action<string, MessageContentType, ErrorType>>>();

        List<Func<bool>> _incomingWaitingMessages = new List<Func<bool>>();
        List<Func<bool>> _incomingWaitingMessagesToBeRemoved = new List<Func<bool>>();

        private const int TIMEOUT_MILLISECONDS = 15000;

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
            Action<string, MessageContentType, ErrorType> responseCallback = null;

            try
            {
                while ((jsonLine = sr.ReadLine()) != null)
                {
                    Debug.WriteLine("S: " + jsonLine);

                    #region Header part
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

                        string typeStr = null;
                        try
                        {
                            typeStr = jObject["type"].Value<string>();
                            messageId = jObject["id"].Value<int>();
                        }
                        catch (JsonReaderException)
                        {
                            // Ignore that packet and continue listening.
                            Debug.WriteLine("Bad Message Header format: " + jsonLine, "Network");
                            continue;
                        }

                        // Make first letter be an upper letter (for e.g. change gameStart to GameStart).
                        typeStr = Utils.UpperFirstLetter(typeStr);
                        MessageContentType type = (MessageContentType)Enum.Parse(typeof(MessageContentType), typeStr);

                        // Incoming message may be a response to previous request
                        if (messageResponses.ContainsKey(messageId))
                        {
                            bool waitingForContent = messageResponses[messageId].Item1;
                            responseCallback = messageResponses[messageId].Item2;
                            messageResponses.Remove(messageId);

                            if (type == MessageContentType.Error)
                            {
                                // Next packet will contain ErrorType.
                                nextPacketType = PacketType.ContentAsResponse;
                            }
                            else if (waitingForContent)
                            {
                                // Next packet will be content and then we will call callback,
                                // for e.g. callback given to BeginLogin.
                                nextPacketType = PacketType.ContentAsResponse;
                            }
                            else
                            {
                                responseCallback.Invoke(jsonLine, type, ErrorType.None);

                                // Don't listen for Content. It will not come.
                                nextPacketType = PacketType.Header;
                            }
                        }

                        else if (type == MessageContentType.Error)
                        {
                            nextPacketType = PacketType.Content;
                        }

                        // If this message isn't a response to the client's request then check what it is, actually.
                        else if (type == MessageContentType.Chat)
                        {
                            // Next packet should contain {username, message, time}
                            nextPacketType = PacketType.Content;
                        }
                        else if (type == MessageContentType.GameKick)
                        {
                            if (OnPlayerKicked != null)
                            {
                                OnPlayerKicked.Invoke();
                            }
                        }
                        else if (type == MessageContentType.GamePlayerJoined)
                        {
                            // Next packet should contain {username, time}
                            nextPacketType = PacketType.Content;
                        }
                        else if (type == MessageContentType.GamePlayerLeft)
                        {
                            // Next packet should contain {username, time}
                            nextPacketType = PacketType.Content;
                        }
                        else if (type == MessageContentType.GamePlayerKicked)
                        {
                            // Next packet should contain {username, time}
                            nextPacketType = PacketType.Content;
                        }
                        else if (type == MessageContentType.GameStart)
                        {
                            // Next packet should contain map
                            nextPacketType = PacketType.Content;
                        }
                        else if (type == MessageContentType.RoundStart)
                        {
                            // Next packet should contain gamestate
                            nextPacketType = PacketType.Content;
                        }
                        else if (type == MessageContentType.RoundEnd)
                        {
                            // Next packet should contain battle simulation results.
                            nextPacketType = PacketType.Content;
                        }
                        else if (type == MessageContentType.GameEnd)
                        {
                            nextPacketType = PacketType.Content;
                        }

                        // This variable is going to be used only when
                        // nextPacketType is PacketType.Content or PacketType.ContentAsResponse.
                        nextContentType = type;
                    }
                    #endregion

                    #region Content part
                    else if (nextPacketType == PacketType.Content)
                    {
                        if (nextContentType == MessageContentType.Error)
                        {
                            // TODO implement behaviour for incoming errors. Incoming from nowhere. omg.
                        }
                        else if (nextContentType == MessageContentType.Chat)
                        {
                            if (OnChatMessageReceived != null)
                            {
                                var msg = JsonLowercaseSerializer.DeserializeObject<ChatMessage>(jsonLine);
                                OnChatMessageReceived.Invoke(msg);
                            }
                        }
                        else if (nextContentType == MessageContentType.GamePlayerJoined)
                        {
                            if (OnOtherPlayerJoined != null)
                            {
                                var msg = JObject.Parse(jsonLine);
                                string username = msg.Value<string>("username");
                                string datetimeStr = msg.Value<string>("time");

                                OnOtherPlayerJoined.Invoke(username, DateTime.ParseExact(datetimeStr, "H:mm", CultureInfo.InvariantCulture));
                            }
                        }
                        else if (nextContentType == MessageContentType.GamePlayerLeft)
                        {
                            if (OnOtherPlayerLeft != null)
                            {
                                var msg = JObject.Parse(jsonLine);
                                string username = msg.Value<string>("username");
                                string datetimeStr = msg.Value<string>("time");

                                OnOtherPlayerLeft.Invoke(username, DateTime.ParseExact(datetimeStr, "H:mm", CultureInfo.InvariantCulture));
                            }
                        }
                        else if (nextContentType == MessageContentType.GamePlayerKicked)
                        {
                            if (OnOtherPlayerKicked != null)
                            {
                                var msg = JObject.Parse(jsonLine);
                                string username = msg.Value<string>("username");
                                string datetimeStr = msg.Value<string>("time");

                                OnOtherPlayerKicked.Invoke(username, DateTime.ParseExact(datetimeStr, "H:mm", CultureInfo.InvariantCulture));
                            }
                        }
                        else if (nextContentType == MessageContentType.GameStart)
                        {
                            if (OnGameStarted != null)
                            {
                                var map = JObject.Parse(jsonLine).GetValue("map").ToObject<Map>();
                                OnGameStarted.Invoke(map);
                            }
                        }
                        else if (nextContentType == MessageContentType.RoundStart)
                        {
                            var roundInfo = JsonLowercaseSerializer.DeserializeObject<NewRoundInfo>(jsonLine);

                            if (OnRoundStarted == null || !OnRoundStarted.Invoke(roundInfo))
                            {
                                EnqueueIncomingMessage(() =>
                                {
                                    return OnRoundStarted != null && OnRoundStarted.Invoke(roundInfo);
                                });                                
                            }
                        }
                        else if (nextContentType == MessageContentType.RoundEnd)
                        {
                            var simRes = JsonLowercaseSerializer.DeserializeObject<List<SimulationResult>>(jsonLine);

                            if (OnRoundEnded == null || !OnRoundEnded.Invoke(simRes))
                            {
                                EnqueueIncomingMessage(() =>
                                {
                                    return OnRoundEnded != null && OnRoundEnded.Invoke(simRes);
                                });
                            }
                        }
                        else if (nextContentType == MessageContentType.GameEnd)
                        {
                            if (OnGameEnded != null)
                            {
                                // TODO implement end of game!
                                OnGameEnded.Invoke();
                            }
                        }

                        // Next packet will be a header for some another message.
                        nextPacketType = PacketType.Header;
                    }
                    else if (nextPacketType == PacketType.ContentAsResponse)
                    {
                        ErrorType errorType = ErrorType.None;

                        if (nextContentType == MessageContentType.Error)
                        {
                            var jObject = JObject.Parse(jsonLine);
                            string typeStr = Utils.UpperFirstLetter(jObject["errorType"].Value<string>());

                            try
                            {
                                errorType = (ErrorType)Enum.Parse(typeof(ErrorType), typeStr);
                            }
                            catch
                            {
                                errorType = ErrorType.Unknown;
                            }
                        }

                        responseCallback.Invoke(jsonLine, nextContentType, errorType);

                        // Next packet will be a header for some another message.
                        nextPacketType = PacketType.Header;
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                if (ex is IOException || ex is SocketException|| ex is ObjectDisposedException)
                {
                    // Connection may be forcibly closed while waiting for message.
                    // It also could be a kick from the server.
                    // We will just catch it and go away.
                }
                else
                {
                    // Throw unknown exceptions.
                    NLog.LogManager.GetCurrentClassLogger().Fatal(ex);
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

        private void EnqueueIncomingMessage(Func<bool> messageDispatcherFunc)
        {
            lock (_incomingWaitingMessages)
            {
                _incomingWaitingMessages.Add(messageDispatcherFunc);
            }
        }

        private void SendJson(string message)
        {
            networkStreamWriter.WriteLine(message);
            networkStreamWriter.Flush();

            Debug.WriteLine("C: " + message);
        }

        private int GenerateMessageId()
        {
            return Interlocked.Increment(ref lastGeneratedId);
        }

        /// <summary>
        /// Sends request for response of given type. It calls responseCallback when response come.
        /// </summary>
        private void SendRequest(MessageContentType messageContentType, bool waitingForContent, object obj,
            Action<string, MessageContentType, ErrorType> responseCallback)
        {
            // Generate id of message
            int id = GenerateMessageId();

            // Save callback awaiting for response
            messageResponses.Add(id, Tuple.Create(waitingForContent, responseCallback));

            // Send header and content of message
            SendMessage(id, messageContentType, obj);
        }

        /// <summary>
        /// Sends some information to the server and doesn't care about response.
        /// </summary>
        private void SendInfo(MessageContentType messageContentType, object obj)
        {
            // Generate id of message
            int id = GenerateMessageId();

            SendMessage(id, messageContentType, obj);
        }

        /// <summary>
        /// Function used by SendInfo() and SendRequest().
        /// </summary>
        private void SendMessage(int id, MessageContentType messageContentType, object obj)
        {
            // Send header
            string messageContentTypeStr = Utils.LowerFirstLetter(messageContentType.ToString());
            var message = JsonLowercaseSerializer.SerializeObject(new
            {
                Id = id,
                Type = messageContentTypeStr,
                Object = obj
            });

            SendJson(message);
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
            lock (_incomingWaitingMessages)
            {
                _incomingWaitingMessagesToBeRemoved.Clear();

                foreach (var message in _incomingWaitingMessages)
                {
                    if (message.Invoke())
                    {
                        _incomingWaitingMessagesToBeRemoved.Add(message);
                    }
                }

                foreach (var message in _incomingWaitingMessagesToBeRemoved)
                {
                    _incomingWaitingMessages.Remove(message);
                }
            }
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
                logger.Info("Beginning connection to {0}:{1}", hostname, port);

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

        public IAsyncResult BeginLogin(string username, string password, AsyncCallback asyncCallback, object asyncState)
        {
            var ar = new AsyncResult<Player>(asyncCallback, asyncState);

            object requestContent = new
            {
                Username = username,
                Password = password
            };

            SendRequest(MessageContentType.Login, false, requestContent, (jsonStr, messageContentType, errorType) =>
            {
                bool loggedIn = messageContentType == MessageContentType.Ok;
                ar.BeginInvoke(() =>
                {
                    if (loggedIn)
                        return new Player(username);
                    else
                        throw new Exception("Login failed due to the error: " + messageContentType.ToString());
                });
            });

            return ar;
        }

        public Player EndLogin(IAsyncResult asyncResult)
        {
            var ar = (AsyncResult<Player>)asyncResult;
            ar.EndInvoke();

            return ar.Result as Player;
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

            object requestContent = new
            {
                LobbyId = lobbyId
            };

            SendRequest(MessageContentType.GameJoin, true, requestContent, (jsonStr, messageContentType, errorType) =>
            {
                if (messageContentType == MessageContentType.GameInfo)
                {
                    ar.BeginInvoke(() =>
                    {
                        return JsonLowercaseSerializer.DeserializeObject<SpecificGameLobbyInfo>(jsonStr);
                    });
                }
                else
                {
                    string errorMessage = "Couldn't join game: " + messageContentType.ToString();

                    if (errorType == ErrorType.GameFull)
                        errorMessage = "Couldn't join game. Lobby is full.";
                    else if (errorType == ErrorType.GameInvalidId)
                        errorMessage = "Selected game have been closed.";

                    ar.HandleException(new Exception(errorMessage), false);
                }
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

        public IAsyncResult BeginSendChatMessage(string message, AsyncCallback asyncCallback, object asyncState)
        {
            var ar = new AsyncResult<object>(asyncCallback, asyncState);

            var infoContent = new
            {
                Message = message
            };

            SendInfo(MessageContentType.Chat, infoContent);
            ar.Complete(null, false);

            return ar;
        }

        public void EndSendChatMessage(IAsyncResult asyncResult)
        {
            var ar = (AsyncResult<object>)asyncResult;
            ar.EndInvoke();
        }

        public IAsyncResult BeginCreateGame(string gameName, Map map, AsyncCallback asyncCallback, object asyncState)
        {
            var ar = new AsyncResult<bool>(asyncCallback, asyncState);

            var requestContent = new
            {
                Name = gameName,
                Map = map
            };

            SendRequest(MessageContentType.GameCreate, false, requestContent, (jsonStr, messageContentType, errorType) =>
            {
                ar.BeginInvoke(() =>
                {
                    if (messageContentType == MessageContentType.Ok)
                        return true;
                    else if (messageContentType == MessageContentType.Error)
                        throw new Exception("Server Error occured: " + Utils.SplitWordsByCapitals(errorType.ToString()));
                    else
                        throw new Exception("Unknown error!");
                });
            });
            
            return ar;
        }
        public void EndCreateGame(IAsyncResult asyncResult)
        {
            var ar = (AsyncResult<bool>)asyncResult;
            ar.EndInvoke();
        }

        public IAsyncResult BeginKickPlayer(string username, AsyncCallback asyncCallback, object asyncState)
        {
            var ar = new AsyncResult<object>(asyncCallback, asyncState);

            SendInfo(MessageContentType.GamePlayerKick, new { Username = username });
            ar.Complete(null, false);

            return ar;
        }

        public void EndKickPlayer(IAsyncResult asyncResult)
        {
            var ar = (AsyncResult<object>)asyncResult;
            ar.EndInvoke();
        }

        public IAsyncResult BeginStartGame(AsyncCallback asyncCallback, object asyncState)
        {
            var ar = new AsyncResult<bool>(asyncCallback, asyncState);

            SendRequest(MessageContentType.GameStart, false, null, (jsonStr, messageContentType, errorType) =>
            {
                ar.BeginInvoke(() =>
                {
                    if (messageContentType == MessageContentType.GameStarted)
                        return true;
                    else if (messageContentType == MessageContentType.Error)
                        throw new Exception("Couldn't start the game. Error: " + errorType);
                    else
                        throw new Exception("Sorry, we couldn't start the game. The reason is unknown. Please try again.");
                });
            });

            return ar;
        }
        
        public void EndStartGame(IAsyncResult asyncResult)
        {
            var ar = (AsyncResult<bool>)asyncResult;
            ar.EndInvoke();
        }

        public IAsyncResult BeginGetGameList(AsyncCallback asyncCallback, object asyncState)
        {
            var ar = new AsyncResult<List<LobbyListInfo>>(asyncCallback, asyncState);

            SendRequest(MessageContentType.GameList, true, null, (jsonStr, messageContentType, errorType) =>
            {
                if (messageContentType == MessageContentType.GameList)
                {
                    ar.BeginInvoke(() =>
                    {
                        return JsonLowercaseSerializer.DeserializeObject<List<LobbyListInfo>>(jsonStr);
                    });
                }
                else if (messageContentType == MessageContentType.Error)
                {
                    string message = "Waiting for game list received: " + errorType + '.';
                    ar.HandleException(new Exception(message), false);
                }                
            });
            
            return ar;
        }

        public List<LobbyListInfo> EndGetGameList(IAsyncResult asyncResult)
        {
            var ar = (AsyncResult<List<LobbyListInfo>>)asyncResult;
            ar.EndInvoke();

            return ar.Result;
        }

        public IAsyncResult BeginSetReady(AsyncCallback asyncCallback, object asyncState)
        {
            var ar = new AsyncResult<object>(asyncCallback, asyncState);

            SendInfo(MessageContentType.Ready, null);
            ar.Complete(null, true);
            return ar;
        }

        public void EndSetReady(IAsyncResult asyncResult)
        {
            var ar = (AsyncResult<object>)asyncResult;
            ar.EndInvoke();
        }

        public IAsyncResult BeginSendCommands(List<UserCommand> commands, AsyncCallback asyncCallback, object asyncState)
        {
            var ar = new AsyncResult<bool>(asyncCallback, asyncState);
            
            SendRequest(MessageContentType.Commands, false, commands, (jsonStr, messageContentType, errorType) =>
            {
                ar.BeginInvoke(() =>
                {
                    if (messageContentType == MessageContentType.Ok)
                        return true;
                    else
                        throw new Exception("OMG! Error: " + errorType);
                });
            });

            return ar;
        }

        public void EndSendCommands(IAsyncResult asyncResult)
        {
            var ar = (AsyncResult<bool>)asyncResult;
            ar.EndInvoke();
        }

        #endregion

        public GameClient Client { get; protected set; }

        private static Logger logger = LogManager.GetCurrentClassLogger();
    }
}
