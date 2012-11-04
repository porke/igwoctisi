namespace Client.Common
{
	using Nuclex.UserInterface.Controls;
	using Client.View;
	using Client.Common.AnimationSystem;
	using Client.Common.AnimationSystem.DefaultAnimations;
	using Nuclex.UserInterface;
	using System;

	public static class NuclexExtensions
	{
		public static void Lerp(ref UniScalar value1, ref UniScalar value2, float amount, out UniScalar result)
		{
			result.Offset = value1.Offset * (1.0f - amount) + value2.Offset * amount;
			result.Fraction = value1.Fraction * (1.0f - amount) + value2.Offset * amount;
		}

		public static Animation<Control> Animate(this Control control, BaseView view)
		{
			var animationMgr = view.ViewMgr.AnimationManager;
			var animation = new Wait<Control>(control, animationMgr, 0);
			animationMgr.AddAnimation(animation);
			return animation;
		}
	}
}
