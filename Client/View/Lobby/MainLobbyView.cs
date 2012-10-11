namespace Client.View.Lobby
{
    using System;
    using Common;
    using Input;
    using Nuclex.UserInterface;
    using Nuclex.UserInterface.Controls;
    using Nuclex.UserInterface.Controls.Desktop;
    using State;
    using System.Collections.Generic;
    using Client.Model;

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

        protected void Logout_Pressed(object sender, EventArgs e)
        {
            state.HandleViewEvent("Logout", e);
        }

        protected void Refresh_Pressed(object sender, EventArgs e)
        {
            state.HandleViewEvent("RefreshGameList", new SenderEventArgs(this));
        }

        protected void JoinGame_Pressed(object sender, EventArgs e)
        {
            try
            {
                int lobbyIndex = _gameList.SelectedItems[0];
                int lobbyId = _gameInfoList[lobbyIndex].LobbyId;
                state.HandleViewEvent("JoinGame", new JoinGameArgs(lobbyId));
            }
            catch (IndexOutOfRangeException) { }
        }

        protected void CreateGame_Pressed(object sender, EventArgs e)
        {
            state.HandleViewEvent("EnterCreateGameView", e);
        }

        #endregion

        public MainLobbyView(State.GameState state)
            : base(state)
        {
            IsLoaded = true;
            IsTransparent = true;
            screen.Desktop.Bounds = new UniRectangle(new UniScalar(0.2f, 0), new UniScalar(0.2f, 0), new UniScalar(0.6f, 0), new UniScalar(0.6f, 0));
            InputReceiver = new NuclexScreenInputReceiver(screen, false);

            CreateChildControls();
        }

        public void RefreshGameList(object gameInfoList)
        {
            _gameInfoList = gameInfoList as List<LobbyListInfo>;
            _gameList.Items.Clear();

            foreach (var info in _gameInfoList)
            {
                _gameList.Items.Add(info.Name);
            }

            // Only first element should be selected
            _gameList.SelectedItems.Clear();
            _gameList.SelectedItems.Add(0);
        }
    }
}
