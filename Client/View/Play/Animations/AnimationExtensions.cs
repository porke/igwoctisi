namespace Client.View.Play.Animations
{
	using System;
	using Client.Common.AnimationSystem;
	using Client.Common.AnimationSystem.DefaultAnimations;
	using Client.Renderer;
	using Microsoft.Xna.Framework;
	using Client.Model;

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

		public static Wait<SimpleCamera> Wait(this Animation<SimpleCamera> animation, double duration)
		{
			return animation.Wait<SimpleCamera>(duration);
		}

		public static void Shake(this Animation<SimpleCamera> camera, float duration)
		{
			//TODO camera shake animation
		}

		#endregion

		#region Spaceship animations

		public static Animation<Spaceship> Animate(this Spaceship ship, AnimationManager animationManager)
		{
			return Animate<Spaceship>(ship, animationManager);
		}
		
		public static MoveTo<Spaceship> MoveTo(this Animation<Spaceship> animation, Vector3 targetPosition, double duration,
			Func<double, double> interpolator = null)
		{
			return animation.MoveTo<Spaceship>(targetPosition, duration, interpolator);
		}

		public static Rotate<Spaceship> Rotate(this Animation<Spaceship> animation, Vector3 rotateAxis, float startingAngle, float targetAngle,
			float duration, Func<double, double> interpolator = null)
		{
			return animation.Rotate<Spaceship>(rotateAxis, startingAngle, targetAngle, duration, interpolator);
		}

		public static Wait<Spaceship> Wait(this Animation<Spaceship> animation, double duration)
		{
			return animation.Wait<Spaceship>(duration);
		}

		public static InterpolateTo<Spaceship> InterpolateTo(this Animation<Spaceship> animation, double targetValue, double duration,
			Func<double, double> interpolator, Func<Spaceship, double> startingValueGetter, Action<Spaceship, double> valueChanger)
		{
			return animation.InterpolateTo<Spaceship>(targetValue, duration, interpolator, startingValueGetter, valueChanger);
		}

		#endregion
	}
}
