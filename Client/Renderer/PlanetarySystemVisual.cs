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
		public ParticleSystem ParticleSystem { get; private set; }

		public Color Color
		{
			get { return _color; } 
			set { _color = value; UpdateColor(); }
		}

		public bool Visible
		{
			get { return ParticleSystem.Visible; }
			set { ParticleSystem.Visible = value; }
		}

		private Point3[] _keyPoints;
		private int _lastKeyPoint;
		private Color _color;


		public PlanetarySystemVisual(PlanetarySystem planetarySystem,Game Game, ContentManager Content, Point3[] keyPoints)
		{
			_keyPoints = keyPoints;
			_lastKeyPoint = 0;
			ParticleSystem = new PlanetarySystemConvexParticleSystem(Game, Content);
		}

		public void Update(GraphicsDevice device, ICamera camera, double delta, double time)
		{
			int index = _lastKeyPoint;
			var position1 = _keyPoints[index].ToVector3();
			index = index + 1 == _keyPoints.Length ? 0 : index + 1;
			var position2 = _keyPoints[index].ToVector3();
			var velocity = position2 - position1;

			ParticleSystem.SetCamera(camera.GetView(), camera.Projection);
			ParticleSystem.AddParticle(position1, velocity);
			_lastKeyPoint = index;
		}

		private void UpdateColor()
		{
			ParticleSystem.SetMinColor(_color);
			ParticleSystem.SetMaxColor(_color);
		}
	}
}
