namespace Client.Common.AnimationSystem.DefaultAnimations
{
	using System;
	using Nuclex.UserInterface;
	using Nuclex.UserInterface.Controls;

	public class MoveControlTo : Animation<Control>
	{
		private UniVector _start;
		private UniVector _end;

		private UniVector _size;

		public override void Begin()
		{
			base.Begin();
		}

		public override void Update(double delta)
		{
			base.Update(delta);
			
			UniScalar currentX = new UniScalar();
			NuclexExtensions.Lerp(ref _start.X, ref _end.X, (float)Progress, out currentX);
			Context.Bounds.Left = currentX;

			UniScalar currentY = new UniScalar();
			NuclexExtensions.Lerp(ref _start.Y, ref _end.Y, (float)Progress, out currentY);
			Context.Bounds.Top = currentY;
		}

		public override void End()
		{
			Context.Bounds.Left = _end.X;
			Context.Bounds.Top = _end.Y;

			base.End();			
		}

		public MoveControlTo(Control context, UniVector destPosition, AnimationManager animationMgr, double duration, Func<double, double> interpolator)
			: base(context, animationMgr, duration, interpolator) 
		{
			_start = new UniVector();
			_start.X = context.Bounds.Left;
			_start.Y = context.Bounds.Top;
			_end = destPosition;
		}
	}

	public static class MoveControlToExtensions
	{		
		public static MoveControlTo MoveControlTo<T>(this Animation<T> animation, UniVector destPosition, double duration = 0.5, Func<double, double> interpolator = null) where T : Control
		{
			var after = new MoveControlTo(animation.Context, destPosition, animation.AnimationMgr, duration, interpolator);
			animation.AddAfter(after);
			return after;
		}
	}
}
