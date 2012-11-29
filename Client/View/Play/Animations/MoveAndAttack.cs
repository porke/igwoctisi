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
			float waitDuration = 0.4f;
			float duration2 = 1;
			float surrenderRotateDuration = 0.5f;
			float surrenderMoveDuration = duration2 - surrenderRotateDuration;
			int attackerFleetsBack = simResult.AttackerFleetsBack;
			bool surrender = attackerFleetsBack > 0;
			bool totallyLost = simResult.FleetCount - simResult.AttackerLosses == 0;

			// temporary variables
			Vector3 rotateCenter, cameraDir;
			float previousAngle = 0;
			Matrix beginningRotation;

			scene.AddSpaceship(ship);
			ship.SetPosition(sourcePosition);
			ship.LookAt(targetPosition, Vector3.Forward);

			ship.Animate(animationManager)
				.MoveTo(targetPosition - direction * (targetPlanet.Radius + ship.Length), duration1, Interpolators.Decelerate(1.4))
				.Wait(waitDuration)
				.Compound(duration2, c =>
				{
					if (surrender)
					{
						cameraDir = Vector3.Normalize(camera.GetPosition() - ship.GetPosition());
						rotateCenter = ship.GetPosition() + new Vector3(0, 0, cameraDir.Z * ship.Length * 2);

						c.Interpolate(surrenderRotateDuration, Interpolators.Linear(),
							(s, percent) =>
							{
								// Start turning back
								var previousPosition = s.GetPosition();
								var virtualPos = previousPosition - rotateCenter;
								float angle = (float)percent * MathHelper.ToRadians(180);
								float angleDiff = previousAngle - angle;

								var oldTransform = s.CalculateWorldTransform();
								var rotation = Matrix.CreateFromAxisAngle(s.Rotation.Right, angleDiff);
								previousAngle = angle;

								var newVirtualPos = Vector3.Transform(virtualPos, rotation);
								s.SetPosition(newVirtualPos + rotateCenter);
								s.Rotation *= rotation;
							})
							.AddCallbackCompound(surrenderMoveDuration, (s, cc) =>
							{
								// Roll 180 degrees
								var rollStartRotation = Matrix.CreateFromAxisAngle(ship.GetLook(), 0);
								var rollTargetRotation = Matrix.CreateFromAxisAngle(ship.GetLook(), MathHelper.ToRadians(180));

								beginningRotation = s.Rotation;
								cc.Interpolate(surrenderMoveDuration / 2, Interpolators.OvershootInterpolator(),
									(s2, percent) =>
									{
										s2.Rotation = beginningRotation * Matrix.Lerp(rollStartRotation, rollTargetRotation, (float)percent);
									});

								// Move to source planet
								cc.MoveTo(sourcePosition, surrenderMoveDuration);
							});
					}
					else if (totallyLost)
					{
						// We have lost our only ships. Make them disappear.
						c.InterpolateTo(0, duration2, Interpolators.SinusOscillator(5),
							(s) => 1.0f,
							(s, v) => s.Opacity = (float)v
						);
					}
					else
					{
						// Move to target planet
						c.MoveTo(targetPosition, duration2);
					}
				})
				.AddCallback(s =>
				{
					Spaceship.Recycle(s);
					waiter.Set();
				});
		}
	}
}
