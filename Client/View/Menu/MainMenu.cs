namespace Client.View.Menu
{
    using System;
    using Common;
    using Input;
    using Nuclex.UserInterface;
    using Nuclex.UserInterface.Controls.Desktop;
    using State;

    class MainMenu : BaseView
    {
        #region Protected members

        protected void CreateChildControls()
        {
            var btnNewGame = new ButtonControl
            {
                Text = "New Game",
                Bounds = new UniRectangle(new UniScalar(0.4f, 0), new UniScalar(0.35f, 0), new UniScalar(0.2f, 0), new UniScalar(0.05f, 0))
            };
            btnNewGame.Pressed += NewGame_Pressed;

            var btnCredits = new ButtonControl
            {
                Text = "Credits",
                Bounds = new UniRectangle(new UniScalar(0.4f, 0), new UniScalar(0.45f, 0), new UniScalar(0.2f, 0), new UniScalar(0.05f, 0))
            };
            btnCredits.Pressed += Credits_Pressed;

            var btnQuit = new ButtonControl
            {
                Text = "Quit",
                Bounds = new UniRectangle(new UniScalar(0.4f, 0), new UniScalar(0.55f, 0), new UniScalar(0.2f, 0), new UniScalar(0.05f, 0))
            };
            btnQuit.Pressed += Quit_Pressed;

            screen.Desktop.Children.AddRange(new[] { btnNewGame, btnCredits, btnQuit });
        }

        protected void NewGame_Pressed(object sender, EventArgs e)
        {
            string hostname = "localhost";// "v.zloichuj.eu";
            int port = 23456;

            var messageBox = new MessageBox(MessageBoxButtons.None)
            {
                Title = "Login",
                Message = "Connecting..."
            };
            ViewMgr.PushLayer(messageBox);
            ViewMgr.Client.Network.BeginConnect(hostname, port, OnConnect, messageBox);
        }

        protected void Credits_Pressed(object sender, EventArgs e)
        {

        }

        protected void Quit_Pressed(object sender, EventArgs e)
        {
            state.Client.Exit();
        }

        protected void OnConnect(IAsyncResult ar)
        {
            var network = ViewMgr.Client.Network;
            var messageBox = (MessageBox)ar.AsyncState;

            try
            {
                network.EndConnect(ar);

                ViewMgr.PopLayer(); // MessageBox
                ViewMgr.PopLayer(); // this
                ViewMgr.PushLayer(new LoginMenu(state));
            }
            catch (Exception exc)
            {
                messageBox.Message = exc.Message;
                messageBox.Buttons = MessageBoxButtons.OK;
                messageBox.OkPressed += (sender, e) => ViewMgr.PopLayer();
            }
        }

        #endregion

        public MainMenu(GameState state) : base(state)
        {
            IsLoaded = true;
            IsTransparent = true;
            InputReceiver = new NuclexScreenInputReceiver(screen, false);

            CreateChildControls();
        }
    }
}
