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

		public override void Update(double delta, double time)
		{
			base.Update(delta, time);
			_viewport.Update(delta, time);

			var scene = PlayState.Scene;
			var hoveredPlanetId = _viewport.HoveredPlanet != null ? _viewport.HoveredPlanet.Id : 0;
			if (scene.HoveredPlanet != hoveredPlanetId)
			{
				if (_viewport.HoveredPlanet != null)
				{
					HoverPlanet(_viewport.HoveredPlanet);
				}
				else
				{
					UnhoverPlanets();
				}
			}
			if (scene.HoveredLink != _viewport.HoveredLink)
			{
				if (_viewport.HoveredLink != null)
				{
					HoverLink(_viewport.HoveredLink);
				}
				else
				{
					UnhoverLinks();
				}
			}
		}
		public override void Draw(double delta, double time)
        {
			_viewport.Scene = PlayState.Scene;
			_viewport.Draw(delta, time);
			base.Draw(delta, time);

            /*var renderer = GameState.Client.Renderer;
            renderer.Draw(PlayState.Scene, delta, time);*/
        }

        #endregion

        #region Event handlers

		public void Viewport_MouseClick(ViewportControl viewport, MouseButtons button)
		{
			var scene = PlayState.Scene;

			if (_viewport.HoveredPlanet != null)
			{
				if (button == MouseButtons.Left)
				{
					if (scene.SelectedPlanet != _viewport.HoveredPlanet.Id)
					{
						SelectPlanet(_viewport.HoveredPlanet);
					}
					else
					{
						DeployFleet(_viewport.HoveredPlanet);
					}
				}
				else if (button == MouseButtons.Right)
				{
					if (scene.SelectedPlanet != null)
					{
						UndeployFleet(_viewport.HoveredPlanet);
					}
				}
			}
			else if (_viewport.HoveredLink != null)
			{
				if (button == MouseButtons.Left)
				{
					MoveFleet(_viewport.HoveredLink);
				}
				else if (button == MouseButtons.Right)
				{
					RevertMoveFleet(_viewport.HoveredLink);
				}
			}
		}
		private void SelectPlanet(Planet planet)
		{
			PlayState.SelectPlanet(planet);
		}
        private void DeployFleet(Planet destinationPlanet)
        {
			PlayState.DeployFleet(destinationPlanet);
        }
        private void UndeployFleet(Planet destinationPlanet)
        {
			PlayState.UndeployFleet(destinationPlanet);
        }
        private void HoverPlanet(Planet planet)
        {
			PlayState.OnHoverPlanet(planet);
        }
        private void UnhoverPlanets()
        {
			PlayState.UnhoverPlanets();
        }
		private void HoverLink(PlanetLink link)
		{
			PlayState.OnHoverLink(link);
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
			_viewport.MouseClick += Viewport_MouseClick;
			this.screen.Desktop.Children.Add(_viewport);
        }
    }
}
