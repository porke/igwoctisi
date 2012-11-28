namespace Client.View.Play
{
	using System;
	using Client.Input;
	using Client.State;
	using Client.View.Controls;
	using Microsoft.Xna.Framework.Input;
	using Model;
	using Nuclex.Input;
	using Nuclex.UserInterface;

    class GameViewport : BaseView
	{
		public event EventHandler PlanetsUnhovered;
		public event EventHandler LinksUnhovered;
		public event EventHandler<EventArgs<Planet>> PlanetHovered;
		public event EventHandler<EventArgs<PlanetLink>> LinkHovered;
		public event EventHandler<EventArgs<Planet>> PlanetSelected;
		public event EventHandler<EventArgs<PlanetLink, int>> FleetMoved;
		public event EventHandler<EventArgs<PlanetLink, int>> FleetMoveReverted;		
		public event EventHandler<EventArgs<Planet, int>> FleetDeployReverted;
		public event EventHandler<EventArgs<Planet, int>> FleetDeployed;

		public bool Enabled { get; set; }

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
        }

        #endregion

        #region Event handlers

		public void Viewport_MouseClick(ViewportControl viewport, MouseButtons button)
		{
			if (!Enabled) return;

			var scene = PlayState.Scene;
			var keyboard = Keyboard.GetState();
			var count = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift)
				? FleetCountForMultiActions : 1;

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
						DeployFleet(_viewport.HoveredPlanet, count);
					}
				}
				else if (button == MouseButtons.Right)
				{
					if (scene.SelectedPlanet != 0)
					{
						UndeployFleet(_viewport.HoveredPlanet, count);
					}
				}
			}
			else if (_viewport.HoveredLink != null)
			{
				if (button == MouseButtons.Left)
				{
					MoveFleet(_viewport.HoveredLink, count);
				}
				else if (button == MouseButtons.Right)
				{
					RevertMoveFleet(_viewport.HoveredLink, count);
				}
			}
		}
		private void SelectPlanet(Planet planet)
		{
			if (PlanetSelected != null)
			{
				PlanetSelected(this, PlanetSelected.CreateArgs(planet));
			}
		}
		private void DeployFleet(Planet destinationPlanet, int count)
        {
			if (FleetDeployed != null)
			{
				FleetDeployed(this, FleetDeployed.CreateArgs(destinationPlanet, count));
			}
        }
        private void UndeployFleet(Planet destinationPlanet, int count)
        {
			if (FleetDeployReverted != null)
			{
				FleetDeployReverted(this, FleetDeployReverted.CreateArgs(destinationPlanet, count));
			}
        }
        private void HoverPlanet(Planet planet)
        {
			if (PlanetHovered != null)
			{
				PlanetHovered(this, PlanetHovered.CreateArgs(planet));
			}
        }
        private void UnhoverPlanets()
        {
			if (PlanetsUnhovered != null)
			{
				PlanetsUnhovered(this, EventArgs.Empty);
			}			
        }
		private void HoverLink(PlanetLink link)
		{
			if (LinkHovered != null)
			{
				LinkHovered(this, LinkHovered.CreateArgs(link));
			}
		}
		private void UnhoverLinks()
		{
			if (LinksUnhovered != null)
			{
				LinksUnhovered(this, EventArgs.Empty);
			}
		}
		private void RevertMoveFleet(PlanetLink linkSelected, int count)
		{
			if (FleetMoveReverted != null)
			{
				FleetMoveReverted(this, FleetMoveReverted.CreateArgs(linkSelected, count));
			}            
		}
		private void MoveFleet(PlanetLink linkSelected, int count)
        {
			if (FleetMoved != null)
			{
				FleetMoved(this, FleetMoved.CreateArgs(linkSelected, count));
			}
        }

        #endregion

		public static readonly int FleetCountForMultiActions = 5;

        public PlayState PlayState { get; protected set; }

		public GameViewport(PlayState state)
			: base(state)
        {
            IsTransparent = false;
			InputReceiver = new NuclexScreenInputReceiver(screen, false);
            PlayState = state;
			Enabled = true;
			State = ViewState.Loaded;

			_viewport = new ViewportControl(PlayState.Client.Renderer);
			_viewport.Bounds = new UniRectangle(new UniScalar(0, 0), new UniScalar(0, 0), new UniScalar(1, 0), new UniScalar(1, 0));
			_viewport.MouseClick += Viewport_MouseClick;
			this.screen.Desktop.Children.Add(_viewport);
        }
    }
}
