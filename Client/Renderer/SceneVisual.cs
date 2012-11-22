namespace Client.Renderer
{
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using Client.Common.AnimationSystem;
	using Client.Model;
	using Client.View.Play.Animations;
	using Microsoft.Xna.Framework.Graphics;
	using Microsoft.Xna.Framework;
	using Client.Common;

	public sealed class SceneVisual
	{
		#region Private members


		private Scene Scene { get; set; }
		private Effect _fxLinks;
		private VertexBuffer _sphereVB;
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

		private void Animation_Deploys(IList<Tuple<Planet, int, Action>> deploys)
		{
			this.AnimateDeploys(AnimationManager, Scene.Map.Camera, deploys);
		}
		private void Animation_MovesAndAttacks(IList<Tuple<Planet, Planet, SimulationResult, Action<SimulationResult>>> movesAndAttacks)
		{
			this.AnimateMovesAndAttacks(movesAndAttacks, AnimationManager, Scene.Map.Camera);
		}

		#endregion

		public SceneVisual(GameClient client, Scene scene, AnimationManager animationManager)
		{
			Scene = scene;
			AnimationManager = animationManager;
			Spaceship.SetupModelPools(client.Content, animationManager);

			var contentMgr = client.Content;
			var device = client.GraphicsDevice;

			_fxLinks = contentMgr.Load<Effect>("Effects\\Links");
			var vertices = Utils.SphereVertices(3).Select(x => new Vertex(x.Position, x.Normal, Color.LightGreen, x.TextureCoordinate)).ToArray();
			_sphereVB = new VertexBuffer(device, Vertex.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
			_sphereVB.SetData(vertices);

			// Install handlers
			scene.AnimDeploys += new Action<IList<Tuple<Planet, int, Action>>>(Animation_Deploys);
			scene.AnimMovesAndAttacks += new Action<List<Tuple<Planet, Planet, SimulationResult, Action<SimulationResult>>>>(Animation_MovesAndAttacks);

			scene.Map.Visual = new MapVisual(client, scene.Map);
			scene.Map.Visual.Initialize();
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
			Scene.Map.Visual.Update(delta, time);
			UpdateSpaceSheeps();
		}
		public void DrawBackground(GraphicsDevice device, ICamera camera, double delta, double time)
		{
			Scene.Map.Visual.DrawBackground(device, camera, delta, time);
		}
		public void Draw(GraphicsDevice device, ICamera camera, double delta, double time)
		{
			foreach (var ship in _spaceships)
			{
				ship.Draw(camera, delta, time);
			}
		}
		public void DrawIndicators(GraphicsDevice device, ICamera camera, double delta, double time)
		{
			Scene.Map.Visual.DrawLinks(device, camera, delta, time);

			// move indicators
			var selectedPlanet = Scene.Map.GetPlanetById(Scene.SelectedPlanet);

			if (selectedPlanet != null)
			{
				selectedPlanet.Visual.DrawIndicators(device, camera, delta, time, Scene.HoveredLink);
				
				/*
				foreach (var link in Scene.Map.Links.Where(x => x.SourcePlanet == selectedPlanet.Id || x.TargetPlanet == selectedPlanet.Id))
				{
					var sourcePlanet = Scene.Map.GetPlanetById(link.SourcePlanet);
					var targetPlanet = Scene.Map.GetPlanetById(link.TargetPlanet);

					var linkWorld = Matrix.CreateScale(XnaRenderer.LinkJointSize) *
						Matrix.CreateTranslation(
						(sourcePlanet.X + targetPlanet.X) / 2.0f,
						(sourcePlanet.Y + targetPlanet.Y) / 2.0f,
						(sourcePlanet.Z + targetPlanet.Z) / 2.0f);

					_fxLinks.Parameters["World"].SetValue(linkWorld);
					_fxLinks.Parameters["Ambient"].SetValue(Scene.HoveredLink == link ? XnaRenderer.HoverAmbient : 0.0f);
					foreach (var pass in _fxLinks.CurrentTechnique.Passes)
					{
						pass.Apply();
						device.SetVertexBuffer(_sphereVB);
						device.DrawPrimitives(PrimitiveType.TriangleList, 0, _sphereVB.VertexCount / 3);
					}
				}*/
			}
		}
	}
}
