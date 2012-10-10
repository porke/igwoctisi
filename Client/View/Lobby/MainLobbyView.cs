namespace Client.View.Lobby
{
    using System;
    using Common;
    using Input;
    using Nuclex.UserInterface;
    using Nuclex.UserInterface.Controls.Desktop;
    using State;
    using Nuclex.UserInterface.Controls.Arcade;
    using Nuclex.UserInterface.Controls;

    class MainLobbyView : BaseView
    {
        #region Protected members

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
            // FIXIT: Refresh must be called on the main thread or else it can break the ui
            // (weird exceptions resulting from concurrent collection modifications) 
            //btnRefresh.Pressed += Refresh_Pressed;

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

            Refresh_Pressed(null, null);
            screen.Desktop.Children.AddRange(new Control[] { btnJoinGame, btnCreateGame, btnLogout, btnRefresh, _gameList });
        }

        protected void Logout_Pressed(object sender, EventArgs e)
        {
            state.Client.Network.BeginDisconnect(OnLogout, null);
        }

        protected void Refresh_Pressed(object sender, EventArgs e)
        {
            state.Client.Network.BeginGetGameList(OnReceiveGameList, null);
        }

        protected void JoinGame_Pressed(object sender, EventArgs e)
        {
            //TODO: Implement join game
        }

        protected void CreateGame_Pressed(object sender, EventArgs e)
        {            
            ViewMgr.PopLayer(); // this
            ViewMgr.PushLayer(new CreateGameView(state));
        }

        private void OnLogout(IAsyncResult result)
        {
            state.Client.Network.EndDisconnect(result);
            state.Client.ChangeState(new MenuState(state.Game));
        }

        private void OnReceiveGameList(IAsyncResult result)
        {            
            _gameList.Items.Clear();

            var asyncResult = result as AsyncResult<object>;
            var gameNames = asyncResult.Result as string[];
            foreach (var name in gameNames)
            {
                _gameList.Items.Add(name);
            }

            state.Client.Network.EndGetGameList(result);
        }

        #endregion

        public MainLobbyView(GameState state)
            : base(state)
        {
            IsLoaded = true;
            IsTransparent = true;
            screen.Desktop.Bounds = new UniRectangle(new UniScalar(0.2f, 0), new UniScalar(0.2f, 0), new UniScalar(0.6f, 0), new UniScalar(0.6f, 0));
            InputReceiver = new NuclexScreenInputReceiver(screen, false);

            CreateChildControls();
        }
    }
}
