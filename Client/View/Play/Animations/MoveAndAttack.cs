namespace Client.View.Play.Animations
{
	using System;
	using System.Collections.Generic;
	using Client.Common.AnimationSystem;
	using Client.Model;
	using Client.Renderer;
	using System.Threading;
	using System.Diagnostics;
	using Microsoft.Xna.Framework;
	
	public static class MoveAndAttack
	{
		public static void AnimateMovesAndAttacks(this SceneVisual scene,
			IList<Tuple<Planet, Planet, SimulationResult, Action<SimulationResult>>> movesAndAttacks,
			AnimationManager animationManager, SimpleCamera camera)
		{
			ThreadPool.QueueUserWorkItem(obj =>
			{
				var waiter = new ManualResetEvent(true);
				foreach (var tpl in movesAndAttacks)
				{
					var sourcePlanet = tpl.Item1;
					var targetPlanet = tpl.Item2;
					var simResult = tpl.Item3;
					var callback = tpl.Item4;

					waiter.Reset();
					if (simResult.Type == SimulationResult.MoveType.Move)
					{
						AnimateMove(sourcePlanet, targetPlanet, simResult, scene, animationManager, camera, waiter);
					}
					else
					{
						Debug.Assert(simResult.Type == SimulationResult.MoveType.Attack);
						AnimateAttack(sourcePlanet, targetPlanet, simResult, scene, animationManager, camera, waiter);
					}
					waiter.WaitOne();
					callback.Invoke(simResult);
				}
			});
		}

		private static void AnimateMove(Planet sourcePlanet, Planet targetPlanet, SimulationResult simResult,
			SceneVisual scene, AnimationManager animationManager, SimpleCamera camera, ManualResetEvent waiter)
		{
			var ship = Spaceship.Acquire(sourcePlanet.Owner.Color);
			scene.AddSpaceship(ship);

			ship.SetPosition(sourcePlanet.Position);
			ship.LookAt(targetPlanet.Position);
			ship.Animate(animationManager)
				//.MoveTo(targetPlanet.Position, 2, Interpolators.AccelerateDecelerate())
				.Compound(2.0, c =>
				{
					c.InterpolateTo(targetPlanet.Position.X, 2.0, Interpolators.AccelerateDecelerate(),
						(s) => s.X,
						(s, x) => { s.X = (float)x; });
					c.InterpolateTo(targetPlanet.Position.Y, 2.0, Interpolators.Decelerate(),
						(s) => s.Y,
						(s, y) => { s.Y = (float)y; });
					c.InterpolateTo(targetPlanet.Position.Z, 1.5, Interpolators.OvershootInterpolator(),
						(s) => s.Z,
						(s, z) => { s.Z = (float)z; });
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
			var ship = Spaceship.Acquire(sourcePlanet.Owner.Color);
			scene.AddSpaceship(ship);

			ship.SetPosition(sourcePlanet.Position);
			ship.LookAt(targetPlanet.Position);
			ship.Animate(animationManager)
				.MoveTo(targetPlanet.Position, 2, Interpolators.AccelerateDecelerate())
				.AddCallback(s =>
				{
					Spaceship.Recycle(s);
					waiter.Set();
				});
		}
	}
}
