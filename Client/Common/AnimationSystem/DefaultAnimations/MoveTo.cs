namespace Client.Common.AnimationSystem.DefaultAnimations
{
    using System;
    using Client.Renderer;
    using Microsoft.Xna.Framework;

	public class MoveTo<T> : Animation<T>
        where T : IMovable
	{
        Vector3 _targetPosition;
        Vector3 _startingPosition;
        

		public MoveTo(T context, AnimationManager animationMgr, Vector3 targetPosition, double duration, Func<double, double> interpolator)
			: base(context, animationMgr, duration, interpolator)
		{
            _targetPosition = targetPosition;
		}

        public override void Begin()
        {
            base.Begin();

            _startingPosition = Context.Position;
        }

        public override void Update(double delta)
        {
            base.Update(delta);

            Context.Position = Vector3.Lerp(_startingPosition, _targetPosition, (float)Progress);
        }

        public override void End()
        {
            Context.Position = _targetPosition;

            base.End();
        }
	}

	public static class MoveToExtensions
	{
		public static MoveTo<T> MoveTo<T>(this Animation<T> animation, Vector3 targetPosition, double duration, Func<double, double> interpolator = null)
            where T : IMovable
        {
            var after = new MoveTo<T>(animation.Context, animation.AnimationMgr, targetPosition, duration, interpolator);
            animation.AddAfter(after);
            return after;
        }
    
	}
}
