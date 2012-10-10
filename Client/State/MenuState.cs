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

        #region Event handlers

        private void QuitGame(EventArgs args)
        {
            Client.Exit();
        }

        private void RequestLogin(EventArgs args)
        {
            string hostname = "localhost"; // "v.zloichuj.eu";
            int port = 23456;
            var loginData = args as LoginEventArgs;

            var messageBox = new MessageBox(MessageBoxButtons.None)
            {
                Title = "Login",
                Message = string.Format("Connecting... {0}@{1}", loginData.Login, loginData.Password)
            };
            ViewMgr.PushLayer(messageBox);
            Client.Network.BeginConnect(hostname, port, OnConnect, Tuple.Create<MessageBox, LoginEventArgs>(messageBox, loginData));
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
            var network = ViewMgr.Client.Network;
            var data = (Tuple<MessageBox, LoginEventArgs>)ar.AsyncState;
            var messageBox = data.Item1;
            var loginData = data.Item2;

            try
            {
                network.EndConnect(ar);
                network.BeginLogin(loginData.Login, loginData.Password, OnLogin, messageBox);

                messageBox.Message = "Logging in as " + loginData.Login + "...";
            }
            catch (Exception exc)
            {
                messageBox.Message = exc.Message;
                messageBox.Buttons = MessageBoxButtons.OK;
                messageBox.OkPressed += (sender, e) => ViewMgr.PopLayer();
            }
        }

        private void OnLogin(IAsyncResult ar)
        {
            var network = ViewMgr.Client.Network;
            var messageBox = (MessageBox)ar.AsyncState;

            try
            {
                network.EndLogin(ar);
                ViewMgr.PopLayer(); // MessageBox
                ViewMgr.PopLayer(); // login input
                
                Client.ChangeState(new LobbyState(Game));
            }
            catch (Exception exc)
            {
                messageBox.Message = exc.Message;
                messageBox.Buttons = MessageBoxButtons.OK;
                messageBox.OkPressed += (sender, e) => ViewMgr.PopLayer();
            }
        }

        private void OnDisconnected_EventHandler(string reason)
        {
            //ViewMgr.PopLayer(); //
            //HandleViewEvent("OnDisconnected", new MessageBoxArgs("Disconnection", "You were forcefully kicked out by the server."));
        }

        #endregion
    }
}
