namespace Client.State
{
    using System;
    using Client.Model;
    using View;
    using View.Lobby;
	using Common;

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
        }
		
		public LobbyState(IGWOCTISI game, Player player, SpecificGameLobbyInfo lobbyInfo, string mapName) : base(game)
		{
			_clientPlayer = player;
			_gameLobby = lobbyInfo;
			_map = mapName != null ? new Map(mapName) : null;

			var gameLobbyView = new GameLobbyView(this, mapName != null);
			var menuBackground = new LobbyBackground(this);

			BindNetworkEvents();
			gameLobbyView.RefreshPlayerList(_gameLobby.Players, _gameLobby.HostName, _clientPlayer.Username);
			ViewMgr.PushLayer(menuBackground);
			ViewMgr.PushLayer(gameLobbyView);
		}

        public override void OnEnter()
        {
            Client.Network.OnDisconnected += new Action<string>(OnDisconnected_EventHandler);
			RefreshGameList(ViewMgr.PeekLayer());
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

        internal void LeaveGameLobby()
        {
            Client.Network.BeginLeaveGame(OnLeaveGame, null);
        }
		internal void CreateGame(string gameName, string mapName)
        {
            var messageBox = new MessageBox(this, MessageBoxButtons.None)
            {
                Title = "Join Game",
                Message = "Joining in..."
            };
			ViewMgr.PushLayer(messageBox);

            var map = new Map(mapName);
            Client.Network.BeginCreateGame(gameName, map, OnCreateGame, Tuple.Create(messageBox, mapName, gameName));
        }
		internal void CancelCreateGame()
        {
			ViewMgr.PopLayer();     // pop create game view
			ViewMgr.PushLayer(new MainLobbyView(this));
        }
		internal void EnterCreateGameView()
        {
			ViewMgr.PopLayer();     // pop main lobby view
			ViewMgr.PushLayer(new CreateGameView(this));
        }
		internal void Logout()
        {
            var ar = Client.Network.BeginLogout(OnLogout, null);
            ar.AsyncWaitHandle.WaitOne();
        }
		internal void JoinGame(int lobbyId)
        {
            var messageBox = new MessageBox(this, MessageBoxButtons.None)
            {
                Title = "Join Game",
                Message = "Joining in..."
            };
			ViewMgr.PushLayer(messageBox);
            Client.Network.BeginJoinGameLobby(lobbyId, OnJoinLobby, messageBox);
        }
		internal void BeginGame()
        {
            var messageBox = new MessageBox(this, MessageBoxButtons.None)
            {
                Title = "Begin Game",
                Message = "Starting game, please wait..."
            };
			ViewMgr.PushLayer(messageBox);

            Client.Network.BeginStartGame(OnGameStarted, messageBox);
        }
		internal void RefreshGameList(BaseView sender)
        {
            var messageBox = new MessageBox(this, MessageBoxButtons.None)
            {
                Title = "Loading Main Lobby",
                Message = "Downloading game list..."
            };

			ViewMgr.PushLayer(messageBox);

            var mainLobbyView = sender;
            Client.Network.BeginGetGameList(OnGetGameList, Tuple.Create(mainLobbyView as MainLobbyView, messageBox));
        }
		internal void SendChatMessage(string message)
        {
			Client.Network.BeginSendChatMessage(message, (res) => { try { Client.Network.EndSendChatMessage(res); } catch { } }, null);
        }
		internal void KickOtherPlayer(string username)
        {
            Client.Network.BeginKickPlayer(username, OnKickPlayer, username);
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
                    
                    Game.ChangeState(new PlayState(Game, _map, _clientPlayer));
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
				menuState.OnDisconnected("Disconnection", "You were disconnected from the server.");
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

                var messageBox = new MessageBox(this, MessageBoxButtons.OK)
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
                Game.ChangeState(new PlayState(Game, map, _clientPlayer));
            });
        }

        #endregion
    }
}
