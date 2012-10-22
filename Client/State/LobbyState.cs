namespace Client.State
{
    using System;
    using Client.Model;
    using View;
    using View.Lobby;
    using System.Collections.Generic;
    using System.Linq;

    class LobbyState : GameState
    {
        private Map _map;
        private Player _clientPlayer;
        private SpecificGameLobbyInfo _gameLobby;
        private bool _isHostingGame { get { return _map != null; } }

        public LobbyState(IGWOCTISI game, Player player) : base(game)
        {
            _clientPlayer = player;

            var menuBackground = new LobbyBackground(this);
            var lobbyMenu = new MainLobbyView(this);

            ViewMgr.PushLayer(menuBackground);
            ViewMgr.PushLayer(lobbyMenu);

            eventHandlers.Add("LeaveGameLobby", LeaveGameLobby);
            eventHandlers.Add("CreateGame", CreateGame);
            eventHandlers.Add("CancelCreateGame", CancelCreateGame);
            eventHandlers.Add("EnterCreateGameView", EnterCreateGameView);
            eventHandlers.Add("Logout", Logout);
            eventHandlers.Add("JoinGame", JoinGame);
            eventHandlers.Add("BeginGame", BeginGame);
            eventHandlers.Add("RefreshGameList", RefreshGameList);
            eventHandlers.Add("SendChatMessage", SendChatMessage);
            eventHandlers.Add("KickOtherPlayer", KickOtherPlayer);
        }

        public override void OnEnter()
        {
            Client.Network.OnDisconnected += new Action<string>(OnDisconnected_EventHandler);
            this.RefreshGameList(new SenderEventArgs(ViewMgr.PeekLayer()));
        }

        public override void OnExit()
        {
            Client.Network.OnDisconnected -= new Action<string>(OnDisconnected_EventHandler);
        }

        private void BindNetworkEvents()
        {
            Client.Network.OnOtherPlayerJoined += Network_OnOtherPlayerJoined;
            Client.Network.OnOtherPlayerLeft += Network_OnOtherPlayerLeft;
            Client.Network.OnOtherPlayerKicked += Network_OnOtherPlayerKicked;
            Client.Network.OnChatMessageReceived += Network_OnChatMessageReceived;
            Client.Network.OnPlayerKicked += Network_OnPlayerKicked;

            if (!_isHostingGame)
            {
                Client.Network.OnGameStarted += Network_OnGameStarted;
            }
        }

        private void UnbindNetworkEvents()
        {
            Client.Network.OnOtherPlayerJoined -= Network_OnOtherPlayerJoined;
            Client.Network.OnOtherPlayerLeft -= Network_OnOtherPlayerLeft;
            Client.Network.OnOtherPlayerKicked -= Network_OnOtherPlayerKicked;
            Client.Network.OnChatMessageReceived -= Network_OnChatMessageReceived;
            Client.Network.OnPlayerKicked -= Network_OnPlayerKicked;

            if (!_isHostingGame)
            {
                Client.Network.OnGameStarted -= Network_OnGameStarted;
            }
        }

        #region View event handlers

        private void LeaveGameLobby(EventArgs args)
        {
            Client.Network.BeginLeaveGame(OnLeaveGame, null);
        }

        private void CreateGame(EventArgs args)
        {
            var newGameParameters = args as CreateGameArgs;

            var messageBox = new MessageBox(MessageBoxButtons.None)
            {
                Title = "Join Game",
                Message = "Joining in..."
            };
            ViewMgr.PushLayer(messageBox);

            var map = new Map(newGameParameters.MapName);
            Client.Network.BeginCreateGame(newGameParameters.GameName, map, OnCreateGame, Tuple.Create(messageBox, newGameParameters.MapName, newGameParameters.GameName));
        }

        private void CancelCreateGame(EventArgs args)
        {
            ViewMgr.PopLayer();     // pop create game view
            ViewMgr.PushLayer(new MainLobbyView(this));
        }

        private void EnterCreateGameView(EventArgs args)
        {
            ViewMgr.PopLayer();     // pop main lobby view
            ViewMgr.PushLayer(new CreateGameView(this));
        }

        private void Logout(EventArgs args)
        {
            var ar = Client.Network.BeginLogout(OnLogout, null);
            ar.AsyncWaitHandle.WaitOne();
        }

        private void JoinGame(EventArgs args)
        {
            var joinGameArgs = args as JoinGameArgs;

            var messageBox = new MessageBox(MessageBoxButtons.None)
            {
                Title = "Join Game",
                Message = "Joining in..."
            };
            ViewMgr.PushLayer(messageBox);
            Client.Network.BeginJoinGameLobby(joinGameArgs.LobbyId, OnJoinLobby, messageBox);
        }

        private void BeginGame(EventArgs args)
        {
            var messageBox = new MessageBox(MessageBoxButtons.None)
            {
                Title = "Begin Game",
                Message = "Starting game, please wait..."
            };
            ViewMgr.PushLayer(messageBox);

            Client.Network.BeginStartGame(OnGameStarted, messageBox);
        }

        private void RefreshGameList(EventArgs args)
        {
            var messageBox = new MessageBox(MessageBoxButtons.None)
            {
                Title = "Loading Main Lobby",
                Message = "Downloading game list..."
            };            

            ViewMgr.PushLayer(messageBox);

            var mainLobbyView = (args as SenderEventArgs).Sender;
            Client.Network.BeginGetGameList(OnGetGameList, Tuple.Create(mainLobbyView as MainLobbyView, messageBox));
        }

        private void SendChatMessage(EventArgs args)
        {
            var msgArgs = args as ChatMessageArgs;
            Client.Network.BeginSendChatMessage(msgArgs.Message, (res) => { try { Client.Network.EndSendChatMessage(res); } catch { } }, null);
        }

        private void KickOtherPlayer(EventArgs args)
        {
            var kickArgs = args as KickPlayerArgs;
            Client.Network.BeginKickPlayer(kickArgs.Username, OnKickPlayer, kickArgs.Username);
        }

        #endregion

        #region Network async callbacks

        private void OnLogout(IAsyncResult result)
        {
            InvokeOnMainThread(obj =>
            {
                try
                {
                    Client.Network.EndLogout(result);
                }
                catch { }
                finally
                {
                    Client.Network.BeginDisconnect(OnDisconnect, null);
                }
            });
        }
                
        private void OnGetGameList(IAsyncResult result)
        {
            var data = (Tuple<MainLobbyView, MessageBox>)result.AsyncState;
            var mainLobbyView = data.Item1;
            var messageBox = data.Item2;

            InvokeOnMainThread(obj =>
            {
                try
                {
                    var gameNames = Client.Network.EndGetGameList(result);
                    mainLobbyView.RefreshGameList(gameNames);
                    ViewMgr.PopLayer(); // MessageBox
                }
                catch (Exception exc)
                {
                    messageBox.Buttons = MessageBoxButtons.OK;
                    messageBox.Message = exc.Message;
                    messageBox.OkPressed += (sender, e) => { ViewMgr.PopLayer(); };
                }
            });
        }

        private void OnJoinLobby(IAsyncResult result)
        {
            var messageBox = result.AsyncState as MessageBox;
            BindNetworkEvents();
            
            InvokeOnMainThread(obj =>
            {
                try
                {
                    _gameLobby = Client.Network.EndJoinGameLobby(result);                     

                    ViewMgr.PopLayer(); // MessageBox
                    ViewMgr.PopLayer(); // MainLobbyView

                    var gameLobbyView = new GameLobbyView(this, false);
                    gameLobbyView.RefreshPlayerList(_gameLobby.Players, _gameLobby.HostName, _clientPlayer.Username);
                    ViewMgr.PushLayer(gameLobbyView);
                }
                catch (Exception exc)
                {
                    messageBox.Buttons = MessageBoxButtons.OK;
                    messageBox.Message = exc.Message;
                    messageBox.OkPressed += (sender, e) => { ViewMgr.PopLayer(); };
                }
            });
        }

        private void OnLeaveGame(IAsyncResult result)
        {
            try
            {
                Client.Network.EndLeaveGame(result);
                _gameLobby = null;

                UnbindNetworkEvents();
            }
            catch { }
            finally
            {
                InvokeOnMainThread(arg =>
                {
                    ViewMgr.PopLayer(); // pop game lobby
                    ViewMgr.PushLayer(new MainLobbyView(this));
                });
            }
        }

        private void OnKickPlayer(IAsyncResult result)
        {
            try
            {
                Client.Network.EndKickPlayer(result);
                // Don't remove the player manually from lists.
                // Server should send GamePlayerLeft message to anyone.
            }
            catch
            {
            }
        }

        private void OnCreateGame(IAsyncResult result)
        {
            BindNetworkEvents();

            InvokeOnMainThread(obj =>
            {
                var data = result.AsyncState as Tuple<MessageBox, string, string>;
                var messageBox = data.Item1;
                string mapName = data.Item2;
                string gameName = data.Item3;

                try
                {
                    Client.Network.EndCreateGame(result);

                    _map = new Map(mapName);
                    _gameLobby = new SpecificGameLobbyInfo(gameName, _clientPlayer);

                    ViewMgr.PopLayer();     // pop MessageBox
                    ViewMgr.PopLayer();     // pop main lobby window

                    var gameLobbyView = new GameLobbyView(this, true);
                    gameLobbyView.RefreshPlayerList(_gameLobby.Players, _gameLobby.HostName, _clientPlayer.Username);
                    ViewMgr.PushLayer(gameLobbyView);
                }
                catch (Exception exc)
                {
                    messageBox.Buttons = MessageBoxButtons.OK;
                    messageBox.OkPressed += (sender, e) => { ViewMgr.PopLayer(); };
                    messageBox.Message = exc.Message;
                }
            });            
        }

        private void OnGameStarted(IAsyncResult result)
        {
            InvokeOnMainThread(obj =>
            {
                var messageBox = result.AsyncState as MessageBox;

                try
                {
                    Client.Network.EndStartGame(result);
                }
                catch (Exception exc)
                {
                    messageBox.Buttons = MessageBoxButtons.OK;
                    messageBox.OkPressed += (sender, e) => { ViewMgr.PopLayer(); ViewMgr.PopLayer(); };
                    messageBox.Message = exc.Message;
                }
            });
        }

        private void OnDisconnect(IAsyncResult result)
        {
            try
            {
                Client.Network.EndDisconnect(result);
            }
            catch { }
            finally { }
        }

        private void OnDisconnected_EventHandler(string reason)
        {
            InvokeOnMainThread(obj =>
            {
                UnbindNetworkEvents();

                var menuState = new MenuState(Game);
                Client.ChangeState(menuState);
                menuState.HandleViewEvent("OnDisconnected", new MessageBoxArgs("Disconnection", "You were disconnected from the server."));
            });
        }

        #endregion

        #region Network event handlers

        private void Network_OnOtherPlayerJoined(string username, DateTime time)
        {
            InvokeOnMainThread(obj =>
            {
                var gameLobbyView = ViewMgr.PeekLayer() as GameLobbyView;
                _gameLobby.AddPlayer(username);
                gameLobbyView.RefreshPlayerList(_gameLobby.Players, _gameLobby.HostName, _clientPlayer.Username);
                gameLobbyView.AddHostMessage(username + " joined.", time.ToString("H:mm"));
            });
        }

        private void Network_OnOtherPlayerLeft(string username, DateTime time)
        {
            InvokeOnMainThread(obj =>
            {
                var gameLobbyView = ViewMgr.PeekLayer() as GameLobbyView;
                _gameLobby.RemovePlayer(username);
                gameLobbyView.RefreshPlayerList(_gameLobby.Players, _gameLobby.HostName, _clientPlayer.Username);
                gameLobbyView.AddHostMessage(username + " left.", time.ToString("H:mm"));
            });
        }

        private void Network_OnOtherPlayerKicked(string username, DateTime time)
        {
            InvokeOnMainThread(obj =>
            {
                var gameLobbyView = ViewMgr.PeekLayer() as GameLobbyView;
                _gameLobby.RemovePlayer(username);
                gameLobbyView.RefreshPlayerList(_gameLobby.Players, _gameLobby.HostName, _clientPlayer.Username);
                gameLobbyView.AddHostMessage(username + " have been kicked by host.", time.ToString("H:mm"));
            });
        }

        private void Network_OnChatMessageReceived(ChatMessage message)
        {
            InvokeOnMainThread(obj =>
            {
                var gameLobbyView = ViewMgr.PeekLayer() as GameLobbyView;
                gameLobbyView.ChatMessageReceived(obj as ChatMessage);
            }, message);
        }
        
        void Network_OnPlayerKicked()
        {
            InvokeOnMainThread(obj =>
            {
                UnbindNetworkEvents();

                ViewMgr.PopLayer(); // GameLobbyView
                ViewMgr.PushLayer(new MainLobbyView(this));

                var messageBox = new MessageBox(MessageBoxButtons.OK)
                {
                    Title = "Out of Game",
                    Message = "You were kicked out from Game Lobby."
                };
                messageBox.OkPressed += (sender, e) => { ViewMgr.PopLayer(); };
                ViewMgr.PushLayer(messageBox);
            });
        }

        void Network_OnGameStarted(Map map)
        {
            InvokeOnMainThread(obj =>
            {
                UnbindNetworkEvents();

                var playerList = new List<Player>(_gameLobby.Players.Select(username => new Player(username)));
                Game.ChangeState(new PlayState(Game, _map, _clientPlayer, playerList));
            });
        }

        #endregion
    }
}
