namespace Client.Renderer
{
    using Microsoft.Xna.Framework.Graphics;
	using Client.Model;
	using Microsoft.Xna.Framework;

    public class PlanetVisual
    {
		public Planet Planet { get; set; }
        public float Period { get; set; }
        public float Yaw { get; set; }
        public float Pitch { get; set; }
        public float Roll { get; set; }
		public Effect Effect { get; set; }
        public Texture2D DiffuseTexture { get; set; }
        public Texture2D CloudsTexture { get; set; }
        public Texture2D CloudsAlphaTexture { get; set; }
		public VertexBuffer VB { get; set; }

		public void Draw(GraphicsDevice device, Matrix view, Matrix projection, double time, float ambient, Color glow)
		{
			var localWorld = Matrix.CreateScale(Planet.Radius) *
							Matrix.CreateRotationY((float)time / Period * MathHelper.TwoPi) *
							Matrix.CreateFromYawPitchRoll(Yaw, Pitch, Roll) *
							Matrix.CreateTranslation(Planet.X, Planet.Y, Planet.Z);

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
    }
}
