namespace Client.View.Play
{
    using Client.State;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;
    using Model;
    using Nuclex.Input;

    class GameViewport : BaseView
    {
        #region IView members

        public override void Draw(double delta, double time)
        {
            var renderer = GameState.Client.Renderer;

            renderer.Draw(PlayState.Scene, delta, time);
        }

        #endregion

        #region Nested class: GameInputReceiver

        private class GameInputReceiver : Client.Input.NullInputReceiver
        {
			public static int FleetCountForMultiActions = 5;

            private Vector2 _currentMousePosition = Vector2.Zero;
            private GameViewport _receiverView;
			private bool _isShiftDown = false;

            protected internal GameInputReceiver(GameViewport receiverView) : base(false)
            {				
                _receiverView = receiverView;
            }

            #region IInputReceiver members

            public override bool OnMouseMoved(Vector2 position)
            {
                _currentMousePosition = position;
                
                var planet = _receiverView.PlayState.Scene.PickPlanet(_currentMousePosition, _receiverView.PlayState.Client.Renderer);
                if (planet != null)
                {
                    _receiverView.OnHoverPlanet(planet);
                }
                else
                {
                    _receiverView.UnhoverPlanets();
                }

				var link = _receiverView.PlayState.Scene.PickLink(_currentMousePosition, _receiverView.PlayState.Client.Renderer);
				if (link != null)
				{
					_receiverView.OnHoverLink(link);
				}
				else
				{
					_receiverView.UnhoverLinks();
				}

                return true;
            }

            public override bool OnMousePressed(MouseButtons button)
            {
                if (button.HasFlag(MouseButtons.Middle)) return true;				

				var scene = this._receiverView.PlayState.Scene;
				PlanetLink link = null;

				int count = _isShiftDown ? FleetCountForMultiActions : 1;
                var planet = _receiverView.PlayState.Scene.PickPlanet(_currentMousePosition, _receiverView.PlayState.Client.Renderer);
				if (planet != null)
				{
					if (scene.SelectedPlanet == planet.Id)
					{
						if (button.HasFlag(MouseButtons.Left))
						{							
							_receiverView.DeployFleet(planet, count);
						}
						else if (button.HasFlag(MouseButtons.Right))
						{
							_receiverView.UndeployFleet(planet, count);
						}
					}
					else
					{
						_receiverView.PlanetSelected(planet);
					}
				}
				else if ((link = _receiverView.PlayState.Scene.PickLink(_currentMousePosition, _receiverView.PlayState.Client.Renderer)) != null)
				{
                    if (button.HasFlag(MouseButtons.Left))
                    {
                        _receiverView.MoveFleet(link, count);
                    }
                    else if (button.HasFlag(MouseButtons.Right))
                    {
						_receiverView.RevertMoveFleet(link, count);
                    }
				}

                return true;
            }

            public override bool OnKeyPressed(Keys key)
            {
                var camera = _receiverView.PlayState.Client.Renderer.GetCamera();

                switch (key)
                {
					case Keys.LeftShift:
					case Keys.RightShift:
						_isShiftDown = true;
						break;
                    case Keys.Down:
                        camera.TranslationDirection = Vector3.UnitY;
                        break;
                    case Keys.Up:
                        camera.TranslationDirection = -Vector3.UnitY;
                        break;
                    case Keys.Left:
                        camera.TranslationDirection = -Vector3.UnitX;
                        break;
                    case Keys.Right:
                        camera.TranslationDirection = Vector3.UnitX;
                        break;
                }

                return true;
            }

            public override bool OnKeyReleased(Keys key)
            {
                switch (key)
                {
					case Keys.LeftShift:
					case Keys.RightShift:
						_isShiftDown = false;
						break;
                    case Keys.Down:
                    case Keys.Up:
                    case Keys.Left:
                    case Keys.Right:
                        _receiverView.PlayState.Client.Renderer.GetCamera().TranslationDirection = Vector3.Zero;
                        break;
                }

                return true;
            }

            #endregion
        }        

        #endregion

        #region Event handlers

        private void DeployFleet(Planet destinationPlanet, int count)
        {
			PlayState.DeployFleet(destinationPlanet, count);
        }
        private void UndeployFleet(Planet destinationPlanet, int count)
        {
			PlayState.UndeployFleet(destinationPlanet, count);
        }
        private void PlanetSelected(Planet planetSelected)
        {
			PlayState.SelectPlanet(planetSelected);
        }
        private void OnHoverPlanet(Planet planetSelected)
        {
			PlayState.OnHoverPlanet(planetSelected);
        }
        private void UnhoverPlanets()
        {
			PlayState.UnhoverPlanets();
        }
		private void OnHoverLink(PlanetLink linkSelected)
		{
			PlayState.OnHoverLink(linkSelected);
		}
		private void UnhoverLinks()
		{
			PlayState.UnhoverLinks();
		}
		private void RevertMoveFleet(PlanetLink linkSelected, int count)
		{
            PlayState.RevertMoveFleet(linkSelected, count);
		}
        private void MoveFleet(PlanetLink linkSelected, int count)
        {
            PlayState.MoveFleet(linkSelected, count);
        }

        #endregion

        public PlayState PlayState { get; protected set; }

		public GameViewport(PlayState state)
			: base(state)
        {
            IsTransparent = false;
            InputReceiver = new GameInputReceiver(this);
            PlayState = state;
			State = ViewState.Loaded;
        }
    }
}
