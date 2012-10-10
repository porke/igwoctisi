namespace Client.State
{
    using View;
    using View.Menu;
    using System;

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
        }

        #region Event handlers

        private void QuitGame(EventArgs args)
        {
            Client.Exit();
        }

        private void RequestLogin(EventArgs args)
        {
            string hostname = "v.zloichuj.eu";
            int port = 23456;
            var loginData = args as LoginEventArgs;

            var messageBox = new MessageBox(MessageBoxButtons.None)
            {
                Title = "Login",
                Message = string.Format("Connecting... {0}@{1}", loginData.Login, loginData.Password)
            };
            ViewMgr.PushLayer(messageBox);
            Client.Network.BeginConnect(hostname, port, OnConnect, messageBox);
        }

        #endregion

        #region Async network callbacks

        private void OnConnect(IAsyncResult ar)
        {
            var network = ViewMgr.Client.Network;
            var messageBox = (MessageBox)ar.AsyncState;

            try
            {
                network.EndConnect(ar);

                ViewMgr.PopLayer(); // MessageBox

                // TODO: Implementation depends on success of the login request
                //ViewMgr.PopLayer(); // Main menu view
                //ViewMgr.PushLayer(new MainMenu(this));
            }
            catch (Exception exc)
            {
                messageBox.Message = exc.Message;
                messageBox.Buttons = MessageBoxButtons.OK;
                messageBox.OkPressed += (sender, e) => ViewMgr.PopLayer();
            }
        }

        #endregion
    }
}
