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
        private ListControl _playerList;
        private InputControl _currentMessage;

        protected void CreateChildControls()
        {
            var btnBeginGame = new ButtonControl()
            {
                Text = "Begin Game",
                Bounds = new UniRectangle(new UniScalar(0.7f, 0), new UniScalar(0.05f, 0), new UniScalar(0.25f, 0), new UniScalar(0.1f, 0))
            };
            btnBeginGame.Pressed += BeginGame_Pressed;

            var btnLeaveGame = new ButtonControl()
            {
                Text = "Leave Game",
                Bounds = new UniRectangle(new UniScalar(0.7f, 0), new UniScalar(0.2f, 0), new UniScalar(0.25f, 0), new UniScalar(0.1f, 0))
            };
            btnLeaveGame.Pressed += LeaveGame_Pressed;

            var btnKickPlayer = new ButtonControl()
            {
                Text = "Kick player",
                Bounds = new UniRectangle(new UniScalar(0.7f, 0), new UniScalar(0.35f, 0), new UniScalar(0.25f, 0), new UniScalar(0.1f, 0))
            };
            btnKickPlayer.Pressed += KickPlayer_Pressed;

            _messageList = new ListControl()
            {
                Bounds = new UniRectangle(new UniScalar(0.05f, 0), new UniScalar(0.5f, 0), new UniScalar(0.9f, 0), new UniScalar(0.4f, 0))
            };
            _messageList.Items.Add("Gratrz 1: Klaniam sie Szanownemu Panu.");
            _messageList.Items.Add("Gratrz 2: Uszanowanie.");

            _currentMessage = new InputControl()
            {
                Text = "Test test Test",
                Bounds = new UniRectangle(new UniScalar(0.05f, 0), new UniScalar(0.925f, 0), new UniScalar(0.9f, 0), new UniScalar(0.1f, 0))
            };            

            _playerList = new ListControl()
            {
                SelectionMode = ListSelectionMode.Single,
                Bounds = new UniRectangle(new UniScalar(0.05f, 0), new UniScalar(0.05f, 0), new UniScalar(0.6f, 0), new UniScalar(0.4f, 0))
            };
            _playerList.Items.Add("Gratrz 1 [Host]");
            _playerList.Items.Add("Gratrz 2");

            screen.Desktop.Children.AddRange(new Control[] { btnBeginGame, btnLeaveGame, _messageList, _currentMessage, _playerList, btnKickPlayer });
        }

        #endregion        

        #region Event handlers

        private void BeginGame_Pressed(object sender, EventArgs e)
        {
            state.HandleViewEvent("BeginGame", e);
        }

        private void LeaveGame_Pressed(object sender, EventArgs e)
        {
            state.HandleViewEvent("LeaveGameLobby", e);
        }

        private void KickPlayer_Pressed(object sender, EventArgs e)
        {
            // TODO: Remove player - available only for the host
        }

        #endregion

        public GameLobbyView(GameState state)
            : base(state)
        {            
            IsLoaded = true;
            IsTransparent = true;
            screen.Desktop.Bounds = new UniRectangle(new UniScalar(0.2f, 0), new UniScalar(0.25f, 0), new UniScalar(0.6f, 0), new UniScalar(0.5f, 0));
            InputReceiver = new NuclexScreenInputReceiver(screen, false);

            CreateChildControls();
        }
    }
}
