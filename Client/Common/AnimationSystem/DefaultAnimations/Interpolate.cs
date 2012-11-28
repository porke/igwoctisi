namespace Client.Common.AnimationSystem.DefaultAnimations
{
	using System;

	public class Interpolate<T> : Animation<T>
	{
		private readonly Action<T, double> _valueChanger;


		public Interpolate(T context, AnimationManager animationManager, double duration,
			Func<double, double> interpolator, Action<T, double> valueChanger)
			: base(context, animationManager, duration, interpolator)
		{
			_valueChanger = valueChanger;
		}
		
		public override void Update(double delta)
		{
			base.Update(delta);

			_valueChanger(Context, Progress);
		}

		public override void End()
		{
			_valueChanger(Context, 1.0);

			base.End();
		}
	}

	public static class InterpolateExtensions
	{
		public static Interpolate<T> Interpolate<T>(this Animation<T> animation, double duration,
			Func<double, double> interpolator, Action<T, double> valueChanger)
		{
			var after = new Interpolate<T>(animation.Context, animation.AnimationMgr, duration, interpolator, valueChanger);
			animation.AddAfter(after);
			return after;
		}
	}
}
