namespace Client.State
{
    using System;
    using System.Configuration;
    using View;
    using View.Menu;

    public class MenuState : GameState
    {
        private const string DefaultHost = "localhost";
        private const int DefaultPort = 23456;

        public MenuState(IGWOCTISI game) : base(game)
        {
            var menuBackground = new MenuBackground(this);
            var mainMenu = new MainMenuView(this);

            ViewMgr.PushLayer(menuBackground);
            ViewMgr.PushLayer(mainMenu);
        }

        public override void OnEnter()
        {
            Game.Window.Title = IGWOCTISI.DefaultMainWindowTitle;
            Client.Network.OnDisconnected += new Action<string>(OnDisconnected_EventHandler);
        }

        public override void OnExit()
        {
            Client.Network.OnDisconnected -= new Action<string>(OnDisconnected_EventHandler);
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

            try
            {
                hostname = ConfigurationManager.AppSettings["hostname"] ?? DefaultHost;
                port = Convert.ToInt32(ConfigurationManager.AppSettings["port"] ?? DefaultPort.ToString());
            }
            catch
            {
                // TODO: Log any error messages
            }
            
            var messageBox = new MessageBox(MessageBoxButtons.None)
            {
                Title = "Log in",
                Message = string.Format("Connecting... {0}@{1}", login, password)
            };

            Client.Network.BeginConnect(hostname, port, OnConnect, Tuple.Create<MessageBox, string, string>(messageBox, login, password));
            ViewMgr.PushLayer(messageBox);
        }
		internal void OnDisconnected(string title, string message)
        {
            var messageBox = new MessageBox(MessageBoxButtons.OK)
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
                var network = ViewMgr.Client.Network;
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
            }, ar.AsyncState);
        }

        private void OnLogin(IAsyncResult ar)
        {
            InvokeOnMainThread(arg =>
            {
                var network = ViewMgr.Client.Network;
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

        private void OnDisconnect(IAsyncResult ar)
        {
            var network = ViewMgr.Client.Network;
            network.EndDisconnect(ar);
        }

        private void OnDisconnected_EventHandler(string reason)
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
