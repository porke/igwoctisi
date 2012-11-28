namespace Client.View.Play.Animations
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Threading;
	using Client.Common.AnimationSystem;
	using Client.Model;
	using Client.Renderer;
	using Microsoft.Xna.Framework;
	using System.ComponentModel;

	public static class MoveAndAttack
	{
		public static void AnimateMovesAndAttacks(this SceneVisual scene,
			IList<Tuple<Planet, Planet, SimulationResult, Action, Action<SimulationResult>>> movesAndAttacks,
			AnimationManager animationManager, SimpleCamera camera)
		{
			var bw = new BackgroundWorker();
			bw.DoWork += new DoWorkEventHandler((sender, workArgs) =>
			{
				foreach (var tpl in movesAndAttacks)
				{
					var sourcePlanet = tpl.Item1;
					var targetPlanet = tpl.Item2;
					var simResult = tpl.Item3;
					var onActionStart = tpl.Item4;
					var onActinEnd = tpl.Item5;

					var waiter = new ManualResetEvent(false);

					onActionStart();
					camera.Animate(animationManager)
						.MoveToAttack(sourcePlanet, targetPlanet)
						.AddCallback(action =>
						{
							if (simResult.Type == SimulationResult.MoveType.Move)
							{
								AnimateMove(sourcePlanet, targetPlanet, simResult, scene, animationManager, camera, waiter);
							}
							else
							{
								AnimateAttack(sourcePlanet, targetPlanet, simResult, scene, animationManager, camera, waiter);
							}
						});
					waiter.WaitOne();
					onActinEnd.Invoke(simResult);
				}
			});
			bw.RunWorkerAsync();
		}

		private static void AnimateMove(Planet sourcePlanet, Planet targetPlanet, SimulationResult simResult,
			SceneVisual scene, AnimationManager animationManager, SimpleCamera camera, ManualResetEvent waiter)
		{
			var player = sourcePlanet.Owner;
			var sourcePosition = sourcePlanet.Visual.GetPosition();
			var targetPosition = targetPlanet.Visual.GetPosition();
			var direction = Vector3.Normalize(targetPosition - sourcePosition);
			sourcePosition += direction * sourcePlanet.Radius;
			targetPosition -= direction * targetPlanet.Radius;

			var ship = Spaceship.Acquire(SpaceshipModelType.LittleSpaceship, player.Color);

			const float shipSpeedFactor = 0.015f;
			float moveDuration = (targetPosition - sourcePosition).Length() * shipSpeedFactor;
			float fadeDuration = ship.Length * shipSpeedFactor;

			scene.AddSpaceship(ship);
			ship.SetPosition(sourcePosition);
			ship.LookAt(targetPosition, Vector3.Forward);
			ship.Animate(animationManager)
				.Compound(moveDuration, c =>
				{
					// Move
					c.MoveTo(targetPlanet.Visual.GetPosition(), moveDuration, Interpolators.AccelerateDecelerate());

					// Fade in and fade out
					c.InterpolateTo(1, fadeDuration, Interpolators.Accelerate(),
						(s) => 0,
						(s, o) => { s.Opacity = (float)o; }
					)
					.Wait(moveDuration - 2 * fadeDuration)
					.InterpolateTo(0, fadeDuration, Interpolators.Decelerate(1.4),
						(s) => 1,
						(s, o) => { s.Opacity = (float)o; }
					);
				})
				.AddCallback(s =>
				{
					Spaceship.Recycle(s);
					waiter.Set();
				});
		}

		private static void AnimateAttack(Planet sourcePlanet, Planet targetPlanet, SimulationResult simResult,
			SceneVisual scene, AnimationManager animationManager, SimpleCamera camera, ManualResetEvent waiter)
		{
			var sourcePosition = sourcePlanet.Visual.GetPosition();
			var targetPosition = targetPlanet.Visual.GetPosition();
			var direction = Vector3.Normalize(targetPosition - sourcePosition);
			var ship = Spaceship.Acquire(SpaceshipModelType.LittleSpaceship, sourcePlanet.Owner.Color);
			
			
			const float shipSpeedFactor = 0.007f;
			float duration1 = (targetPosition - sourcePosition).Length() * shipSpeedFactor;
			scene.AddSpaceship(ship);
			ship.SetPosition(sourcePosition);
			ship.LookAt(targetPosition, Vector3.Forward);
			float waitDuration = 0.4f;
			float duration2 = 1;

			ship.Animate(animationManager)
				.MoveTo(targetPosition - direction * (targetPlanet.Radius * 2 + ship.Length), duration1, Interpolators.Decelerate(1.4))
				.Wait(waitDuration)
				.Compound(duration2, c =>
				{
					c.MoveTo(targetPosition, duration2);
				})
				.AddCallback(s =>
				{
					Spaceship.Recycle(s);
					waiter.Set();
				});
		}
	}
}
