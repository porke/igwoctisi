namespace Client.State
{
	using System;
	using System.Configuration;
	using Client.Model;
	using View;
	using View.Menu;
	using Client.View.Play;

	public class MenuState : GameState
    {
        private const string DefaultHost = "localhost";
        private const int DefaultPort = 23456;

        public MenuState(IGWOCTISI game) : base(game)
        {
            var menuBackground = new MenuBackground(this);
            var mainMenu = new MainMenuView(this);
			mainMenu.LoginPressed += RequestLogin;
			mainMenu.QuitPressed += QuitGame;
			mainMenu.EnterPlayStatePressed += EnterPlayState;

            ViewMgr.PushLayer(menuBackground);
			ViewMgr.PushLayer(mainMenu);

			// TODO: lookup for tweaking the stats window
			var stats = new EndgameData();
			stats.Places.Add("asad1");
			stats.Places.Add("asad2");
			stats.Places.Add("asad3");
			stats.Places.Add("asad4");
			stats.Time = 666;
			stats.Stats.Add(new GameStatistic() { Name = "xyz", Values = new System.Collections.Generic.List<int>() { 888, 2, 888, 4, 5, 6, 7, 8 } });
			stats.Stats.Add(new GameStatistic() { Name = "xyza", Values = new System.Collections.Generic.List<int>() { 666, 2, 3, 4, 5, 6, 7, 8 } });
			stats.Stats.Add(new GameStatistic() { Name = "xyzcz", Values = new System.Collections.Generic.List<int>() { 1, 999, 3, 4, 5, 6, 7, 8 } });
			var statsWindow = new GameStats(this, stats, "asad2");
			ViewMgr.PushLayer(statsWindow);
        }

        public override void OnEnter()
        {
            Game.Window.Title = IGWOCTISI.DefaultMainWindowTitle;
            Client.Network.OnDisconnected += new Action<string>(Network_OnDisconnected);
        }

        public override void OnExit()
        {
            Client.Network.OnDisconnected -= new Action<string>(Network_OnDisconnected);
        }

        #region View event handlers

        private void QuitGame(object sender, EventArgs args)
        {
            Client.Network.BeginDisconnect(null, null);
            Client.Exit();
        }
		private void RequestLogin(object sender, EventArgs<string, string> args)
        {
			string login = args.Item1;
			string password = args.Item2;

            int port = DefaultPort;
            string hostname = DefaultHost;

            hostname = ConfigurationManager.AppSettings["hostname"] ?? DefaultHost;
            port = Convert.ToInt32(ConfigurationManager.AppSettings["port"] ?? DefaultPort.ToString());
            
            var messageBox = new MessageBox(this, MessageBoxButtons.None)
            {
                Title = "Log in",
                Message = "Connecting... {0}"
            };

            Client.Network.BeginConnect(hostname, port, OnConnect, Tuple.Create<MessageBox, string, string>(messageBox, login, password));
			ViewMgr.PushLayer(messageBox);
        }
        private void EnterPlayState(object sender, EventArgs<string, string> args)
        {
			string login = args.Item1;
			string password = args.Item2;

            int port = DefaultPort;
            string hostname = DefaultHost;

            hostname = ConfigurationManager.AppSettings["hostname"] ?? DefaultHost;
            port = Convert.ToInt32(ConfigurationManager.AppSettings["port"] ?? DefaultPort.ToString());

            var messageBox = new MessageBox(this, MessageBoxButtons.None)
            {
                Title = "Log in",
                Message = "Connecting..."
            };

            Client.Network.BeginConnect(hostname, port, OnConnect_Debug, Tuple.Create<MessageBox, string, string>(messageBox, login, password));
			ViewMgr.PushLayer(messageBox);
        }
		internal void OnDisconnected(string title, string message)
        {
            var messageBox = new MessageBox(this, MessageBoxButtons.OK)
            {
                Title = title,
                Message = message
            };
			messageBox.OkPressed += (sender, e) => { ViewMgr.PopLayer(); };
			ViewMgr.PushLayer(messageBox);
        }

        #endregion

        #region Async network callbacks

        private void OnConnect(IAsyncResult ar)
        {                        
            InvokeOnMainThread(arg =>
            {
                var network = Client.Network;
                var connectData = (Tuple<MessageBox, string, string>)ar.AsyncState;
                var messageBox = connectData.Item1;
				var username = connectData.Item2;
				var password = connectData.Item3;

                try
                {
                    if (network.EndConnect(ar))
                    {
						network.BeginLogin(username, password, OnLogin, messageBox);
						messageBox.Message = "Logging in as " + username + "...";
                    }
                    else
                    {
                        throw new Exception("Couldn't connect to the server.");
                    }
                }
                catch (Exception exc)
                {
                    messageBox.Message = exc.Message;
                    messageBox.Buttons = MessageBoxButtons.OK;
					messageBox.OkPressed += (sender, e) => ViewMgr.PopLayer();
                }
            });
        }

        private void OnConnect_Debug(IAsyncResult ar)
        {
            InvokeOnMainThread(arg =>
            {
                var network = Client.Network;
                var connectData = (Tuple<MessageBox, string, string>)ar.AsyncState;
                var messageBox = connectData.Item1;
                var username = connectData.Item2;
                var password = connectData.Item3;

                try
                {
                    if (network.EndConnect(ar))
                    {
                        network.BeginLogin(username, password, OnLogin_Debug, messageBox);
                        messageBox.Message = "Logging in as " + username + "...";
                    }
                    else
                    {
                        throw new Exception("Couldn't connect to the server.");
                    }
                }
                catch (Exception exc)
                {
                    messageBox.Message = exc.Message;
                    messageBox.Buttons = MessageBoxButtons.OK;
                    messageBox.OkPressed += (sender, e) => ViewMgr.PopLayer();
                }
            });
        }

        private void OnLogin(IAsyncResult ar)
        {
            InvokeOnMainThread(arg =>
            {
				var network = Client.Network;
                var messageBox = (MessageBox)ar.AsyncState;

                try
                {
                    var player = network.EndLogin(ar);
                    Game.Window.Title = IGWOCTISI.DefaultMainWindowTitle + " @ " + player.Username ;
                    Client.ChangeState(new LobbyState(Game, player));
                }
                catch (Exception exc)
                {
                    network.BeginDisconnect(OnDisconnect, null);
                    messageBox.Message = exc.Message;
                    messageBox.Buttons = MessageBoxButtons.OK;
					messageBox.OkPressed += (sender, e) => ViewMgr.PopLayer();
                }
            });
        }

        private void OnLogin_Debug(IAsyncResult ar)
        {
            InvokeOnMainThread(arg =>
            {
                var network = Client.Network;
                var messageBox = (MessageBox)ar.AsyncState;

                try
                {
                    var player = network.EndLogin(ar);
                    Game.Window.Title = IGWOCTISI.DefaultMainWindowTitle + " @ " + player.Username;

					network.BeginGetGameList(OnGetGameList_Debug, Tuple.Create(messageBox, player));
                }
                catch (Exception exc)
                {
                    network.BeginDisconnect(OnDisconnect, null);
                    messageBox.Message = exc.Message;
                    messageBox.Buttons = MessageBoxButtons.OK;
                    messageBox.OkPressed += (sender, e) => ViewMgr.PopLayer();
                }
            });
        }

		private void OnGetGameList_Debug(IAsyncResult ar)
		{
			InvokeOnMainThread(arg =>
			{
				var network = Client.Network;
				var data = ar.AsyncState as Tuple<MessageBox, Player>;
				var messageBox = data.Item1;
				var player = data.Item2;

				try
				{
					var games = Game.Network.EndGetGameList(ar);

					if (games.Count > 0)
					{
						network.BeginJoinGameLobby(games[0].LobbyId, OnJoinGameLobby_Debug, Tuple.Create(messageBox, player));
						messageBox.Message = "Joining to lobby \"" + games[0].Name + "\"...";
					}
					else
					{
						string mapName = "Hexagon";
						string gameName = "TestGame";
						network.BeginCreateGame(gameName, new Map(mapName), OnCreateGame_Debug, Tuple.Create(messageBox, mapName, gameName, player));
					}
				}
				catch (Exception exc)
				{
					network.BeginDisconnect(OnDisconnect, null);
					messageBox.Message = exc.Message;
					messageBox.Buttons = MessageBoxButtons.OK;
					messageBox.OkPressed += (sender, e) => ViewMgr.PopLayer();
				}
			});
		}

		private void OnJoinGameLobby_Debug(IAsyncResult ar)
		{
			InvokeOnMainThread(arg =>
			{
				var network = Client.Network;
				var data = ar.AsyncState as Tuple<MessageBox, Player>;
				var messageBox = data.Item1;
				var player = data.Item2;

				try
				{
					var lobbyInfo = network.EndJoinGameLobby(ar);

					ViewMgr.PopLayer();     // pop MessageBox
					ViewMgr.PopLayer();     // pop main lobby window

					Game.ChangeState(new LobbyState(Game, player, lobbyInfo, null));
				}
				catch (Exception exc)
				{
					messageBox.Buttons = MessageBoxButtons.OK;
					messageBox.OkPressed += (sender, e) => { ViewMgr.PopLayer(); };
					messageBox.Message = exc.Message;
				}
			});
		}

        private void OnCreateGame_Debug(IAsyncResult ar)
        {
            InvokeOnMainThread(obj =>
            {
				var network = Client.Network;
                var data = ar.AsyncState as Tuple<MessageBox, string, string, Player>;
                var messageBox = data.Item1;
                string mapName = data.Item2;
                string gameName = data.Item3;
                var player = data.Item4;

                try
                {
                    network.EndCreateGame(ar);

                    ViewMgr.PopLayer();     // pop MessageBox
                    ViewMgr.PopLayer();     // pop main lobby window

					Game.ChangeState(new LobbyState(Game, player, new SpecificGameLobbyInfo(gameName, player), mapName));

					// OBSOLETE:
                    //var map = new Map(mapName);
                    //network.BeginStartGame(OnGameStarted_Debug, Tuple.Create(messageBox, map, player));
                }
                catch (Exception exc)
                {
                    messageBox.Buttons = MessageBoxButtons.OK;
                    messageBox.OkPressed += (sender, e) => { ViewMgr.PopLayer(); };
                    messageBox.Message = exc.Message;
                }
            });
        }

        private void OnDisconnect(IAsyncResult ar)
        {
			var network = Client.Network;
            network.EndDisconnect(ar);
        }

        private void OnGameStarted_Debug(IAsyncResult result)
        {
            InvokeOnMainThread(obj =>
            {
				var network = Client.Network;
                var data = result.AsyncState as Tuple<MessageBox, Map, Player>;
                var messageBox = data.Item1;
                var map = data.Item2;
                var player = data.Item3;

                try
                {
                    network.EndStartGame(result);

                    Game.ChangeState(new PlayState(Game, map, player));
                }
                catch (Exception exc)
                {
                    messageBox.Buttons = MessageBoxButtons.OK;
                    messageBox.OkPressed += (sender, e) => { ViewMgr.PopLayer(); ViewMgr.PopLayer(); };
                    messageBox.Message = exc.Message;
                }
            });
        }

        private void Network_OnDisconnected(string reason)
        {
            InvokeOnMainThread(arg =>
            {
				ViewMgr.PopLayer(); // Probably "Logging in..." MessageBox
				OnDisconnected("Disconnection", "You were forcefully kicked out by the server.");
            }, reason);
        }

        #endregion
    }
}
