namespace Client.Renderer.Particles.ParticleSystems
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Microsoft.Xna.Framework;
	using Microsoft.Xna.Framework.Content;
	using Microsoft.Xna.Framework.Graphics;


	class PlanetarySystemConvexParticleSystem : ParticleSystem
	{
		public PlanetarySystemConvexParticleSystem(Game game, ContentManager content)
			: base(game, content)
		{
		}

		public void SetColor(Color color)
		{
			base.SetMinColor(color);
			base.SetMaxColor(color);
		}

		protected override void InitializeSettings(ParticleSettings settings)
		{
			settings.TexturePath = @"Textures\Effects\Flare";

			settings.MaxParticles = 2400;

			settings.Duration = TimeSpan.FromSeconds(2);

			settings.DurationRandomness = 1;

			settings.MinHorizontalVelocity = 0;
			settings.MaxHorizontalVelocity = 15;

			settings.MinVerticalVelocity = -10;
			settings.MaxVerticalVelocity = 10;

			settings.Gravity = new Vector3(0, 0, 0);

			settings.MinColor = new Color(255, 255, 255, 10);
			settings.MaxColor = new Color(255, 255, 255, 40);

			settings.MinStartSize = 5;
			settings.MaxStartSize = 10;

			settings.MinEndSize = 80;
			settings.MaxEndSize = 80;

			// Use additive blending.
			settings.BlendState = BlendState.Additive;
		}
	}
}
