namespace Client.View.Play.Animations
{
    using System;
    using Client.Common.AnimationSystem;
    using Client.Model;
    using Client.Renderer;
    using System.Collections.Generic;
    using System.Threading;
	using Microsoft.Xna.Framework;


    public static class DeployAnimation
    {
        private static Random rand = new Random();

        public static void AnimateDeploys(this SceneVisual scene, AnimationManager animationManager, SimpleCamera camera,
            IList<Tuple<Planet, int, Action>> deploys)
        {
			bool performRotateShip = rand.Next() % 3 == 0;

            // Camera quake 5 arena 2
			if (performRotateShip)
            camera.Animate(animationManager)
                .Wait(0.25);
                //.Shake(0.4);

			float deploySpeedFactor = camera.Z > 4500 ? 1.75f : 1.25f;
			float deployDuration = deploySpeedFactor * Math.Abs(camera.Min.Z / 2000);

			ThreadPool.QueueUserWorkItem(obj =>
			{
				var waiter = new ManualResetEvent(true);
				foreach (var deploy in deploys)
				{
					Planet targetPlanet = deploy.Item1;
					int newFleetsCount = deploy.Item2;
					Action onDeployEnd = deploy.Item3;

					waiter.Reset();
					var ship = Spaceship.Acquire(targetPlanet.Owner.Color);
					scene.AddSpaceship(ship);

					ship.SetPosition(camera.GetPosition());
					ship.LookAt(targetPlanet.Position, Vector3.Up);
					ship.Animate(animationManager)
						.Compound(deployDuration, c =>
						{
							// Move spaceship to the target deploy planet
							c.MoveTo(targetPlanet.Position, deployDuration, Interpolators.Decelerate());

							// Rotating that happens randomly...
							if (performRotateShip)
								c.Rotate(ship.GetLook(), 0, 360, 1, Interpolators.OvershootInterpolator());

							// Fade in and fade out
							c.InterpolateTo(1, deployDuration / 5, Interpolators.OvershootInterpolator(),
								(s) => 0,
								(s, o) => { s.Opacity = (float)o; })
							.Wait(deployDuration / 5 * 2)
							.InterpolateTo(0, deployDuration / 5 * 2, Interpolators.Decelerate(1.4),
								(s) => 1,
								(s, o) => { s.Opacity = (float)o; });

						})
						.AddCallback(s =>
						{
							onDeployEnd.Invoke();
							Spaceship.Recycle(s);
							waiter.Set();
						});
					waiter.WaitOne();
				}
			});
        }
    }
}
