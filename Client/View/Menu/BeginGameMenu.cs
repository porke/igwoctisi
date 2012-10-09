namespace Client.View.Menu
{
    using System;
    using Common;
    using Input;
    using Nuclex.UserInterface;
    using Nuclex.UserInterface.Controls.Desktop;
    using State;

    class BeginGameMenu : IView
    {
        #region Protected members

        protected Screen _screen;

        protected void CreateChildControls()
        {
            var btnJoinGame = new ButtonControl
            {
                Text = "Join Game",
                Bounds = new UniRectangle(new UniScalar(0.4f, 0), new UniScalar(0.40f, 0), new UniScalar(0.2f, 0), new UniScalar(0.05f, 0))
            };
            btnJoinGame.Pressed += JoinGame_Pressed;

            var btnCreateGame = new ButtonControl
            {
                Text = "Create Game",
                Bounds = new UniRectangle(new UniScalar(0.4f, 0), new UniScalar(0.55f, 0), new UniScalar(0.2f, 0), new UniScalar(0.05f, 0))
            };
            btnCreateGame.Pressed += CreateGame_Pressed;

            _screen.Desktop.Children.AddRange(new[] { btnJoinGame, btnCreateGame });
        }

        protected void JoinGame_Pressed(object sender, EventArgs e)
        {
            State.Game.ChangeState(new LobbyState(State.Game));
        }

        protected void CreateGame_Pressed(object sender, EventArgs e)
        {
            State.Game.ChangeState(new LobbyState(State.Game));
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

        public BeginGameMenu(MenuState state)
        {
            State = state;
            _screen = new Screen(800, 600);
            InputReceiver = new NuclexScreenInputReceiver(_screen, false);

            CreateChildControls();
        }
    }
}
