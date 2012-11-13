namespace Client.Renderer.Controls
{
	using Client.View.Controls;
	using Nuclex.UserInterface.Visuals.Flat;

	public class FlatIconControlRenderer : IFlatControlRenderer<IconControl>
	{
		public void Render(IconControl control, IFlatGuiGraphics graphics)
		{
			graphics.DrawElement(control.IconName, control.GetAbsoluteBounds());
		}
	}
}
