namespace Client.State
{
	using System;
	using Client.Model;
	using Client.View.Menu;
	using View;
	using View.Lobby;

	class LobbyState : GameState
    {
        private Map _map;
        private Player _clientPlayer;
        private SpecificGameLobbyInfo _gameLobby;
        private bool _isHostingGame { get { return _map != null; } }

        public LobbyState(IGWOCTISI game, Player player) : base(game)
        {
            _clientPlayer = player;

            var menuBackground = new MenuBackground(this);
			ViewMgr.PushLayer(menuBackground);
			EnterMainLobbyView();
        }
		
		public LobbyState(IGWOCTISI game, Player player, SpecificGameLobbyInfo lobbyInfo, string mapName) : base(game)
		{
			_clientPlayer = player;
			_gameLobby = lobbyInfo;
			_map = mapName != null ? new Map(mapName) : null;

			var menuBackground = new LobbyBackground(this);

			BindNetworkEvents();
			ViewMgr.PushLayer(menuBackground);
			EnterGameLobbyView(_map != null);
		}

        public override void OnEnter()
        {
            Client.Network.OnDisconnected += new Action<string>(OnDisconnected_EventHandler);
			MainLobby_RefreshGameList(ViewMgr.PeekLayer(), EventArgs.Empty);
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

        private void GameLobby_Leave(object sender, EventArgs e)
        {
            Client.Network.BeginLeaveGame(Response_LeaveGame, null);
        }
		private void CreateGame_Confirm(object sender, EventArgs<string, string> e)
        {
			string gameName = e.Item1;
			string mapName = e.Item2;

            var messageBox = new MessageBox(this, MessageBoxButtons.None)
            {
                Title = "Join Game",
                Message = "Joining in..."
            };
			ViewMgr.PushLayer(messageBox);

            var map = new Map(mapName);
            Client.Network.BeginCreateGame(gameName, map, Response_CreateGame, Tuple.Create(messageBox, mapName, gameName));
        }
		private void CreateGame_Cancel(object sender, EventArgs e)
        {
			ViewMgr.PopLayer();     // pop create game view
			EnterMainLobbyView();
        }
		private void MainLobby_ShowCreateGameView(object sender, EventArgs e)
        {
			ViewMgr.PopLayer();     // pop main lobby view
			EnterCreateGameView();
        }
		private void MainLobby_Logout(object sender, EventArgs e)
        {
            var ar = Client.Network.BeginLogout(Response_Logout, null);
            ar.AsyncWaitHandle.WaitOne();
        }
		private void MainLobby_JoinGame(object sender, EventArgs<int> args)
        {
			int lobbyId = args.Item;
            var messageBox = new MessageBox(this, MessageBoxButtons.None)
            {
                Title = "Join Game",
                Message = "Joining in..."
            };
			ViewMgr.PushLayer(messageBox);
            Client.Network.BeginJoinGameLobby(lobbyId, Response_JoinLobby, messageBox);
        }
		private void GameLobby_BeginGame(object sender, EventArgs e)
        {
            var messageBox = new MessageBox(this, MessageBoxButtons.None)
            {
                Title = "Begin Game",
                Message = "Starting game, please wait..."
            };
			ViewMgr.PushLayer(messageBox);

            Client.Network.BeginStartGame(Response_GameStarted, messageBox);
        }
		private void MainLobby_RefreshGameList(object sender, EventArgs e)
        {
			var senderView = sender as BaseView;
            var messageBox = new MessageBox(this, MessageBoxButtons.None)
            {
                Title = "Loading Main Lobby",
                Message = "Downloading game list..."
            };

			ViewMgr.PushLayer(messageBox);

			var mainLobbyView = senderView;
            Client.Network.BeginGetGameList(Response_GetGameList, Tuple.Create(mainLobbyView as MainLobbyView, messageBox));
        }
		private void GameLobby_SendChatMessage(object sender, EventArgs<string> e)
        {
			string message = e.Item;
			Client.Network.BeginSendChatMessage(message, (res) => { try { Client.Network.EndSendChatMessage(res); } catch { } }, null);
        }
		private void GameLobby_KickOtherPlayer(object sender, EventArgs<string> e)
        {
			string username = e.Item;
            Client.Network.BeginKickPlayer(username, Response_KickPlayer, username);
        }

        #endregion

        #region Network responses

        private void Response_Logout(IAsyncResult result)
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
                    Client.Network.BeginDisconnect(Response_Disconnect, null);
                }
            });
        }
                
        private void Response_GetGameList(IAsyncResult result)
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

        private void Response_JoinLobby(IAsyncResult result)
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
					EnterGameLobbyView(false);
                }
                catch (Exception exc)
                {
                    messageBox.Buttons = MessageBoxButtons.OK;
                    messageBox.Message = exc.Message;
					messageBox.OkPressed += (sender, e) => { ViewMgr.PopLayer(); };
                }
            });
        }

        private void Response_LeaveGame(IAsyncResult result)
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
					EnterMainLobbyView();
                });
            }
        }

        private void Response_KickPlayer(IAsyncResult result)
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

        private void Response_CreateGame(IAsyncResult result)
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
					EnterGameLobbyView(true);
                }
                catch (Exception exc)
                {
                    messageBox.Buttons = MessageBoxButtons.OK;
					messageBox.OkPressed += (sender, e) => { ViewMgr.PopLayer(); };
                    messageBox.Message = exc.Message;
                }
            });            
        }

        private void Response_GameStarted(IAsyncResult result)
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

        private void Response_Disconnect(IAsyncResult result)
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
				EnterMainLobbyView();

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

		#region View enter functions (event binding)

		private void EnterMainLobbyView()
		{
			var lobbyMenu = new MainLobbyView(this);
			ViewMgr.PushLayer(lobbyMenu);
			lobbyMenu.JoinGamePressed += MainLobby_JoinGame;
			lobbyMenu.LogoutPressed += MainLobby_Logout;
			lobbyMenu.RefreshPressed += MainLobby_RefreshGameList;
			lobbyMenu.CreateGamePressed += MainLobby_ShowCreateGameView;
		}
		private void EnterCreateGameView()
		{
			var createGameView = new CreateGameView(this);
			createGameView.CreateGameCancelled += CreateGame_Cancel;
			createGameView.CreateGameConfirmed += CreateGame_Confirm;
			ViewMgr.PushLayer(createGameView);
		}
		private void EnterGameLobbyView(bool isHost)
		{
			var gameLobbyView = new GameLobbyView(this, isHost);
			gameLobbyView.ChatMessageSent += GameLobby_SendChatMessage;
			gameLobbyView.BeginGamePressed += GameLobby_BeginGame;
			gameLobbyView.GameLeavePressed += GameLobby_Leave;
			gameLobbyView.PlayerKicked += GameLobby_KickOtherPlayer;
			gameLobbyView.RefreshPlayerList(_gameLobby.Players, _gameLobby.HostName, _clientPlayer.Username);
			ViewMgr.PushLayer(gameLobbyView);
		}

		#endregion
	}
}
