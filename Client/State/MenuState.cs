namespace Client.State
{
    using View;
    using View.Menu;
    using System;
    using Client.View.Lobby;

    public class MenuState : GameState
    {
        public MenuState(IGWOCTISI game) : base(game)
        {
            var menuBackground = new MenuBackground(this);
            var mainMenu = new MainMenuView(this);

            ViewMgr.PushLayer(menuBackground);
            ViewMgr.PushLayer(mainMenu);

            eventHandlers.Add("QuitGame", QuitGame);
            eventHandlers.Add("RequestLogin", RequestLogin);
            eventHandlers.Add("OnDisconnected", OnDisconnected);
        }

        public override void OnEnter()
        {
            Client.Network.OnDisconnected += new Action<string>(OnDisconnected_EventHandler);
        }

        public override void OnExit()
        {
            Client.Network.OnDisconnected -= new Action<string>(OnDisconnected_EventHandler);
        }

        #region View event handlers

        private void QuitGame(EventArgs args)
        {
            Client.Network.BeginDisconnect(null, null);
            Client.Exit();
        }

        private void RequestLogin(EventArgs args)
        {
            var network = Client.Network;
            string hostname = "localhost";// "178.32.225.209";// "v.zloichuj.eu";
            int port = 23456;
            var loginData = args as LoginEventArgs;

            var messageBox = new MessageBox(MessageBoxButtons.None)
            {
                Title = "Log in",
                Message = string.Format("Connecting... {0}@{1}", loginData.Login, loginData.Password)
            };

            network.BeginConnect(hostname, port, OnConnect, Tuple.Create<MessageBox, LoginEventArgs>(messageBox, loginData));
            ViewMgr.PushLayer(messageBox);
        }

        private void OnDisconnected(EventArgs args)
        {
            var msgBoxArgs = args as MessageBoxArgs;
            var messageBox = new MessageBox(MessageBoxButtons.OK)
            {
                Title = msgBoxArgs.Title,
                Message = msgBoxArgs.Message
            };
            messageBox.OkPressed += (sender, e) => { ViewMgr.PopLayer(); };
            ViewMgr.PushLayer(messageBox);
        }

        #endregion

        #region Async network callbacks

        private void OnConnect(IAsyncResult ar)
        {                        
            InvokeOnMainThread(
                delegate(object arg)
                {
                    var network = ViewMgr.Client.Network;
                    var connectData = (Tuple<MessageBox, LoginEventArgs>)ar.AsyncState;
                    var messageBox = connectData.Item1;
                    var loginData = connectData.Item2;

                    try
                    {
                        if (network.EndConnect(ar))
                        {
                            network.BeginLogin(loginData.Login, loginData.Password, OnLogin, messageBox);
                            messageBox.Message = "Logging in as " + loginData.Login + "...";
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
            InvokeOnMainThread(
                delegate(object arg)
                {
                    var network = ViewMgr.Client.Network;
                    var messageBox = (MessageBox)ar.AsyncState;

                    try
                    {
                        network.EndLogin(ar);
                        Client.ChangeState(new LobbyState(Game));
                    }
                    catch (Exception exc)
                    {
                        network.BeginDisconnect(OnDisconnect, null);
                        messageBox.Message = exc.Message;
                        messageBox.Buttons = MessageBoxButtons.OK;
                        messageBox.OkPressed += (sender, e) => ViewMgr.PopLayer();
                    }
                }, null);            
        }

        private void OnDisconnect(IAsyncResult ar)
        {
            var network = ViewMgr.Client.Network;
            network.EndDisconnect(ar);
        }

        private void OnDisconnected_EventHandler(string reason)
        {
            InvokeOnMainThread(
                delegate(object arg)
                {
                    ViewMgr.PopLayer(); // Probably "Logging in..." MessageBox
                    HandleViewEvent("OnDisconnected", new MessageBoxArgs("Disconnection", "You were forcefully kicked out by the server."));
                }, reason);            
        }

        #endregion
    }
}
