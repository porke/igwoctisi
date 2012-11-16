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
            // Camera quake 5 arena 2
            camera.Animate(animationManager)
                .Wait(0.25);
                //.Shake(0.4);

			float deploySpeedFactor = camera.Min.Z > 4500 ? 0.80f : 1.25f;
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
							c.MoveTo(targetPlanet.Position, deployDuration, Interpolators.Decelerate());

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
