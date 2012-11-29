namespace Client.View.Lobby
{
	using System;
	using System.Collections.Generic;
	using Client.Common;
	using Client.Input;
	using Client.Model;
	using Client.State;
	using Common.AnimationSystem;
	using Common.AnimationSystem.DefaultAnimations;
	using Nuclex.UserInterface;
	using Nuclex.UserInterface.Controls;
	using Nuclex.UserInterface.Controls.Desktop;
	using Client.View.Controls;

    class MainLobbyView : BaseView
    {
		public event EventHandler LogoutPressed;
		public event EventHandler RefreshPressed;
		public event EventHandler CreateGamePressed;
		public event EventHandler<EventArgs<int>> JoinGamePressed;

        #region Protected members

        private List<LobbyListInfo> _gameInfoList = new List<LobbyListInfo>();
        private ListControl _gameList;

        protected void CreateChildControls()
        {
			var mainDialog = new IconControl("rounded_background")
			{
				Bounds = new UniRectangle(new UniScalar(0.2f, 0), new UniScalar(0.2f, 0), new UniScalar(0.6f, 0), new UniScalar(0.6f, 0))
			};

            var btnJoinGame = new ButtonControl
            {
                Text = "Join Game",
                Bounds = new UniRectangle(new UniScalar(0.05f, 0), new UniScalar(0.85f, 0), new UniScalar(0.2f, 0), new UniScalar(0.1f, 0))
            };
            btnJoinGame.Pressed += JoinGame_Pressed;

            var btnRefresh = new ButtonControl
            {
                Text = "Refresh",
                Bounds = new UniRectangle(new UniScalar(0.283f, 0), new UniScalar(0.85f, 0), new UniScalar(0.2f, 0), new UniScalar(0.1f, 0))
            };
            btnRefresh.Pressed += Refresh_Pressed;

            var btnCreateGame = new ButtonControl
            {
                Text = "Create Game",
                Bounds = new UniRectangle(new UniScalar(0.516f, 0), new UniScalar(0.85f, 0), new UniScalar(0.2f, 0), new UniScalar(0.1f, 0))
            };
            btnCreateGame.Pressed += CreateGame_Pressed;            

            var btnLogout = new ButtonControl
            {
                Text = "Logout",
                Bounds = new UniRectangle(new UniScalar(0.749f, 0), new UniScalar(0.85f, 0), new UniScalar(0.2f, 0), new UniScalar(0.1f, 0))
            };
            btnLogout.Pressed += Logout_Pressed;            

            _gameList = new ListControl
            {
                SelectionMode = ListSelectionMode.Single,                
                Bounds = new UniRectangle(new UniScalar(0.05f, 0), new UniScalar(0.05f, 0), new UniScalar(0.9f, 0), new UniScalar(0.75f, 0))
            };

			screen.Desktop.Children.Add(mainDialog);
			mainDialog.Children.AddRange(new Control[] { btnJoinGame, btnCreateGame, btnLogout, btnRefresh, _gameList });
        }

        #endregion

        #region Event handlers

        private void Logout_Pressed(object sender, EventArgs e)
        {
			if (LogoutPressed != null)
			{
				LogoutPressed(this, EventArgs.Empty);
			}
        }
        private void Refresh_Pressed(object sender, EventArgs e)
        {
			if (RefreshPressed != null)
			{
				RefreshPressed(this, EventArgs.Empty);
			}
        }
        private void JoinGame_Pressed(object sender, EventArgs e)
        {
            if (_gameList.SelectedItems.Count == 0) return;

			if (JoinGamePressed != null)
			{
				int lobbyIndex = _gameList.SelectedItems[0];
				int lobbyId = _gameInfoList[lobbyIndex].LobbyId;
				JoinGamePressed(this, JoinGamePressed.CreateArgs(lobbyId));
			}
        }
        private void CreateGame_Pressed(object sender, EventArgs e)
        {
			if (CreateGamePressed != null)
			{
				CreateGamePressed(this, EventArgs.Empty);
			}
        }

        #endregion

		#region Update requests

		public void RefreshGameList(List<LobbyListInfo> gameInfoList)
		{
			_gameInfoList = gameInfoList;
			_gameList.Items.Clear();

			foreach (var info in _gameInfoList)
			{
				_gameList.Items.Add(info.Name);
			}

			// Only first element should be selected
			if (_gameList.Items.Count > 0)
			{
				_gameList.SelectedItems.Clear();
				_gameList.SelectedItems.Add(0);
			}
		}

		#endregion

		#region BaseView members

		protected override void OnReturnTo(double time)
		{
			screen.Desktop.Bounds = new UniRectangle(new UniScalar(-0.8f, 0), new UniScalar(0.2f, 0), new UniScalar(0.6f, 0), new UniScalar(0.6f, 0));
			screen.Desktop.Animate(this).MoveControlTo(new UniVector(new UniScalar(0.2f, 0), new UniScalar(0.2f, 0))).AddCallback(x => State = ViewState.Visible);
		}

		#endregion

		public MainLobbyView(LobbyState state, ViewState initialState)
            : base(state)
        {
            IsTransparent = true;
            screen.Desktop.Bounds = new UniRectangle(new UniScalar(), new UniScalar(), new UniScalar(1.0f, 0), new UniScalar(1.0f, 0));
            InputReceiver = new NuclexScreenInputReceiver(screen, false);

            CreateChildControls();
			State = initialState;
        }        
    }
}
