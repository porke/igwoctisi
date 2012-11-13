namespace Client.Common.AnimationSystem
{
	using System;

	public class CompoundAnimation<T> : Animation<T>
	{
		private readonly Action<CompoundAnimation<T>> _compoundAnimation;

		public CompoundAnimation(T context, AnimationManager animationManager, double duration, Action<CompoundAnimation<T>> compoundAnimation)
			: base(context, animationManager, duration, Interpolators.Linear())
		{
			_compoundAnimation = compoundAnimation;
		}

		public override void Begin()
		{
			base.Begin();

			_compoundAnimation.Invoke(this);
		}
	}

	public static class CompoundAnimationExtensions
	{
		public static CompoundAnimation<T> Compound<T>(this Animation<T> animation, double duration, Action<CompoundAnimation<T>> compoundAnimation)
		{
			var after = new CompoundAnimation<T>(animation.Context, animation.AnimationMgr, duration, compoundAnimation);
			animation.AddAfter(after);
			return after;
		}
	}
}
