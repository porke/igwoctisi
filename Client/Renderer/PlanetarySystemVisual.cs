namespace Client.Renderer
{
	using Client.Model;
	using Client.Renderer.Particles;
	using Client.Renderer.Particles.ParticleSystems;
	using Microsoft.Xna.Framework;
	using Microsoft.Xna.Framework.Graphics;

	public sealed class PlanetarySystemVisual
	{
		public PlanetarySystemConvexParticleSystem ParticleSystem { get; private set; }

		public Color Color
		{
			get { return _color; } 
			set { _color = value; ParticleSystem.AnimateColorChange(value, 1.75f); }
		}

		public bool Visible
		{
			get { return ParticleSystem.Visible; }
			set { ParticleSystem.Visible = value; }
		}
		
		private Color _color;
		private PlanetarySystem _planetarySystem;
		public Player _lastOwner;


		public PlanetarySystemVisual(GameClient client, PlanetarySystem planetarySystem)
		{
			_planetarySystem = planetarySystem;
			ParticleSystem = new PlanetarySystemConvexParticleSystem(client, client.Content, planetarySystem.Bounds);
		}

		public void Update(GraphicsDevice device, ICamera camera, Map map, double delta, double time)
		{
			ParticleSystem.SetCamera(camera.GetView(), camera.Projection);

			// Update color due to 
			var player = map.GetPlanetById(_planetarySystem.Planets[0]).Owner;
			bool foundOwner = player != null;
			if (player != null)
			{
				for (int i = 0; i < _planetarySystem.Planets.Length; ++i)
				{
					var planet = map.GetPlanetById(_planetarySystem.Planets[i]);
					if (planet.Owner != player)
						foundOwner = false;
				}
			}

			if (foundOwner && _lastOwner != player)
			{
				// TODO animate color change
				Color = player.Color.XnaColor;
			}
			else
			{
				Color = Color.FromNonPremultiplied(10, 10, 10, 255);
			}
		}
	}
}
