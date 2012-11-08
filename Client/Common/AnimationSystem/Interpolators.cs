namespace Client.Common.AnimationSystem
{
	using System;

	public static class Interpolators
	{
		/// <summary>
		/// A very simple interpolator which provides a linear progression by just returning the current input.
		/// </summary>
		/// <returns></returns>
		public static Func<double, double> Linear()
		{
			return t => t;
		}

		/// <summary>
		/// An interpolator where the changes start backwards and then spring forward as the time progresses.
		/// </summary>
		/// <param name="tension">tension the tension controlling the rate spring effect of the animation</param>
		/// <returns></returns>
		public static Func<double, double> Anticipate(double tension = 2.0)
		{
			return t => t * t * ((tension + 1) * t - tension);
		}

		/// <summary>
		/// An interpolator where the rate of change starts out slowly and then accelerates over time.
		/// </summary>
		/// <param name="factor"></param>
		/// <returns></returns>
		public static Func<double, double> Accelerate(double factor = 1.0)
		{
			return t =>
			{
				if (factor >= 1.0f)
				{
					return t*t;
				}
				else
				{
					return (float) Math.Pow(t, factor*2);
				}
			};
		}

		/// <summary>
		/// An interpolator where the rate of change starts out fast and then decelerates over time.
		/// </summary>
		/// <param name="factor"></param>
		/// <returns></returns>
		public static Func<double, double> Decelerate(double factor = 1.0)
		{
			return t =>
			{
				if (factor >= 1.0)
				{
					return 1.0 - (1.0 - t)*(1.0 - t);
				}
				else
				{
					return (float) (1.0 - Math.Pow((1.0 - t), factor*2));
				}
			};
		}

		/// <summary>
		/// An interpolator where the rate of change starts out slowly, grows over time and ends slowly.
		/// </summary>
		/// <returns></returns>
		public static Func<double, double> AccelerateDecelerate()
		{
		    return t => Math.Cos((t + 1)*Math.PI)/2.0 + 0.5;
		}

	    /// <summary>
		/// An interpolator where the change overshoots the target and springs back to the target position.
		/// </summary>
		/// <param name="factor">rate of overshoot.</param>
		/// <returns></returns>
		public static Func<double, double> OvershootInterpolator(double factor = 1.0)
		{
			return t =>
			{
				t -= 1.0f;
				return t*t*((factor + 1)*t + factor) + 1.0;
			};
		}
	}
}
