namespace Client.Renderer.Particles.ParticleSystems
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Microsoft.Xna.Framework;
	using Microsoft.Xna.Framework.Content;
	using Microsoft.Xna.Framework.Graphics;
	using Client.Model;


	public class PlanetarySystemConvexParticleSystem : ParticleSystem
	{
		private Vector3[] _keyPoints;
		private int _index;
		private float _animationAmount;
		private Vector3 _previousPosition;

		private const float _timeFactor = 2.5f;


		public PlanetarySystemConvexParticleSystem(Game game, ContentManager content, Point3[] keyPoints)
			: base(game, content)
		{
			_keyPoints = keyPoints.Select(p => p.ToVector3()).ToArray();
			_index = 0;
		}

		public void SetColor(Color color)
		{
			base.SetMinColor(color);
			base.SetMaxColor(color);
		}

		protected override void InitializeSettings(ParticleSettings settings)
		{
			settings.TexturePath = @"Textures\Effects\Flare";

			settings.MaxParticles = 4400;

			settings.Duration = TimeSpan.FromSeconds(8);

			settings.DurationRandomness = 0.1f;

			settings.MinHorizontalVelocity = 0;
			settings.MaxHorizontalVelocity = 5;

			settings.MinVerticalVelocity = 0;
			settings.MaxVerticalVelocity = 5;

			settings.Gravity = new Vector3(0, 0, 0);
			
			settings.MinStartSize = 5;
			settings.MaxStartSize = 10;

			settings.MinEndSize = 80;
			settings.MaxEndSize = 80;

			// Use additive blending.
			settings.BlendState = BlendState.Additive;
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			int n = _keyPoints.Length;
			int index = _index;

			var p1 = _keyPoints[index];
			if (index < n - 1) ++index; else index = 0;
			var position = Vector3.CatmullRom(
				p1,
				_keyPoints[index < n ? index++ : (index = 0)],
				_keyPoints[index < n ? index++ : (index = 0)],
				_keyPoints[index < n ? index : 0],
				_animationAmount
			);
			var velocity = Vector3.Normalize(_previousPosition - position);
			
			AddParticle(position, velocity);

			_previousPosition = position;
			_animationAmount += (float)gameTime.ElapsedGameTime.TotalSeconds * _timeFactor;
			if (_animationAmount >= 1.0f)
			{
				_animationAmount = 0;

				_index++;
				if (_index >= n)
					_index = 0;
			}
		}
	}
}
