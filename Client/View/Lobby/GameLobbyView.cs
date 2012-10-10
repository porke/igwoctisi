namespace Client.View.Lobby
{
    using System;
    using Common;
    using Input;
    using Nuclex.UserInterface;
    using Nuclex.UserInterface.Controls;
    using Nuclex.UserInterface.Controls.Desktop;
    using State;

    class GameLobbyView : IView
    {
        #region Protected members

        protected Screen _screen;

        private ListControl _messageList;
        private InputControl _currentMessage;

        protected void CreateChildControls()
        {
            var btnBeginGame = new ButtonControl()
            {
                Text = "Begin Game",
                Bounds = new UniRectangle(new UniScalar(0.6f, 0), new UniScalar(0.2f, 0), new UniScalar(0.35f, 0), new UniScalar(0.1f, 0))
            };
            btnBeginGame.Pressed += BeginGame_Pressed;

            var btnLeaveGame = new ButtonControl()
            {
                Text = "Leave Game",
                Bounds = new UniRectangle(new UniScalar(0.6f, 0), new UniScalar(0.35f, 0), new UniScalar(0.35f, 0), new UniScalar(0.1f, 0))
            };
            btnLeaveGame.Pressed += LeaveGame_Pressed;

            _messageList = new ListControl()
            {
                Bounds = new UniRectangle(new UniScalar(0.05f, 0), new UniScalar(0.5f, 0), new UniScalar(0.9f, 0), new UniScalar(0.4f, 0))
            };

            _currentMessage = new InputControl()
            {
                Bounds = new UniRectangle(new UniScalar(0.05f, 0), new UniScalar(0.925f, 0), new UniScalar(0.9f, 0), new UniScalar(0.1f, 0))
            };

            _screen.Desktop.Children.AddRange(new Control[] { btnBeginGame, btnLeaveGame, _messageList, _currentMessage });
        }

        protected void BeginGame_Pressed(object sender, EventArgs e)
        {
            State.Game.ChangeState(new PlayState(State.Game));
        }

        protected void LeaveGame_Pressed(object sender, EventArgs e)
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

        public LobbyState State { get; protected set; }
        public ViewManager ViewMgr { get; protected set; }

        public GameLobbyView(LobbyState state)
        {
            State = state;
            _screen = new Screen(800, 600);
            _screen.Desktop.Bounds = new UniRectangle(new UniScalar(0.3f, 0), new UniScalar(0.3f, 0), new UniScalar(0.4f, 0), new UniScalar(0.4f, 0));
            InputReceiver = new NuclexScreenInputReceiver(_screen, false);

            CreateChildControls();
        }
    }
}
