namespace Client.Common.AnimationSystem.DefaultAnimations
{
	using System;
	using Microsoft.Xna.Framework;

	public class Rotate<T> : Animation<T>
		where T : ITransformable
	{
		private Vector3 _rotateAxis;
		private float _startingAngle;
		private float _targetAngle;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="context"></param>
		/// <param name="animationManager"></param>
		/// <param name="rotateAxis"></param>
		/// <param name="startingAngle">degrees</param>
		/// <param name="targetAngle">degrees</param>
		/// <param name="duration"></param>
		/// <param name="interpolator"></param>
		public Rotate(T context, AnimationManager animationManager, Vector3 rotateAxis, float startingAngle, float targetAngle,
			float duration, Func<double, double> interpolator)
			: base (context, animationManager, duration, interpolator)
		{
			_rotateAxis = rotateAxis;
			_startingAngle = MathHelper.ToRadians(startingAngle);
			_targetAngle = MathHelper.ToRadians(targetAngle);
		}

		public override void Update(double delta)
		{
			base.Update(delta);

			Context.Rotation = Matrix.CreateFromAxisAngle(_rotateAxis, MathHelper.Lerp(_startingAngle, _targetAngle, (float)Progress));
		}

		public override void End()
		{
			Context.Rotation = Matrix.CreateFromAxisAngle(_rotateAxis, _targetAngle);

			base.End();
		}
	}

	public static class RotateExtensions
	{
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="animation"></param>
		/// <param name="rotateAxis"></param>
		/// <param name="startingAngle">degrees</param>
		/// <param name="targetAngle">degrees</param>
		/// <param name="duration"></param>
		/// <param name="interpolator"></param>
		/// <returns></returns>
		public static Rotate<T> Rotate<T>(this Animation<T> animation, Vector3 rotateAxis, float startingAngle, float targetAngle,
			float duration, Func<double, double> interpolator = null)
			where T : ITransformable
		{
			var after = new Rotate<T>(animation.Context, animation.AnimationMgr, rotateAxis, startingAngle, targetAngle, duration, interpolator);
			animation.AddAfter(after);
			return after;
		}
	}
}
