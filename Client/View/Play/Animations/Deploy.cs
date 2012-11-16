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
						.MoveTo(targetPlanet.Position, 1.25, Interpolators.Accelerate(2.5))
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
