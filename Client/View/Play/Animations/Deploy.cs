namespace Client.View.Play.Animations
{
    using System;
    using Client.Common.AnimationSystem;
    using Client.Model;
    using Client.Renderer;
    using System.Collections.Generic;
    using System.Threading;


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
            
            // Deploy one ship for one planet coming from the camera origin
            foreach (var deploy in deploys)
            {
                Planet targetPlanet = deploy.Item1;
                int newFleetsCount = deploy.Item2;
                Action onDeployEnd = deploy.Item3;

                var ship = Spaceship.Acquire(targetPlanet.Owner.Color);
                scene.AddSpaceship(ship);

                ship.Position = camera.Position;
                ship.Animate(animationManager)
                    .Wait(rand.NextDouble() % 0.5)
                    .MoveTo(targetPlanet.Position, 0.75, Interpolators.Accelerate(2.5))
                    .AddCallback(s =>
                    {
                        onDeployEnd.Invoke();
                        Spaceship.Recycle(s);
                    });
            }
        }
    }
}
