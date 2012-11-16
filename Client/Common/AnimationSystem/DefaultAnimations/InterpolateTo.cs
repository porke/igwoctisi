namespace Client.Common.AnimationSystem.DefaultAnimations
{
	using System;

	public class InterpolateTo<T> : Animation<T>
	{
		private readonly Func<T, double> _startingValueGetter;
		private readonly Action<T, double> _valueChanger;
		private double _startingValue;
		private readonly double _targetValue;


		public InterpolateTo(T context, AnimationManager animationManager,
			double targetValue, double duration, Func<double, double> interpolator,
			Func<T, double> startingValueGetter, Action<T, double> valueChanger)
			: base (context, animationManager, duration, interpolator)
		{
			_startingValueGetter = startingValueGetter;
			_valueChanger = valueChanger;
			_targetValue = targetValue;
		}

		public override void Begin()
		{
			base.Begin();

			_startingValue = _startingValueGetter(Context);
		}

		public override void Update(double delta)
		{
			base.Update(delta);

			_valueChanger(Context, (1.0 - Progress) * _startingValue + Progress * _targetValue);
		}

		public override void End()
		{
			_valueChanger(Context, _targetValue);

			base.End();
		}
	}

	public static class InterpoalteToExtensions
	{
		public static InterpolateTo<T> InterpolateTo<T>(this Animation<T> animation, double targetValue, double duration,
			Func<double, double> interpolator, Func<T, double> startingValueGetter, Action<T, double> valueChanger)
		{
			var after = new InterpolateTo<T>(animation.Context, animation.AnimationMgr, targetValue, duration, interpolator, startingValueGetter, valueChanger);
			animation.AddAfter(after);
			return after;
		}
	}
}
