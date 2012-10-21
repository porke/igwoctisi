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
            var renderer = state.Client.Renderer;

            renderer.Draw(PlayState.Scene, delta, time);
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
					_receiverView.LinkSelected(link);
				}

                return true;
            }

            public override bool OnKeyPressed(Keys key)
            {
                var camera = _receiverView.PlayState.Client.Renderer.GetCamera();

                switch (key)
                {
                    case Keys.Down:
                        camera.TranslationDirection = -Vector3.UnitY;
                        break;
                    case Keys.Up:
                        camera.TranslationDirection = Vector3.UnitY;
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
            state.HandleViewEvent("DeployFleet", new SelectPlanetArgs(destinationPlanet));
        }

        private void UndeployFleet(Planet destinationPlanet)
        {
            state.HandleViewEvent("UndeployFleet", new SelectPlanetArgs(destinationPlanet));
        }

        private void PlanetSelected(Planet planetSelected)
        {
            state.HandleViewEvent("SelectPlanet", new SelectPlanetArgs(planetSelected));
        }

        private void OnHoverPlanet(Planet planetSelected)
        {
            state.HandleViewEvent("OnHoverPlanet", new SelectPlanetArgs(planetSelected));
        }

        private void UnhoverPlanets()
        {
            state.HandleViewEvent("UnhoverPlanets", null);
        }

		private void OnHoverLink(PlanetLink linkSelected)
		{
			state.HandleViewEvent("OnHoverLink", new SelectLinkArgs(linkSelected));
		}

		private void UnhoverLinks()
		{
			state.HandleViewEvent("UnhoverLinks", null);
		}

		private void LinkSelected(PlanetLink linkSelected)
		{
			state.HandleViewEvent("SelectLink", new SelectLinkArgs(linkSelected));
		}

        #endregion

        public PlayState PlayState { get; protected set; }

        public GameViewport(GameState state) : base(state)
        {
            IsLoaded = true;
            IsTransparent = false;
            InputReceiver = new GameInputReceiver(this);
            PlayState = (PlayState)state;
        }
    }
}
