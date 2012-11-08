namespace Client.View.Play.Animations
{
    using System;
    using Client.Common.AnimationSystem;
    using Client.Common.AnimationSystem.DefaultAnimations;
    using Client.Model;
    using Client.Renderer;
    using System.Threading;
    using System.Collections.Generic;


    public static class DeployAnimation
    {
        //public static void AnimateDeployAsync(this Spaceship ship, Planet targetPlanet, int newFleetsCount, Action onEndCallback)
        //{
        //    ThreadPool.QueueUserWorkItem(obj =>
        //    {
        //        AnimateDeploy(ship, targetPlanet, newFleetsCount);
        //        onEndCallback.Invoke();
        //    });
        //}

        private static Random rand = new Random();

        public static void AnimateDeploy(this Spaceship ship, AnimationManager animationManager, SimpleCamera camera,
            Planet targetPlanet, int newFleetsCount, Action onEndCallback)
        {
            // Camera quake 5 arena 2
            camera.Animate(animationManager)
                  .Shake(1);

            // Deploy from the camera
            ship.Position = camera.Position;
            ship.Animate(animationManager)
                .MoveTo(targetPlanet.Position, 1.75f,
                        Interpolators.AccelerateDecelerate())
                .AddCallback(s => onEndCallback());
        }
    }
}
