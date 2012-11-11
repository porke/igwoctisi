namespace Client.View.Play
{
    using Client.State;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;
    using Model;
    using Nuclex.Input;
	using Client.Input.Controls;
	using Nuclex.UserInterface;
	using Client.Input;

    class GameViewport : BaseView
	{
		#region Protected members

		protected ViewportControl _viewport;

		#endregion

		#region IView members

		public override void Draw(double delta, double time)
        {
			_viewport.Scene = PlayState.Scene;
			_viewport.Draw(delta, time);
			base.Draw(delta, time);

            /*var renderer = GameState.Client.Renderer;
            renderer.Draw(PlayState.Scene, delta, time);*/
        }

        #endregion

        #region Nested class: GameInputReceiver

        private class GameInputReceiver : Client.Input.NullInputReceiver
        {
            private Vector2 _currentMousePosition = Vector2.Zero;
            private GameViewport _receiverView;

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

                var planet = _receiverView.PlayState.Scene.PickPlanet(_currentMousePosition, _receiverView.PlayState.Client.Renderer);
				if (planet != null)
				{
					if (scene.SelectedPlanet == planet.Id)
					{
						if (button.HasFlag(MouseButtons.Left))
						{
							_receiverView.DeployFleet(planet);
						}
						else if (button.HasFlag(MouseButtons.Right))
						{
							_receiverView.UndeployFleet(planet);
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
                        _receiverView.MoveFleet(link);
                    }
                    else if (button.HasFlag(MouseButtons.Right))
                    {
                        _receiverView.RevertMoveFleet(link);
                    }
				}

                return true;
            }
            public override bool OnKeyPressed(Keys key)
            {
                var camera = _receiverView.PlayState.Client.Renderer.GetCamera();

                switch (key)
                {
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

        private void DeployFleet(Planet destinationPlanet)
        {
			PlayState.DeployFleet(destinationPlanet);
        }
        private void UndeployFleet(Planet destinationPlanet)
        {
			PlayState.UndeployFleet(destinationPlanet);
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
		private void RevertMoveFleet(PlanetLink linkSelected)
		{
            PlayState.RevertMoveFleet(linkSelected);
		}
        private void MoveFleet(PlanetLink linkSelected)
        {
            PlayState.MoveFleet(linkSelected);
        }

        #endregion

        public PlayState PlayState { get; protected set; }

		public GameViewport(PlayState state)
			: base(state)
        {
            IsTransparent = false;
			InputReceiver = new NuclexScreenInputReceiver(screen, false);//new GameInputReceiver(this);
            PlayState = state;
			State = ViewState.Loaded;

			_viewport = new ViewportControl(PlayState.Client.Renderer);
			_viewport.Bounds = new UniRectangle(new UniScalar(0, 0), new UniScalar(0, 0), new UniScalar(1, 0), new UniScalar(1, 0));
			this.screen.Desktop.Children.Add(_viewport);
        }
    }
}
