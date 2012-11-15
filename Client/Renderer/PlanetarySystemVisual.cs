namespace Client.Renderer
{
	using Client.Model;
	using Client.Renderer.Particles;
	using Client.Renderer.Particles.ParticleSystems;
	using Microsoft.Xna.Framework;
	using Microsoft.Xna.Framework.Content;
	using Microsoft.Xna.Framework.Graphics;

	public sealed class PlanetarySystemVisual
	{
		public PlanetarySystemConvexParticleSystem ParticleSystem { get; private set; }

		public Color Color
		{
			get { return _color; } 
			set { _color = value; ParticleSystem.SetColor(value); }
		}

		public bool Visible
		{
			get { return ParticleSystem.Visible; }
			set { ParticleSystem.Visible = value; }
		}
		
		private Color _color;


		public PlanetarySystemVisual(PlanetarySystem planetarySystem,Game Game, ContentManager Content, Point3[] keyPoints)
		{
			ParticleSystem = new PlanetarySystemConvexParticleSystem(Game, Content, keyPoints);
		}

		public void Update(GraphicsDevice device, ICamera camera, double delta, double time)
		{
			ParticleSystem.SetCamera(camera.GetView(), camera.Projection);
		}
	}
}
