namespace Client.Renderer
{
	using System;
	using System.Linq;
	using Client.Common;
	using Microsoft.Xna.Framework;
	using Microsoft.Xna.Framework.Graphics;
	using Model;

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

		protected void InitializeMapVisual(Map map)
		{
			var vertices = new VertexPositionColor[map.Links.Count * 2];
			var color = Color.LightGreen;

			for (var i = 0; i < map.Links.Count; ++i)
			{
				var link = map.Links[i];
				var sourcePlanet = map.Planets.First(x => x.Id == link.SourcePlanet);
				var targetPlanet = map.Planets.First(x => x.Id == link.TargetPlanet);

				vertices[2 * i + 0] = new VertexPositionColor(new Vector3(sourcePlanet.X, sourcePlanet.Y, sourcePlanet.Z), color);
				vertices[2 * i + 1] = new VertexPositionColor(new Vector3(targetPlanet.X, targetPlanet.Y, targetPlanet.Z), color);
			}

			var visual = new MapVisual();
			visual.LinksVB = new VertexBuffer(GraphicsDevice, VertexPositionColor.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
			visual.LinksVB.SetData(vertices);

			map.Visual = visual;
		}
		protected void InitializePlanetVisual(Planet planet)
		{
			var contentMgr = Client.Content;
			var visual = new PlanetVisual();
			var random = new Random(Guid.NewGuid().GetHashCode());

			visual.Planet = planet;
			visual.Effect = contentMgr.Load<Effect>("Effects\\Planet");
			visual.InfoFont = contentMgr.Load<SpriteFont>("Fonts\\HUD");
			visual.VB = _sphereVB;
			visual.Period = (float)(random.NextDouble() * 10.0 + 5.0);
			visual.Yaw = (float)(random.NextDouble() * MathHelper.TwoPi);
			visual.Pitch = (float)(random.NextDouble() * MathHelper.TwoPi);
			visual.Roll = (float)(random.NextDouble() * MathHelper.TwoPi);
			
			if (!string.IsNullOrEmpty(planet.Diffuse))
			{
				visual.DiffuseTexture = contentMgr.Load<Texture2D>(planet.Diffuse);
			}
			if (!string.IsNullOrEmpty(planet.Clouds))
			{
				visual.CloudsTexture = contentMgr.Load<Texture2D>(planet.Clouds);
			}
			if (!string.IsNullOrEmpty(planet.CloudsAlpha))
			{
				visual.CloudsAlphaTexture = contentMgr.Load<Texture2D>(planet.CloudsAlpha);
			}

			planet.Visual = visual;
		}

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
			GraphicsDevice = Client.GraphicsDevice;

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
		public void Draw(ICamera camera, Scene scene, double delta, double time)
		{
			GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil, Color.Black, 1, 0);

			var map = scene.Map;

			_spriteBatch.Begin();
			_spriteBatch.Draw(_txSpace, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
			_spriteBatch.End();

            // Turn depth buffer on (SpriteBatch may turn it off).
			GraphicsDevice.DepthStencilState = DepthStencilState.Default; // new DepthStencilState() { DepthBufferEnable = true };

			#region Links

			_fxLinks.Parameters["World"].SetValue(Matrix.Identity);
			_fxLinks.Parameters["View"].SetValue(camera.GetView());
			_fxLinks.Parameters["Projection"].SetValue(camera.Projection);

			if (map.Visual == null)
			{
				InitializeMapVisual(map);
			}

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
				}
			}

			#endregion

			// planets
			foreach (var planet in map.Planets)
			{
				if (planet.Visual == null)
				{
					InitializePlanetVisual(planet);
				}
				var planetarySystem = scene.Map.GetSystemByPlanetid(planet.Id);

				var ambient = scene.SelectedPlanet == planet.Id || scene.HoveredPlanet == planet.Id ? HoverAmbient : 0.0f;
				var glow = planetarySystem != null ? planetarySystem.Color : Color.LightGray;

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
	}
}
