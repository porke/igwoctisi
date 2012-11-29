namespace Client.View.Play.Animations
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Threading;
	using Client.Common.AnimationSystem;
	using Client.Model;
	using Client.Renderer;
	using System.Diagnostics;

    public static class DeployAnimation
    {
        private static Random rand = new Random();

        public static void AnimateDeploys(this SceneVisual scene, AnimationManager animationManager, SimpleCamera camera,
			IList<Tuple<Planet, int, Action, Action>> deploys)
        {
			var bw = new BackgroundWorker();
			bw.DoWork += new DoWorkEventHandler((sender, workArgs) =>
			{
				var waiter = new ManualResetEventSlim(false);
				foreach (var deploy in deploys)
				{
					Planet targetPlanet = deploy.Item1;
					int newFleetsCount = deploy.Item2;
					Action onDeployStart = deploy.Item3;
					Action onDeployEnd = deploy.Item4;
					Player player = targetPlanet.Owner;
					
					onDeployStart();
					waiter.Reset();
					camera.Animate(animationManager)
						.MoveToDeploy(targetPlanet)
						.AddCallback(action =>
						{
							moveSpaceship(targetPlanet, newFleetsCount, waiter, player, scene, animationManager, camera);
						});
					waiter.Wait();
					onDeployEnd.Invoke();
				}
			});
			bw.RunWorkerAsync();
        }

		private static void moveSpaceship(Planet targetPlanet, int newFleetsCount, ManualResetEventSlim waiter, Player player,
			SceneVisual scene, AnimationManager animationManager, SimpleCamera camera)
		{
			bool performShipRotate = rand.Next() % 3 == 0;
			float deploySpeedFactor = camera.Z > 4500 ? 1.45f : 1.05f;
			float deployDuration = deploySpeedFactor * Math.Abs(camera.Min.Z / 2000);

			var ship = Spaceship.Acquire(SpaceshipModelType.LittleSpaceship, player.Color);
			scene.AddSpaceship(ship);

			// Camera quake 5 arena 2
			if (performShipRotate)
				camera.Animate(animationManager)
					.Wait(0.25)
					.Shake(0.4, 40);

			ship.Animate(animationManager)
				.Compound(deployDuration, c =>
				{
					// Set start position to the camera position
					ship.SetPosition(camera.GetPosition());
					ship.LookAt(targetPlanet.Visual.GetPosition(), camera.GetUpVector());

					// Move spaceship to the target deploy planet
					c.MoveTo(targetPlanet.Position, deployDuration, Interpolators.Decelerate());

					// Rotating that happens randomly...
					if (performShipRotate)
						c.Rotate(ship.GetLook(), 0, 360, 1, Interpolators.OvershootInterpolator());

					// Fade in and fade out
					c.InterpolateTo(1, deployDuration / 5, Interpolators.OvershootInterpolator(),
						(s) => 0,
						(s, o) => { s.Opacity = (float)o; })
					.Wait(deployDuration / 5 * 2)
					.InterpolateTo(0, deployDuration / 5, Interpolators.Decelerate(1.4),
						(s) => 1,
						(s, o) => { s.Opacity = (float)o; });

				})
				.AddCallback(s =>
				{
					waiter.Set();
					Spaceship.Recycle(s);
				});
		}
	}
}
