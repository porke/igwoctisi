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
            _gameList.Items.Add("Game 1");
            _gameList.Items.Add("Game 2");
            _gameList.Items.Add("Game 3");
            _gameList.Items.Add("Game 4");
            _gameList.Items.Add("Game 5");

            Refresh_Pressed(null, null);
            screen.Desktop.Children.AddRange(new Control[] { btnJoinGame, btnCreateGame, btnLogout, btnRefresh, _gameList });
        }

        protected void Logout_Pressed(object sender, EventArgs e)
        {
            state.HandleViewEvent("Logout", e);
        }

        protected void Refresh_Pressed(object sender, EventArgs e)
        {
            state.HandleViewEvent("RefreshGameList", e);
        }

        protected void JoinGame_Pressed(object sender, EventArgs e)
        {
            state.HandleViewEvent("JoinGame", new JoinGameArgs(string.Empty));
        }

        protected void CreateGame_Pressed(object sender, EventArgs e)
        {
            state.HandleViewEvent("EnterCreateGameView", e);
            var asyncResult = result as AsyncResult<List<GameInfo>>;
            var gameList = asyncResult.Result as List<GameInfo>;
            State.Client.Network.EndGetGameList(result);

            foreach (GameInfo game in gameList)
            {
                _gameList.Items.Add(game.Name);
            }

            State.Client.Network.EndGetGameList(result);
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
