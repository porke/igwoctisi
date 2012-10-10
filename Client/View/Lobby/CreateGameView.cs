namespace Client.View.Lobby
{
    using System;
    using System.IO;
    using Common;
    using Input;
    using Nuclex.UserInterface;
    using Nuclex.UserInterface.Controls;
    using Nuclex.UserInterface.Controls.Desktop;
    using State;

    class CreateGameView : IView
    {
        #region Protected members

        protected Screen _screen;

        private ListControl _mapList;
        private InputControl _gameName;

        private void CreateChildControls()
        {
            var lblGameName = new LabelControl("Game name")
            {
                Bounds = new UniRectangle(new UniScalar(0.7f, 0), new UniScalar(0.0f, 0), new UniScalar(0.1f, 0), new UniScalar(0.05f, 0))
            };

            var lblMaps = new LabelControl("Maps")
            {
                Bounds = new UniRectangle(new UniScalar(0.25f, 0), new UniScalar(0.0f, 0), new UniScalar(0.1f, 0), new UniScalar(0.05f, 0))
            };

            _mapList = new ListControl()
            {
                SelectionMode = ListSelectionMode.Single,
                Bounds = new UniRectangle(new UniScalar(0.05f, 0), new UniScalar(0.05f, 0), new UniScalar(0.5f, 0), new UniScalar(1.0f, 0))
            };
            LoadMapNames();

            _gameName = new InputControl()
            {
                Text = "My Game Name",
                Bounds = new UniRectangle(new UniScalar(0.6f, 0), new UniScalar(0.05f, 0), new UniScalar(0.4f, 0), new UniScalar(0.1f, 0))
            };

            var btnCreateGame = new ButtonControl()
            {
                Text = "Create",
                Bounds = new UniRectangle(new UniScalar(0.6f, 0), new UniScalar(0.175f, 0), new UniScalar(0.4f, 0), new UniScalar(0.1f, 0))
            };
            btnCreateGame.Pressed += CreateGame_Pressed;

            var btnCancel = new ButtonControl()
            {
                Text = "Cancel",
                Bounds = new UniRectangle(new UniScalar(0.6f, 0), new UniScalar(0.3f, 0), new UniScalar(0.4f, 0), new UniScalar(0.1f, 0))
            };
            btnCancel.Pressed += Cancel_Pressed;

            _screen.Desktop.Children.AddRange(new Control[] {lblGameName, lblMaps, _mapList, btnCancel, btnCreateGame, _gameName} );
        }

        private void Cancel_Pressed(object sender, EventArgs args)
        {
            ViewMgr.PopLayer();     // this
            ViewMgr.PushLayer(new MainLobbyView(State));
        }

        private void CreateGame_Pressed(object sender, EventArgs args)
        {
            ViewMgr.PopLayer();     // this
            ViewMgr.PushLayer(new GameLobbyView(State));
        }

        private void LoadMapNames()
        {
            var applicationDir = AppDomain.CurrentDomain.BaseDirectory;
            var mapNames = Directory.EnumerateFiles(applicationDir + "/Content/Maps");
            foreach (var mapName in mapNames)
            {
                var filename = Path.GetFileNameWithoutExtension(mapName);
                _mapList.Items.Add(filename);
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

        public CreateGameView(LobbyState state)
        {
            State = state;
            _screen = new Screen(800, 600);
            _screen.Desktop.Bounds = new UniRectangle(new UniScalar(0.3f, 0), new UniScalar(0.25f, 0), new UniScalar(0.4f, 0), new UniScalar(0.5f, 0));
            InputReceiver = new NuclexScreenInputReceiver(_screen, false);

            CreateChildControls();            
        }
    }
}
