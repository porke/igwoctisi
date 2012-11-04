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

            float progress = (float)Progress;
            Context.Position = new Vector3(
                progress * _targetPosition.X + (1.0f - progress) * _startingPosition.X,
                progress * _targetPosition.Y + (1.0f - progress) * _startingPosition.Y,
                progress * _targetPosition.Z + (1.0f - progress) * _startingPosition.Z);
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

        public static MoveTo<Spaceship> MoveTo(this Animation<Spaceship> animation, Vector3 targetPosition, double duration, Func<double, double> interpolator = null)
        {
            return animation.MoveTo<Spaceship>(targetPosition, duration, interpolator);
        }
    
	}
}
