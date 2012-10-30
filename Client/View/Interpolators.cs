using System;


namespace Client.View
{
	public static class Interpolators
	{
		public static Func<double, double> Linear()
		{
			return x => x;
		}
		public static Func<double, double> AccelerateDecelerate(double factor = 2, double doubledFactor = 2)
		{
			return x => Math.Cos((x + 1) * Math.PI) / 2.0 + 0.5;
		}
	}
}
