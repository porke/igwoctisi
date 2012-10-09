namespace Client.View.Menu
{
    using System;
    using Common;
    using Input;
    using Nuclex.UserInterface;
    using Nuclex.UserInterface.Controls.Desktop;
    using State;

    class MainMenu : IView
    {
        #region Protected members

        protected Screen _screen;

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

            _screen.Desktop.Children.AddRange(new[] { btnNewGame, btnCredits, btnQuit });
        }

        protected void NewGame_Pressed(object sender, EventArgs e)
        {
            ViewMgr.PopLayer();
            ViewMgr.PushLayer(new LoginMenu(State));
        }

        protected void Credits_Pressed(object sender, EventArgs e)
        {

        }

        protected void Quit_Pressed(object sender, EventArgs e)
        {
            State.Client.Exit();
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

        public MainMenu(MenuState state)
        {
            State = state;
            _screen = new Screen(800, 600);
            InputReceiver = new NuclexScreenInputReceiver(_screen, false);

            CreateChildControls();
        }
    }
}
