namespace Client.View.Lobby
{
    using System;
    using Common;
    using Input;
    using Nuclex.UserInterface;
    using Nuclex.UserInterface.Controls;
    using Nuclex.UserInterface.Controls.Desktop;
    using State;

    class GameLobbyView : BaseView
    {
        #region Protected members

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

            screen.Desktop.Children.AddRange(new Control[] { btnBeginGame, btnLeaveGame, _messageList, _currentMessage });
        }

        protected void BeginGame_Pressed(object sender, EventArgs e)
        {
            state.Game.ChangeState(new PlayState(state.Game));
        }

        protected void LeaveGame_Pressed(object sender, EventArgs e)
        {
            state.Game.ChangeState(new LobbyState(state.Game));
        }

        #endregion

        public GameLobbyView(GameState state)
            : base(state)
        {
            IsLoaded = true;
            IsTransparent = true;
            screen.Desktop.Bounds = new UniRectangle(new UniScalar(0.3f, 0), new UniScalar(0.3f, 0), new UniScalar(0.4f, 0), new UniScalar(0.4f, 0));
            InputReceiver = new NuclexScreenInputReceiver(screen, false);

            CreateChildControls();
        }
    }
}
