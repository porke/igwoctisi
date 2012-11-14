namespace Client.Renderer
{
    using Microsoft.Xna.Framework.Graphics;
	using Client.Model;
	using Microsoft.Xna.Framework;

    public class PlanetVisual
    {
		public static readonly Vector2 NameTextOffset = new Vector2(0, 25.0f);
		public static readonly Vector2 FleetsTextOffset = new Vector2(0.0f, 42.0f);
		public static readonly Vector2 FleetsIncomeTextOffset = new Vector2(21.0f, 42.0f);
		public static readonly Vector2 FleetsDeltaTextOffset = new Vector2(21, -42.0f);
		public static readonly Vector2 OwnerTextOffset = new Vector2(0, 59.0f);

		public Planet Planet { get; set; }
        public float Period { get; set; }
        public float Yaw { get; set; }
        public float Pitch { get; set; }
        public float Roll { get; set; }
		public Effect Effect { get; set; }
		public SpriteFont InfoFont { get; set; }
        public Texture2D DiffuseTexture { get; set; }
        public Texture2D CloudsTexture { get; set; }
        public Texture2D CloudsAlphaTexture { get; set; }
		public VertexBuffer VB { get; set; }

		public void Draw(GraphicsDevice device, ICamera camera, double time, float ambient, Color glow)
		{
			var localWorld = Matrix.CreateScale(Planet.Radius) *
							Matrix.CreateRotationY((float)time / Period * MathHelper.TwoPi) *
							Matrix.CreateFromYawPitchRoll(Yaw, Pitch, Roll) *
							Matrix.CreateTranslation(Planet.X, Planet.Y, Planet.Z);
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

			foreach (var pass in Effect.CurrentTechnique.Passes)
			{
				pass.Apply();
				device.SetVertexBuffer(VB);
				device.DrawPrimitives(PrimitiveType.TriangleList, 0, VB.VertexCount / 3);
			}
		}
		public void DrawInfo(GraphicsDevice device, SpriteBatch batch, ICamera camera, bool showDetails)
		{
			var planetScreen = camera.Project(device.Viewport, Planet.Position);

			// owner name
			var ownerText = Planet.Owner != null ? Planet.Owner.Username : string.Empty;
			var ownerTextSize = InfoFont.MeasureString(ownerText);
			var ownerTextScreen = new Vector2(planetScreen.X - ownerTextSize.X/2.0f, planetScreen.Y - ownerTextSize.Y/2.0f);
			var ownerTextColor = Planet.Owner != null ? Planet.Owner.Color.XnaColor : Color.Gray;
			batch.DrawString(InfoFont, ownerText, ownerTextScreen + OwnerTextOffset, ownerTextColor);

			// planet name
			var nameTextSize = InfoFont.MeasureString(Planet.Name);
			var nameTextScreen = new Vector2(planetScreen.X - nameTextSize.X / 2.0f, planetScreen.Y - nameTextSize.Y / 2.0f);
			batch.DrawString(InfoFont, Planet.Name, nameTextScreen + NameTextOffset, Color.Yellow);

			// number of fleets
			var fleetsText = Planet.NumFleetsPresent.ToString();
			var fleetsTextSize = InfoFont.MeasureString(fleetsText);
			var fleetsTextScreen = new Vector2(planetScreen.X - fleetsTextSize.X / 2.0f, planetScreen.Y - fleetsTextSize.Y / 2.0f);
			batch.DrawString(InfoFont, fleetsText, fleetsTextScreen + FleetsTextOffset, Color.Yellow);

			if (Planet.FleetChange != 0)
			{
				var changeText = string.Format("{0}{1}", Planet.FleetChange > 0 ? "+" : "", Planet.FleetChange);
				var changeTextColor = Planet.FleetChange > 0 ? Color.Green : Color.Red;
				batch.DrawString(InfoFont, changeText, fleetsTextScreen + FleetsDeltaTextOffset, changeTextColor);
			}

			if (showDetails)
			{
				// fleets income
				var fleetsIncomeText = string.Format("+{0}", Planet.BaseUnitsPerTurn);
				batch.DrawString(InfoFont, fleetsIncomeText, fleetsTextScreen + FleetsIncomeTextOffset, Color.White);
			}
		}
    }
}
