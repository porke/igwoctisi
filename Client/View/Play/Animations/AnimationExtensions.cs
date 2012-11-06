namespace Client.View.Play.Animations
{
	using Client.Common.AnimationSystem;
	using Client.Renderer;
	using Client.Common.AnimationSystem.DefaultAnimations;
	using Microsoft.Xna.Framework;
	using System;

	public static class AnimationExtensions
	{
		public static Animation<Spaceship> Animate(this Spaceship ship, AnimationManager animationManager)
		{
			var animation = Animation<Spaceship>.Dummy(ship, animationManager);
			animationManager.AddAnimation(animation);
			return animation;
		}
		
		public static MoveTo<Spaceship> MoveTo(this Animation<Spaceship> animation, Vector3 targetPosition, double duration, Func<double, double> interpolator = null)
		{
			return animation.MoveTo<Spaceship>(targetPosition, duration, interpolator);
		}
	}
}
