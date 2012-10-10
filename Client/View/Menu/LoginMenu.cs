namespace Client.View.Menu
{
    using System;
    using Common;
    using Input;
    using Nuclex.UserInterface;
    using Nuclex.UserInterface.Controls;
    using Nuclex.UserInterface.Controls.Desktop;
    using State;
    using System.Net;

    class LoginMenu : BaseView
    {
        #region Protected members

        protected InputControl tbLogin;
        protected InputControl tbPassword;

        protected void CreateChildControls()
        {
            tbLogin = new InputControl
            {
                Bounds = new UniRectangle(new UniScalar(0.29f, 0), new UniScalar(0.4f, 0), new UniScalar(0.42f, 0), new UniScalar(0.05f, 0))
            };

            tbPassword = new InputControl
            {
                Bounds = new UniRectangle(new UniScalar(0.29f, 0), new UniScalar(0.5f, 0), new UniScalar(0.42f, 0), new UniScalar(0.05f, 0))
            };

            var btnLogin = new ButtonControl
            {
                Text = "Login",
                Bounds = new UniRectangle(new UniScalar(0.29f, 0), new UniScalar(0.6f, 0), new UniScalar(0.2f, 0), new UniScalar(0.05f, 0))
            };
            btnLogin.Pressed += Login_Pressed;

            var btnBack = new ButtonControl
            {
                Text = "Back",
                Bounds = new UniRectangle(new UniScalar(0.51f, 0), new UniScalar(0.6f, 0), new UniScalar(0.2f, 0), new UniScalar(0.05f, 0))
            };
            btnBack.Pressed += Back_Pressed;

            screen.Desktop.Children.AddRange(new Control[] { tbLogin, tbPassword, btnLogin, btnBack });
        }
        protected void Login_Pressed(object sender, EventArgs e)
        {
            var messageBox = new MessageBox(MessageBoxButtons.None)
            {
                Title = "Login",
                Message = "Logging in..."
            };

            ViewMgr.Client.Network.BeginLogin(tbLogin.Text, tbPassword.Text, OnLogin, messageBox);

        }
        protected void Back_Pressed(object sender, EventArgs e)
        {
            ViewMgr.Client.Network.BeginDisconnect(null, null);
            ViewMgr.PopLayer();
            ViewMgr.PushLayer(new MainMenu(state));
        }
                
        protected void OnLogin(IAsyncResult ar)
        {
            var network = ViewMgr.Client.Network;
            var messageBox = (MessageBox) ar.AsyncState;

            try
            {
                network.EndLogin(ar);
                ViewMgr.PopLayer(); // MessageBox
                ViewMgr.PopLayer(); // this

                ViewMgr.Client.ChangeState(new LobbyState(state.Game));
            }
            catch (Exception exc)
            {
                messageBox.Message = exc.Message;
                messageBox.Buttons = MessageBoxButtons.OK;
                messageBox.OkPressed += (sender, e) => ViewMgr.PopLayer();
            }
        }

        #endregion

        public LoginMenu(GameState state)
            : base(state)
        {
            IsLoaded = true;
            IsTransparent = true;
            InputReceiver = new NuclexScreenInputReceiver(screen, false);

            CreateChildControls();
        }
    }
}
