namespace Client.Renderer
{
	using System;
	using System.Collections.Generic;
	using Client.Common.AnimationSystem;
	using Client.Model;
	using Client.View.Play.Animations;
	using Microsoft.Xna.Framework.Graphics;

	public sealed class SceneVisual
	{
		#region Protected members

		private Scene Scene { get; set; }
		private readonly AnimationManager AnimationManager;
		private readonly List<Spaceship> _spaceships = new List<Spaceship>();
		private readonly List<Spaceship> _spaceshipsToAdd = new List<Spaceship>();

		public void UpdateSpaceSheeps()
		{
			// add spacesheeps
			lock (_spaceshipsToAdd)
			{
				// It is more efficient to use that type of loop than foreach/ForEach.
				for (int i = 0, n = _spaceshipsToAdd.Count; i < n; ++i)
				{
					_spaceships.Add(_spaceshipsToAdd[i]);
				}

				_spaceshipsToAdd.Clear();
			}

			// remove invisible spacesheeps
			_spaceships.RemoveAll(x => !x.Visible);
		}

		#endregion

		#region Event handlers

		protected void Animation_Deploys(IList<Tuple<Planet, int, Action>> deploys)
		{
			this.AnimateDeploys(AnimationManager, Scene.Map.Camera, deploys);
		}
		protected void Animation_MovesAndAttacks(IList<Tuple<Planet, Planet, SimulationResult, Action<SimulationResult>>> movesAndAttacks)
		{
			this.AnimateMovesAndAttacks(movesAndAttacks, AnimationManager, Scene.Map.Camera);
		}

		#endregion

		public SceneVisual(GameClient client, Scene scene, AnimationManager animationManager)
		{
			Scene = scene;
			AnimationManager = animationManager;
			Spaceship.SetupModelPools(client.Content, animationManager);

			// Install handlers
			scene.AnimDeploys += new Action<IList<Tuple<Planet, int, Action>>>(Animation_Deploys);
			scene.AnimMovesAndAttacks += new Action<List<Tuple<Planet, Planet, SimulationResult, Action<SimulationResult>>>>(Animation_MovesAndAttacks);

			scene.Map.Visual = new MapVisual(client, scene.Map);
		}
		public void AddSpaceship(Spaceship ship)
		{
			lock (_spaceshipsToAdd)
			{
				_spaceshipsToAdd.Add(ship);
			}
		}
		public void Update(double delta, double time)
		{
			UpdateSpaceSheeps();
		}
		public void DrawBackground(Viewport viewport, double delta, double time)
		{
			Scene.Map.Visual.DrawBackground(viewport, delta, time);
		}
		public void Draw(double delta, double time)
		{
			foreach (var ship in _spaceships)
			{
				ship.Draw(Scene.Map.Camera, delta, time);
			}
		}
	}
}
