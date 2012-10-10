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
    using System.Collections.Generic;
    using Client.Model;

    class MainLobbyView : IView
    {
        #region Protected members

        protected Screen _screen;

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
            _screen.Desktop.Children.AddRange(new Control[] { btnJoinGame, btnCreateGame, btnLogout, btnRefresh, _gameList });
        }

        protected void Logout_Pressed(object sender, EventArgs e)
        {
            State.Client.Network.BeginDisconnect(OnLogout, null);
        }

        protected void Refresh_Pressed(object sender, EventArgs e)
        {
            State.Client.Network.BeginGetGameList(OnReceiveGameList, null);
        }

        protected void JoinGame_Pressed(object sender, EventArgs e)
        {
            //TODO: Implement join game
        }

        protected void CreateGame_Pressed(object sender, EventArgs e)
        {            
            ViewMgr.PopLayer(); // this
            ViewMgr.PushLayer(new CreateGameView(State));
        }

        private void OnLogout(IAsyncResult result)
        {
            State.Client.Network.EndDisconnect(result);
            State.Client.ChangeState(new MenuState(State.Game));
        }

        private void OnReceiveGameList(IAsyncResult result)
        {            
            _gameList.Items.Clear();

            var asyncResult = result as AsyncResult<List<GameInfo>>;
            var gameList = asyncResult.Result as List<GameInfo>;
            State.Client.Network.EndGetGameList(result);

            foreach (GameInfo game in gameList)
            {
                _gameList.Items.Add(game.Name);
            }

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

        public MainLobbyView(LobbyState state)
        {
            State = state;
            _screen = new Screen(800, 600);
            _screen.Desktop.Bounds = new UniRectangle(new UniScalar(0.2f, 0), new UniScalar(0.2f, 0), new UniScalar(0.6f, 0), new UniScalar(0.6f, 0));
            InputReceiver = new NuclexScreenInputReceiver(_screen, false);

            CreateChildControls();
        }
    }
}
