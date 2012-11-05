namespace Client.View.Play.Animations
{
	using Client.Common.AnimationSystem;
	using Client.Renderer;

    public static class AnimationExtensions
    {
        public static Animation<Spaceship> Animate(this Spaceship ship, AnimationManager animationManager)
        {
            return Animation<Spaceship>.Dummy(ship, animationManager);
        }
    }
}
