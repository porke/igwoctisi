namespace Client.Renderer
{
    using Microsoft.Xna.Framework.Graphics;
	using Client.Model;
	using Microsoft.Xna.Framework;
	using Microsoft.Xna.Framework.Content;
	using System;
	using Client.Common;
	using System.Linq;
	using Client.Common.AnimationSystem;
	using System.Collections.Generic;

    public class PlanetVisual : ITransformable
    {
		public static readonly Vector2 NameTextOffset = new Vector2(0, -42.0f);
		public static readonly Vector2 FleetsTextOffset = new Vector2(0.0f, 42.0f);
		public static readonly Vector2 FleetsIncomeTextOffset = new Vector2(21.0f, 42.0f);
		public static readonly Vector2 FleetsDeltaTextOffset = new Vector2(21, -42.0f);
		public static readonly Vector2 OwnerTextOffset = new Vector2(0, 25.0f);

		#region ITransformable members

		public float X { get; set; }
		public float Y { get; set; }
		public float Z { get; set; }

		public Matrix Rotation { get; set; }

		public float ScaleX { get; set; }
		public float ScaleY { get; set; }
		public float ScaleZ { get; set; }

		#endregion

		public Planet Planet { get; set; }
        public float Period { get; set; }
		public Effect Effect { get; set; }
		public Effect GlowEffect { get; set; }
		public SpriteFont InfoFont { get; set; }
        public Texture2D DiffuseTexture { get; set; }
        public Texture2D CloudsTexture { get; set; }
        public Texture2D CloudsAlphaTexture { get; set; }
		public VertexBuffer VB { get; set; }

		private List<IndicatorVisual> _indicators;

		public PlanetVisual(GameClient client, Planet planet)
		{
			Planet = planet;
			var device = client.GraphicsDevice;
			var contentMgr = client.Content;

			var random = new Random(Guid.NewGuid().GetHashCode());
			Effect = contentMgr.Load<Effect>("Effects\\Planet");
			GlowEffect = contentMgr.Load<Effect>("Effects\\Glow");
			InfoFont = contentMgr.Load<SpriteFont>("Fonts\\HUD");
			Period = (float)(random.NextDouble() * 10.0 + 5.0);
			Rotation = Matrix.CreateFromYawPitchRoll(
				(float)(random.NextDouble() * MathHelper.TwoPi),
				(float)(random.NextDouble() * MathHelper.TwoPi),
				(float)(random.NextDouble() * MathHelper.TwoPi)
			);
			this.SetPosition(new Vector3(Planet.X, Planet.Y, Planet.Z));
			this.SetScale(Planet.Radius);

			var vertices = Utils.SphereVertices(3).Select(x => new Vertex(x.Position, x.Normal, Color.LightGreen, x.TextureCoordinate)).ToArray();
			VB = new VertexBuffer(device, Vertex.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
			VB.SetData(vertices);

			if (!string.IsNullOrEmpty(planet.Diffuse))
			{
				DiffuseTexture = contentMgr.Load<Texture2D>(planet.Diffuse);
			}
			if (!string.IsNullOrEmpty(planet.Clouds))
			{
				CloudsTexture = contentMgr.Load<Texture2D>(planet.Clouds);
			}
			if (!string.IsNullOrEmpty(planet.CloudsAlpha))
			{
				CloudsAlphaTexture = contentMgr.Load<Texture2D>(planet.CloudsAlpha);
			}

			_indicators = new List<IndicatorVisual>();
			var indicatorModel = contentMgr.Load<Model>(@"Models\Arrow");
			foreach (var neighbour in planet.NeighbourPlanets)
			{
				_indicators.Add(new IndicatorVisual(indicatorModel, planet, neighbour));
			}
		}
		public void Initialize()
		{
			foreach (var indicator in _indicators)
			{
				indicator.Initialize();
			}
		}
		public void Draw(GraphicsDevice device, ICamera camera, double delta, double time, float ambient, Color glow, bool grayPlanet)
		{
			// Update scale, rotation and translation from model
			Rotation *= Matrix.CreateRotationY((float)delta / Period * MathHelper.TwoPi);
			this.SetPosition(new Vector3(Planet.X, Planet.Y, Planet.Z));
			this.SetScale(Planet.Radius);

			var localWorld = this.GetScaleMatrix() * this.Rotation * this.GetTranslationMatrix();
			var view = camera.GetView();
			var projection = camera.Projection;

			Effect.Parameters["World"].SetValue(localWorld);
			Effect.Parameters["View"].SetValue(view);
			Effect.Parameters["Projection"].SetValue(projection);
			Effect.Parameters["Diffuse"].SetValue(DiffuseTexture);
			Effect.Parameters["Clouds"].SetValue(CloudsTexture);
			Effect.Parameters["CloudsAlpha"].SetValue(CloudsAlphaTexture);
			Effect.Parameters["Ambient"].SetValue(ambient);
			Effect.Parameters["Glow"].SetValue(glow.ToVector4());
			Effect.Parameters["PlanetOpacity"].SetValue(grayPlanet ? 0.3f : 1.0f);
			Effect.Parameters["PlanetGrayScale"].SetValue(grayPlanet ? 1 : 0);

			foreach (var pass in Effect.CurrentTechnique.Passes)
			{
				pass.Apply();
				device.SetVertexBuffer(VB);
				device.DrawPrimitives(PrimitiveType.TriangleList, 0, VB.VertexCount / 3);
			}
		}
		public void DrawIndicators(GraphicsDevice device, ICamera camera, double delta, double time, PlanetLink hoveredLink)
		{
			foreach (var indicator in _indicators)
			{
				bool hovered = hoveredLink != null
					&& indicator.TargetPlanet.Id == hoveredLink.TargetPlanet;

				indicator.Draw(device, camera, delta, time, hovered);
			}
		}
		public void DrawGlow(GraphicsDevice device, ICamera camera, double delta, double time, Color glow, bool grayPlanet)
		{
			var localWorld = this.GetScaleMatrix() * this.Rotation * this.GetTranslationMatrix();
			var view = camera.GetView();
			var projection = camera.Projection;

			GlowEffect.Parameters["World"].SetValue(localWorld);
			GlowEffect.Parameters["View"].SetValue(view);
			GlowEffect.Parameters["Projection"].SetValue(projection);
			GlowEffect.Parameters["Glow"].SetValue(glow.ToVector4());
			GlowEffect.Parameters["PlanetOpacity"].SetValue(grayPlanet ? 0.3f : 1.0f);
			GlowEffect.Parameters["PlanetGrayScale"].SetValue(grayPlanet ? 1 : 0);

			foreach (var pass in GlowEffect.CurrentTechnique.Passes)
			{
				pass.Apply();
				device.SetVertexBuffer(VB);
				device.DrawPrimitives(PrimitiveType.TriangleList, 0, VB.VertexCount / 3);
			}
		}
		public void DrawInfo(GraphicsDevice device, SpriteBatch batch, ICamera camera, bool showDetails)
		{
			var planetScreen = camera.Project(device.Viewport, Planet.Position);				

			// number of fleets
			var fleetsText = Planet.NumFleetsPresent.ToString();
			var fleetsTextSize = InfoFont.MeasureString(fleetsText);
			var fleetsTextScreen = new Vector2(planetScreen.X - fleetsTextSize.X / 2.0f, planetScreen.Y - fleetsTextSize.Y / 2.0f);
			batch.DrawString(InfoFont, fleetsText, fleetsTextScreen + FleetsTextOffset, Color.Yellow);

			// owner name
			var ownerText = Planet.Owner != null ? Planet.Owner.Username : string.Empty;
			var ownerTextSize = InfoFont.MeasureString(ownerText);
			var ownerTextScreen = new Vector2(planetScreen.X - ownerTextSize.X / 2.0f, planetScreen.Y - ownerTextSize.Y / 2.0f);
			var ownerTextColor = Planet.Owner != null ? Planet.Owner.Color.XnaColor : Color.Gray;
			batch.DrawString(InfoFont, ownerText, ownerTextScreen + OwnerTextOffset, ownerTextColor);

			if (Planet.FleetChange != 0)
			{
				var changeText = string.Format("{0}{1}", Planet.FleetChange > 0 ? "+" : "", Planet.FleetChange);
				var changeTextColor = Planet.FleetChange > 0 ? Color.Green : Color.Red;
				batch.DrawString(InfoFont, changeText, fleetsTextScreen + FleetsDeltaTextOffset, changeTextColor);
			}

			if (showDetails)
			{
				// planet name
				var nameTextSize = InfoFont.MeasureString(Planet.Name);
				var nameTextScreen = new Vector2(planetScreen.X - nameTextSize.X / 2.0f, planetScreen.Y - nameTextSize.Y / 2.0f);
				batch.DrawString(InfoFont, Planet.Name, nameTextScreen + NameTextOffset, Color.Yellow);

				// fleets income
				var fleetsIncomeText = string.Format("+{0}", Planet.BaseUnitsPerTurn);
				batch.DrawString(InfoFont, fleetsIncomeText, fleetsTextScreen + FleetsIncomeTextOffset, Color.White);
			}
		}
		public IndicatorVisual GetIndicator(int targetPlanetId)
		{
			return _indicators.First(iv => iv.TargetPlanet.Id == targetPlanetId);
		}
	}
}
