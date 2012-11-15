namespace Client.Common
{
	using Client.Common.AnimationSystem;
	using Client.Common.AnimationSystem.DefaultAnimations;
	using Client.View;
	using Nuclex.UserInterface;
	using Nuclex.UserInterface.Controls;

	public static class NuclexExtensions
	{
		public static void Lerp(ref UniScalar value1, ref UniScalar value2, float amount, out UniScalar result)
		{
			result.Offset = value1.Offset * (1.0f - amount) + value2.Offset * amount;
			result.Fraction = value1.Fraction * (1.0f - amount) + value2.Fraction * amount;
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
