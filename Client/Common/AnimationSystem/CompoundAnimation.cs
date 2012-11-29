namespace Client.Common.AnimationSystem
{
	using System;

	public class CompoundAnimation<T> : Animation<T>
	{
		private readonly Action<Animation<T>> _compoundAnimation;
		private readonly Animation<T> _previousAnimation;

		public CompoundAnimation(T context, AnimationManager animationManager, double duration, Action<Animation<T>> compoundAnimation, Animation<T> previousAnimation)
			: base(context, animationManager, duration, Interpolators.Linear())
		{
			_compoundAnimation = compoundAnimation;
			_previousAnimation = previousAnimation;
		}

		public override void Begin()
		{
			base.Begin();

			_compoundAnimation.Invoke(_previousAnimation);
		}
	}

	public static class CompoundAnimationExtensions
	{
		public static Animation<T> Compound<T>(this Animation<T> animation, double duration, Action<Animation<T>> compoundAnimation)
		{
			var after = new CompoundAnimation<T>(animation.Context, animation.AnimationMgr, duration, compoundAnimation, animation);
			animation.AddAfter(after);
			return after;
		}
	}
}
