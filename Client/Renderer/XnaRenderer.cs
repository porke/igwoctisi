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
		protected VertexBuffer _sphereVB;

        private SimpleCamera _camera;

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

		public bool RaySphereIntersection(Vector2 screenPosition, Vector3 position, float radius)
		{
			var ray = _camera.GetRay(GraphicsDevice.Viewport, new Vector3(screenPosition, 0));
			return ray.Intersects(new BoundingSphere(position, radius)) != null;
		}
		public bool RayLinkIntersection(Vector2 screenPosition, Vector3 linkSource, Vector3 linkTarget)
		{
			var ray = _camera.GetRay(GraphicsDevice.Viewport, new Vector3(screenPosition, 0));
			return ray.Intersects(new BoundingSphere((linkSource + linkTarget) / 2.0f, LinkJointSize)) != null;
		}

		public void Initialize(GameClient client)
		{
			Client = client;
			GraphicsDevice = Client.GraphicsDevice;
		    _camera = new SimpleCamera(GraphicsDevice);

			var contentMgr = Client.Content;
			_spriteBatch = new SpriteBatch(GraphicsDevice);
			_fontHud = contentMgr.Load<SpriteFont>("Fonts\\HUD");
			_fxLinks = contentMgr.Load<Effect>("Effects\\Links");
			_fxPlanet = contentMgr.Load<Effect>("Effects\\Planet");

			var vertices = Utils.SphereVertices(3).Select(x => new Vertex(x.Position, x.Normal, Color.LightGreen, x.TextureCoordinate)).ToArray();
			_sphereVB = new VertexBuffer(GraphicsDevice, Vertex.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
			_sphereVB.SetData(vertices);
		}
		public void Release()
		{
			Client = null;
		}
		public void Draw(Scene scene, double delta, double time)
		{
            // Turn depth buffer on (SpriteBatch may turn it off).
			GraphicsDevice.DepthStencilState = DepthStencilState.Default; // new DepthStencilState() { DepthBufferEnable = true };

            // Update current camera and pass it to the scene renderer
			_camera.Update(delta);
            scene.Visual.Camera = _camera;

			var map = scene.Map;
			
			#region Links

			_camera.ApplyToEffect(_fxLinks, Matrix.Identity);

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

			#region Planets

			foreach (var planet in map.Planets)
			{
				if (planet.Visual == null)
				{
					InitializePlanetVisual(planet);
				}
				var visual = planet.Visual;

				_fxPlanet.Parameters["Ambient"].SetValue(scene.SelectedPlanet == planet.Id || scene.HoveredPlanet == planet.Id ? HoverAmbient : 0.0f);
				_fxPlanet.Parameters["Diffuse"].SetValue(visual.DiffuseTexture);
				_fxPlanet.Parameters["Clouds"].SetValue(visual.CloudsTexture);
				_fxPlanet.Parameters["CloudsAlpha"].SetValue(visual.CloudsAlphaTexture);

				var localWorld = Matrix.CreateScale(planet.Radius) * 
							Matrix.CreateRotationY((float)time / visual.Period * MathHelper.TwoPi) *
							Matrix.CreateFromYawPitchRoll(visual.Yaw, visual.Pitch, visual.Roll) *
							Matrix.CreateTranslation(planet.X, planet.Y, planet.Z);

				_camera.ApplyToEffect(_fxPlanet, localWorld);                
				foreach (var pass in _fxPlanet.CurrentTechnique.Passes)
				{
					pass.Apply();
					GraphicsDevice.SetVertexBuffer(_sphereVB);
					GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, _sphereVB.VertexCount / 3);
				}

				if (scene.SelectedPlanet == planet.Id)
				{
					foreach (var link in map.Links.Where(x => x.SourcePlanet == planet.Id || x.TargetPlanet == planet.Id))
					{
						var sourcePlanet = map.GetPlanetById(link.SourcePlanet);
						var targetPlanet = map.GetPlanetById(link.TargetPlanet);

						var linkWorld = Matrix.CreateScale(LinkJointSize) *
							Matrix.CreateTranslation(
							(sourcePlanet.X + targetPlanet.X)/2.0f,
							(sourcePlanet.Y + targetPlanet.Y)/2.0f,
							(sourcePlanet.Z + targetPlanet.Z)/2.0f);

						_camera.ApplyToEffect(_fxLinks, linkWorld);
						_fxLinks.Parameters["Ambient"].SetValue(scene.HoveredLink == link ? HoverAmbient : 0.0f);
						foreach (var pass in _fxLinks.CurrentTechnique.Passes)
						{
							pass.Apply();
							GraphicsDevice.SetVertexBuffer(_sphereVB);
							GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, _sphereVB.VertexCount / 3);
						}
					}
				}
			}
			
			#endregion

            #region Spaceships (and also should there be planets and links)

            scene.Visual.Draw(delta, time);

            #endregion

            #region Planets info

            _spriteBatch.Begin();

			foreach (var planet in scene.Map.Planets)
			{
				var planetScreen = _camera.Project(GraphicsDevice.Viewport, new Vector3(planet.X, planet.Y, planet.Z));
				var fleetText = planet.NumFleetsPresent.ToString();
				var fleetIncome = string.Format("+{0}", planet.BaseUnitsPerTurn);
				var ownerName = planet.Owner != null ? planet.Owner.Username : string.Empty;
				var nameSize = _fontHud.MeasureString(planet.Name);
				var fleetsSize = _fontHud.MeasureString(fleetText);
				var ownerColor = planet.Owner != null ? planet.Owner.Color.XnaColor : Color.Gray;

				var nameScreen = new Vector2(planetScreen.X - nameSize.X / 2.0f, planetScreen.Y - nameSize.Y / 2.0f);
				_spriteBatch.DrawString(_fontHud, planet.Name, nameScreen + NameOffset, Color.Yellow);

                var fleetsScreen = new Vector2(planetScreen.X - fleetsSize.X / 2.0f, planetScreen.Y - fleetsSize.Y / 2.0f);
                if (planet.ShowDetails)
                {                    
                    _spriteBatch.DrawString(_fontHud, fleetText, fleetsScreen + FleetsOffset, Color.Yellow);
                }

                if (scene.HoveredPlanet == planet.Id)
                {
                    _spriteBatch.DrawString(_fontHud, fleetIncome, fleetsScreen + FleetIncomeOffset, Color.Yellow);
                }

				if (planet.FleetChange != 0)
				{
					var color = planet.FleetChange > 0 ? Color.Green : Color.Red;
					var text = string.Format("{0}{1}", planet.FleetChange > 0 ? "+" : "", planet.FleetChange);
					_spriteBatch.DrawString(_fontHud, text, fleetsScreen + FleetDeltaOffset, color);
				}

				_spriteBatch.DrawString(_fontHud, ownerName, nameScreen + OwnerNameOffset, ownerColor);
			}

			_spriteBatch.End();

			#endregion
		}

		#endregion

		public SimpleCamera GetCamera() { return _camera; }

		public GameClient Client { get; protected set; }
		public GraphicsDevice GraphicsDevice { get; protected set; }
	}
}
