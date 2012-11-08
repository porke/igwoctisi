namespace Client.State
{
    using System;
    using System.Configuration;
    using View;
    using View.Menu;
    using Client.Model;

    public class MenuState : GameState
    {
        private const string DefaultHost = "localhost";
        private const int DefaultPort = 23456;

        public MenuState(IGWOCTISI game) : base(game)
        {
            var menuBackground = new MenuBackground(this);
            var mainMenu = new MainMenuView(this);

            Client.ViewMgr.PushLayer(menuBackground);
			Client.ViewMgr.PushLayer(mainMenu);
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

        internal void QuitGame()
        {
            Client.Network.BeginDisconnect(null, null);
            Client.Exit();
        }
		internal void RequestLogin(string login, string password)
        {
            int port = DefaultPort;
            string hostname = DefaultHost;

            hostname = ConfigurationManager.AppSettings["hostname"] ?? DefaultHost;
            port = Convert.ToInt32(ConfigurationManager.AppSettings["port"] ?? DefaultPort.ToString());
            
            var messageBox = new MessageBox(this, MessageBoxButtons.None)
            {
                Title = "Log in",
                Message = string.Format("Connecting... {0}@{1}", login, password)
            };

            Client.Network.BeginConnect(hostname, port, OnConnect, Tuple.Create<MessageBox, string, string>(messageBox, login, password));
			Client.ViewMgr.PushLayer(messageBox);
        }
        internal void EnterPlayState(string login, string password)
        {
            int port = DefaultPort;
            string hostname = DefaultHost;

            hostname = ConfigurationManager.AppSettings["hostname"] ?? DefaultHost;
            port = Convert.ToInt32(ConfigurationManager.AppSettings["port"] ?? DefaultPort.ToString());

            var messageBox = new MessageBox(this, MessageBoxButtons.None)
            {
                Title = "Log in",
                Message = string.Format("Connecting... {0}@{1}", login, password)
            };

            Client.Network.BeginConnect(hostname, port, OnConnect_Debug, Tuple.Create<MessageBox, string, string>(messageBox, login, password));
            Client.ViewMgr.PushLayer(messageBox);
        }
		internal void OnDisconnected(string title, string message)
        {
            var messageBox = new MessageBox(this, MessageBoxButtons.OK)
            {
                Title = title,
                Message = message
            };
			messageBox.OkPressed += (sender, e) => { Client.ViewMgr.PopLayer(); };
			Client.ViewMgr.PushLayer(messageBox);
        }

        #endregion

        #region Async network callbacks

        private void OnConnect(IAsyncResult ar)
        {                        
            InvokeOnMainThread(arg =>
            {
				var network = Client.ViewMgr.Client.Network;
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
					messageBox.OkPressed += (sender, e) => Client.ViewMgr.PopLayer();
                }
            });
        }

        private void OnConnect_Debug(IAsyncResult ar)
        {
            InvokeOnMainThread(arg =>
            {
                var network = Client.ViewMgr.Client.Network;
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
                    messageBox.OkPressed += (sender, e) => Client.ViewMgr.PopLayer();
                }
            });
        }

        private void OnLogin(IAsyncResult ar)
        {
            InvokeOnMainThread(arg =>
            {
				var network = Client.ViewMgr.Client.Network;
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
					messageBox.OkPressed += (sender, e) => Client.ViewMgr.PopLayer();
                }
            });
        }

        private void OnLogin_Debug(IAsyncResult ar)
        {
            InvokeOnMainThread(arg =>
            {
                var network = Client.ViewMgr.Client.Network;
                var messageBox = (MessageBox)ar.AsyncState;

                try
                {
                    var player = network.EndLogin(ar);
                    Game.Window.Title = IGWOCTISI.DefaultMainWindowTitle + " @ " + player.Username;

                    // TODO connect to the existing host if it exists
                    string mapName = "TestMap";
                    string gameName = "TestGame";
                    Game.Network.BeginCreateGame(gameName, new Map(mapName), OnCreateGame_Debug, Tuple.Create(messageBox, mapName, gameName, player));

                }
                catch (Exception exc)
                {
                    network.BeginDisconnect(OnDisconnect, null);
                    messageBox.Message = exc.Message;
                    messageBox.Buttons = MessageBoxButtons.OK;
                    messageBox.OkPressed += (sender, e) => Client.ViewMgr.PopLayer();
                }
            });
        }

        private void OnCreateGame_Debug(IAsyncResult result)
        {
            InvokeOnMainThread(obj =>
            {
                var data = result.AsyncState as Tuple<MessageBox, string, string, Player>;
                var messageBox = data.Item1;
                string mapName = data.Item2;
                string gameName = data.Item3;
                var player = data.Item4;

                try
                {
                    Client.Network.EndCreateGame(result);

                    Client.ViewMgr.PopLayer();     // pop MessageBox
                    Client.ViewMgr.PopLayer();     // pop main lobby window

                    var map = new Map(mapName);
                    Game.Network.BeginStartGame(OnGameStarted_Debug, Tuple.Create(messageBox, map, player));
                }
                catch (Exception exc)
                {
                    messageBox.Buttons = MessageBoxButtons.OK;
                    messageBox.OkPressed += (sender, e) => { Client.ViewMgr.PopLayer(); };
                    messageBox.Message = exc.Message;
                }
            });
        }

        private void OnDisconnect(IAsyncResult ar)
        {
			var network = Client.ViewMgr.Client.Network;
            network.EndDisconnect(ar);
        }

        private void OnGameStarted_Debug(IAsyncResult result)
        {
            InvokeOnMainThread(obj =>
            {
                var data = result.AsyncState as Tuple<MessageBox, Map, Player>;
                var messageBox = data.Item1;
                var map = data.Item2;
                var player = data.Item3;

                try
                {
                    Client.Network.EndStartGame(result);

                    Game.ChangeState(new PlayState(Game, map, player));
                }
                catch (Exception exc)
                {
                    messageBox.Buttons = MessageBoxButtons.OK;
                    messageBox.OkPressed += (sender, e) => { Client.ViewMgr.PopLayer(); Client.ViewMgr.PopLayer(); };
                    messageBox.Message = exc.Message;
                }
            });
        }

        private void Network_OnDisconnected(string reason)
        {
            InvokeOnMainThread(arg =>
            {
				Client.ViewMgr.PopLayer(); // Probably "Logging in..." MessageBox
				OnDisconnected("Disconnection", "You were forcefully kicked out by the server.");
            }, reason);
        }

        #endregion
    }
}
