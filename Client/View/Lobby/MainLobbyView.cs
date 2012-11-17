namespace Client.View.Lobby
{
	using System;
	using System.Collections.Generic;
	using Client.Common;
	using Client.Input;
	using Client.Model;
	using Client.State;
	using Nuclex.UserInterface;
	using Nuclex.UserInterface.Controls;
	using Nuclex.UserInterface.Controls.Desktop;

    class MainLobbyView : BaseView
    {
        #region Protected members

        private List<LobbyListInfo> _gameInfoList = new List<LobbyListInfo>();
        private ListControl _gameList;

        protected void CreateChildControls()
        {
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

            screen.Desktop.Children.AddRange(new Control[] { btnJoinGame, btnCreateGame, btnLogout, btnRefresh, _gameList });
        }

        #endregion

        #region Event handlers

        private void Logout_Pressed(object sender, EventArgs e)
        {
			LobbyState.Logout();
        }
        private void Refresh_Pressed(object sender, EventArgs e)
        {
			LobbyState.RefreshGameList(this);
        }
        private void JoinGame_Pressed(object sender, EventArgs e)
        {
            if (_gameList.SelectedItems.Count == 0) return;

            int lobbyIndex = _gameList.SelectedItems[0];
            int lobbyId = _gameInfoList[lobbyIndex].LobbyId;

			LobbyState.JoinGame(lobbyId);
        }
        private void CreateGame_Pressed(object sender, EventArgs e)
        {
			LobbyState.EnterCreateGameView();
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

		public LobbyState LobbyState { get; protected set; }

		public MainLobbyView(LobbyState state)
            : base(state)
        {
			LobbyState = state;
            IsTransparent = true;
            screen.Desktop.Bounds = new UniRectangle(new UniScalar(0.2f, 0), new UniScalar(0.2f, 0), new UniScalar(0.6f, 0), new UniScalar(0.6f, 0));
            InputReceiver = new NuclexScreenInputReceiver(screen, false);

            CreateChildControls();
			State = ViewState.Loaded;
        }        
    }
}
