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

    class LoginMenu : IView
    {
        #region Protected members

        protected Screen _screen;
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

            _screen.Desktop.Children.AddRange(new Control[] { tbLogin, tbPassword, btnLogin, btnBack });
        }
        protected void Login_Pressed(object sender, EventArgs e)
        {
            var messageBox = new MessageBox(MessageBoxButtons.None)
            {
                Title = "Login",
                Message = "Logging in..."
            };
            ViewMgr.PushLayer(messageBox);

            ViewMgr.Client.Network.BeginLogin(tbLogin.Text, tbPassword.Text, OnLogin, messageBox);

        }
        protected void Back_Pressed(object sender, EventArgs e)
        {
            ViewMgr.Client.Network.BeginDisconnect(null, null);
            ViewMgr.PopLayer();
            ViewMgr.PushLayer(new MainMenu(State));
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

                ViewMgr.Client.ChangeState(new LobbyState(State.Game));
            }
            catch (Exception exc)
            {
                messageBox.Message = exc.Message;
                messageBox.Buttons = MessageBoxButtons.OK;
                messageBox.OkPressed += (sender, e) => ViewMgr.PopLayer();
            }
        }

        #endregion

        #region IView members

        public bool IsLoaded
        {
            get { return true; }
        }
        public bool IsTransparent
        {
            get { return true; }
        }
        public IInputReceiver InputReceiver { get; protected set; }

        public void OnShow(ViewManager viewMgr, double time)
        {
            ViewMgr = viewMgr;
        }
        public void OnHide(double time)
        {
        }
        public void Update(double delta, double time)
        {
        }
        public void Draw(double delta, double time)
        {
            ViewMgr.Client.Visualizer.Draw(_screen);
        }

        #endregion

        public MenuState State { get; protected set; }
        public ViewManager ViewMgr { get; protected set; }

        public LoginMenu(MenuState state)
        {
            State = state;
            _screen = new Screen(800, 600);
            InputReceiver = new NuclexScreenInputReceiver(_screen, false);

            CreateChildControls();
        }
    }
}
