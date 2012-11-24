namespace Client.Renderer.Controls
{
	using Nuclex.UserInterface.Visuals.Flat;
	using Client.View.Controls;

	public class FlatExtendedListControl : IFlatControlRenderer<ExtendedListControl.ExtendedListRow>
	{
		public void Render(ExtendedListControl.ExtendedListRow control, IFlatGuiGraphics graphics)
		{
			// Do nothing, ExtendedListRow is a compound control
		}
	}
}
