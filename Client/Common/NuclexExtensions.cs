using Nuclex.UserInterface.Controls;
using Client.View;
using Client.View.Animations;


namespace Client.Common
{
	public static class NuclexExtensions
	{
		public static Animation<Control> Animate(this Control control, BaseView view)
		{
			var animationMgr = view.ViewMgr.AnimationManager;
			var animation = new Wait<Control>(control, animationMgr, 0);
			animationMgr.AddAnimation(animation);
			return animation;
		}
	}
}
