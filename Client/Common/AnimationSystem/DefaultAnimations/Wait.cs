namespace Client.Common.AnimationSystem.DefaultAnimations
{
	public class Wait<T> : Animation<T>
	{
		public Wait(T context, AnimationManager animationMgr, double duration)
			: base(context, animationMgr, duration, Interpolators.Linear())
		{
		}
	}

	public static class WaitExtensions
	{
		public static Wait<T> Wait<T>(this Animation<T> animation, double duration)
		{
			var after = new Wait<T>(animation.Context, animation.AnimationMgr, duration);
			animation.AddAfter(after);
			return after;
		}
	}
}
