namespace Client.View.Lobby
{
    using System;
    using System.IO;
    using Client.Input.Controls;
    using Common;
    using Input;
    using Nuclex.UserInterface;
    using Nuclex.UserInterface.Controls;
    using Nuclex.UserInterface.Controls.Desktop;
    using State;

    class CreateGameView : BaseView
    {
        #region Private members

        private ListControl _mapList;
        private CommandInputControl _gameName;

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

            _gameName = new CommandInputControl()
            {
                Text = "My Game Name",
                Bounds = new UniRectangle(new UniScalar(0.6f, 0), new UniScalar(0.05f, 0), new UniScalar(0.4f, 0), new UniScalar(0.1f, 0))
            };
            _gameName.OnCommandHandler += CreateGame_Pressed;

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

            screen.Desktop.Children.AddRange(new Control[] {lblGameName, lblMaps, _mapList, btnCancel, btnCreateGame, _gameName} );

            LoadMapNames();
            if (_mapList.Items.Count > 0)
            {
                _mapList.SelectedItems.Add(0);
            }
            else
            {
                btnCreateGame.Enabled = false;
            }
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

        #region Event handlers

        private void Cancel_Pressed(object sender, EventArgs args)
        {
			LobbyState.CancelCreateGame();
        }
        private void CreateGame_Pressed(object sender, EventArgs args)
        {
            var mapName = _mapList.Items[_mapList.SelectedItems[0]];
			LobbyState.CreateGame(_gameName.Text, mapName);
        }

        #endregion

		public LobbyState LobbyState { get; protected set; }

		public CreateGameView(LobbyState state)
            : base(state)
        {
			LobbyState = state;
            IsTransparent = true;
            screen.Desktop.Bounds = new UniRectangle(new UniScalar(0.3f, 0), new UniScalar(0.25f, 0), new UniScalar(0.4f, 0), new UniScalar(0.5f, 0));
            InputReceiver = new NuclexScreenInputReceiver(screen, false);

            CreateChildControls();
			State = ViewState.Loaded;
        }
    }
}
