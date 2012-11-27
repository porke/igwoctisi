namespace Client.Renderer
{
	using System.Collections.Generic;
	using System.Linq;
	using Client.Common;
	using Microsoft.Xna.Framework;
	using Microsoft.Xna.Framework.Graphics;
	using Model;

	public class XnaRenderer : IRenderer
	{
		#region Protected members

		protected SpriteBatch _spriteBatch;
		private Dictionary<int, bool> _planetsDetailsShowing = new Dictionary<int,bool>();

		#endregion

		#region IRenderer members

		public bool RaySphereIntersection(ICamera camera, Vector2 screenPosition, Vector3 position, float radius)
		{
			var ray = camera.GetRay(GraphicsDevice.Viewport, new Vector3(screenPosition, 0));
			return ray.Intersects(new BoundingSphere(position, radius)) != null;
		}
		public bool RayLinkIntersection(ICamera camera, Vector2 screenPosition, Planet sourcePlanet, Planet targetPlanet)
		{
			var ray = camera.GetRay(GraphicsDevice.Viewport, new Vector3(screenPosition, 0));
			var indicator = sourcePlanet.Visual.GetIndicator(targetPlanet.Id);
			return indicator.IsIntersected(ray);
		}

		public void Initialize(GameClient client)
		{
			Client = client;
			GraphicsDeviceService = (IGraphicsDeviceService)Client.Services.GetService(typeof(IGraphicsDeviceService));
			GraphicsDevice = GraphicsDeviceService.GraphicsDevice;

			var contentMgr = Client.Content;
			_spriteBatch = new SpriteBatch(GraphicsDevice);

			var quadVertices = new[] {
				new VertexPositionColor(new Vector3(0, 0, 0), Color.Red),
				new VertexPositionColor(new Vector3(0, 1, 0), Color.Red),
				new VertexPositionColor(new Vector3(1, 1, 0), Color.Red),
				new VertexPositionColor(new Vector3(1, 0, 0), Color.Red)
			};
		}
		public void Release()
		{
			Client = null;
		}
		public void Update(Scene scene, double delta, double time)
		{
			scene.Visual.Update(delta, time);
		}
		public void Draw(ICamera camera, Scene scene, double delta, double time)
		{
			GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil, Color.Black, 1, 0);

			var map = scene.Map;

            // Turn depth and stencil buffers on (SpriteBatch may turn it off).
			GraphicsDevice.DepthStencilState = DepthStencilState.Default;

			scene.Visual.DrawBackground(GraphicsDevice, camera, delta, time);

			// planets
			foreach (var planet in map.Planets)
			{
				var planetarySystem = scene.Map.GetSystemByPlanetid(planet.Id);

				var ambient = scene.SelectedPlanet == planet.Id || scene.HoveredPlanet == planet.Id ? HoverAmbient : 0.0f;
				var glow = planetarySystem != null && planet.Owner != null ? planet.Owner.Color.XnaColor : Color.LightGray;

				bool grayPlanet = planet.Owner != scene.ClientPlayer
					&& planet.NeighbourPlanets.All(p => p.Owner != scene.ClientPlayer);
				planet.Visual.Draw(GraphicsDevice, camera, delta, time, ambient, glow, grayPlanet);

				// cache that information
				_planetsDetailsShowing[planet.Id] = !grayPlanet;
			}

			// links & move indicators
			scene.Visual.DrawIndicators(GraphicsDevice, camera, delta, time);

			GraphicsDevice.Clear(ClearOptions.Stencil, Color.Black, 1, 0);

			// spacesheeps
			scene.Visual.Draw(GraphicsDevice, camera, delta, time);

            // planets info
            _spriteBatch.Begin();
			foreach (var planet in scene.Map.Planets)
			{
				if (_planetsDetailsShowing[planet.Id])
				{
					planet.Visual.DrawInfo(GraphicsDevice, _spriteBatch, camera, scene.HoveredPlanet == planet.Id);
				}
			}
			_spriteBatch.End();
		}

		#endregion

		public const float HoverAmbient = 1.0f;

		public GameClient Client { get; protected set; }
		public GraphicsDevice GraphicsDevice { get; protected set; }
		public IGraphicsDeviceService GraphicsDeviceService { get; protected set; }
	}
}
