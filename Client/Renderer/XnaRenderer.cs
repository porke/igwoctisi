namespace Client.Renderer
{
	using System;
	using System.Linq;
	using Client.Common;
	using Microsoft.Xna.Framework;
	using Microsoft.Xna.Framework.Graphics;
	using Model;
	using Client.Renderer.Particles;
	using System.Collections.Generic;
	using Client.Renderer.Particles.ParticleSystems;

	public class XnaRenderer : IRenderer
	{
		#region Protected members

		protected const float HoverAmbient = 1.0f;
		protected const float LinkJointSize = 15.0f;
		protected readonly Vector2 NameOffset = new Vector2(0.0f, 25.0f);
		protected readonly Vector2 FleetsOffset = new Vector2(0.0f, 42.0f);
		protected readonly Vector2 FleetIncomeOffset = new Vector2(21.0f, 42.0f);
		protected readonly Vector2 FleetDeltaOffset = new Vector2(21.0f, -42.0f);
		protected readonly Vector2 OwnerNameOffset = new Vector2(0.0f, 59.0f);
		protected SpriteBatch _spriteBatch;
		protected SpriteFont _fontHud;
		protected Effect _fxLinks, _fxPlanet;
		protected VertexBuffer _sphereVB, _sphereVB2;
		protected Texture2D _txSpace;

				planetarySystem.Visual = new PlanetarySystemVisual(Client, Client.Content, planetarySystem.Bounds);
				planetarySystem.Visual.Color = Color.LightGray;
		#endregion

		#region IRenderer members

		public bool RaySphereIntersection(ICamera camera, Vector2 screenPosition, Vector3 position, float radius)
		{
			var ray = camera.GetRay(GraphicsDevice.Viewport, new Vector3(screenPosition, 0));
			return ray.Intersects(new BoundingSphere(position, radius)) != null;
		}
		public bool RayLinkIntersection(ICamera camera, Vector2 screenPosition, Vector3 linkSource, Vector3 linkTarget)
		{
			var ray = camera.GetRay(GraphicsDevice.Viewport, new Vector3(screenPosition, 0));
			return ray.Intersects(new BoundingSphere((linkSource + linkTarget) / 2.0f, LinkJointSize)) != null;
		}

		public void Initialize(GameClient client)
		{
			Client = client;
			GraphicsDeviceService = (IGraphicsDeviceService)Client.Services.GetService(typeof(IGraphicsDeviceService));
			GraphicsDevice = GraphicsDeviceService.GraphicsDevice;

			var contentMgr = Client.Content;
			_spriteBatch = new SpriteBatch(GraphicsDevice);
			_fontHud = contentMgr.Load<SpriteFont>("Fonts\\HUD");
			_fxLinks = contentMgr.Load<Effect>("Effects\\Links");
			_fxPlanet = contentMgr.Load<Effect>("Effects\\Planet");
			_txSpace = contentMgr.Load<Texture2D>("Textures\\Space");

			var quadVertices = new[] {
				new VertexPositionColor(new Vector3(0, 0, 0), Color.Red),
				new VertexPositionColor(new Vector3(0, 1, 0), Color.Red),
				new VertexPositionColor(new Vector3(1, 1, 0), Color.Red),
				new VertexPositionColor(new Vector3(1, 0, 0), Color.Red)
			};

			var vertices = Utils.SphereVertices(3).Select(x => new Vertex(x.Position, x.Normal, Color.LightGreen, x.TextureCoordinate)).ToArray();
			_sphereVB = new VertexBuffer(GraphicsDevice, Vertex.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
			_sphereVB.SetData(vertices);

			_sphereVB2 = new VertexBuffer(GraphicsDevice, Vertex.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
			_sphereVB2.SetData(vertices);
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

			_spriteBatch.Begin();
			_spriteBatch.Draw(_txSpace, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
			_spriteBatch.End();

            // Turn depth buffer on (SpriteBatch may turn it off).
			GraphicsDevice.DepthStencilState = DepthStencilState.Default;

			#region Links

			_fxLinks.Parameters["World"].SetValue(Matrix.Identity);
			_fxLinks.Parameters["View"].SetValue(camera.GetView());
			_fxLinks.Parameters["Projection"].SetValue(camera.Projection);

			_fxLinks.Parameters["Ambient"].SetValue(0.0f);
			foreach (var pass in _fxLinks.CurrentTechnique.Passes)
			{
				pass.Apply();
				GraphicsDevice.SetVertexBuffer(map.Visual.LinksVB);
				GraphicsDevice.DrawPrimitives(PrimitiveType.LineList, 0, map.Visual.LinksVB.VertexCount / 2);
			}

			#endregion

			#region Systems (until particle system)

			_fxLinks.Parameters["Ambient"].SetValue(0.0f);
			_fxLinks.Parameters["View"].SetValue(camera.GetView());
			_fxLinks.Parameters["Projection"].SetValue(camera.Projection);
			foreach (var planetarySystem in map.PlanetarySystems)
			{
				foreach (var point in planetarySystem.Bounds)
				{
					var world = Matrix.CreateScale(5) *
						Matrix.CreateTranslation(point.X, point.Y, point.Z);
					_fxLinks.Parameters["World"].SetValue(world);

					foreach (var pass in _fxLinks.CurrentTechnique.Passes)
					{
						pass.Apply();
						GraphicsDevice.SetVertexBuffer(_sphereVB);
						GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, _sphereVB.VertexCount / 3);
					}

					planetarySystem.Visual.Update(GraphicsDevice, camera, delta, time);
				}
			}

			#endregion

			// planets
			foreach (var planet in map.Planets)
			{
				var planetarySystem = scene.Map.GetSystemByPlanetid(planet.Id);

				var ambient = scene.SelectedPlanet == planet.Id || scene.HoveredPlanet == planet.Id ? HoverAmbient : 0.0f;
				var glow = planetarySystem != null && planet.Owner != null ? planet.Owner.Color.XnaColor : Color.LightGray;

				planet.Visual.Draw(GraphicsDevice, camera, time, ambient, glow);
			}

			#region Move indicators

			var selectedPlanet = scene.Map.GetPlanetById(scene.SelectedPlanet);
			if (selectedPlanet != null)
			{
				foreach (var link in map.Links.Where(x => x.SourcePlanet == selectedPlanet.Id || x.TargetPlanet == selectedPlanet.Id))
				{
					var sourcePlanet = map.GetPlanetById(link.SourcePlanet);
					var targetPlanet = map.GetPlanetById(link.TargetPlanet);

					var linkWorld = Matrix.CreateScale(LinkJointSize) *
						Matrix.CreateTranslation(
						(sourcePlanet.X + targetPlanet.X) / 2.0f,
						(sourcePlanet.Y + targetPlanet.Y) / 2.0f,
						(sourcePlanet.Z + targetPlanet.Z) / 2.0f);

					_fxLinks.Parameters["World"].SetValue(linkWorld);
					_fxLinks.Parameters["View"].SetValue(camera.GetView());
					_fxLinks.Parameters["Projection"].SetValue(camera.Projection);
					_fxLinks.Parameters["Ambient"].SetValue(scene.HoveredLink == link ? HoverAmbient : 0.0f);
					foreach (var pass in _fxLinks.CurrentTechnique.Passes)
					{
						pass.Apply();
						GraphicsDevice.SetVertexBuffer(_sphereVB);
						GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, _sphereVB.VertexCount / 3);
					}
				}
			}
			
			#endregion

			// spacesheeps
			scene.Visual.Draw(delta, time);

            // planets info
            _spriteBatch.Begin();
			foreach (var planet in scene.Map.Planets)
			{
				planet.Visual.DrawInfo(GraphicsDevice, _spriteBatch, camera, scene.HoveredPlanet == planet.Id);
			}
			_spriteBatch.End();
		}

		#endregion

		public GameClient Client { get; protected set; }
		public GraphicsDevice GraphicsDevice { get; protected set; }
		public IGraphicsDeviceService GraphicsDeviceService { get; protected set; }
	}
}
