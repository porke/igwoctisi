namespace Client.View.Play.Animations
{
	using System;
	using Client.Common.AnimationSystem;
	using Client.Common.AnimationSystem.DefaultAnimations;
	using Client.Renderer;
	using Microsoft.Xna.Framework;

	public static class AnimationExtensions
	{
		private static Animation<T> Animate<T>(this T obj, AnimationManager animationManager)
		{
			var animation = Animation<T>.Dummy(obj, animationManager);
			animationManager.AddAnimation(animation);
			return animation;
		}

		#region Camera animations

		public static Animation<SimpleCamera> Animate(this SimpleCamera camera, AnimationManager animationManager)
		{
			return Animate<SimpleCamera>(camera, animationManager);
		}

		#endregion

		#region Spaceship animations

		public static Animation<Spaceship> Animate(this Spaceship ship, AnimationManager animationManager)
		{
			return Animate<Spaceship>(ship, animationManager);
		}
		
		public static MoveTo<Spaceship> MoveTo(this Animation<Spaceship> animation, Vector3 targetPosition, double duration, Func<double, double> interpolator = null)
		{
			return animation.MoveTo<Spaceship>(targetPosition, duration, interpolator);
		}

		#endregion
	}
}
