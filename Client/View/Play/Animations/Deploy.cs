namespace Client.View.Play.Animations
{
    using System;
    using Client.Common.AnimationSystem;
    using Client.Common.AnimationSystem.DefaultAnimations;
    using Client.Model;
    using Client.Renderer;

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

        public static void AnimateDeploy(this Spaceship ship, AnimationManager animationManager, Planet targetPlanet, int newFleetsCount, Action onEndCallback)
        {
            ship.Animate(animationManager)
                .MoveTo(targetPlanet.Position, 1)
                .AddCallback(s => onEndCallback());
        }
    }
}
