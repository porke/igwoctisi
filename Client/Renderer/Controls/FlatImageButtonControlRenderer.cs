namespace Client.Renderer.Controls
{
	using Nuclex.UserInterface.Visuals.Flat;
	using Client.View.Controls;

	public class FlatImageButtonControlRenderer : IFlatControlRenderer<ImageButtonControl>
	{
		public void Render(ImageButtonControl control, IFlatGuiGraphics graphics)
		{
			var controlBounds = control.GetAbsoluteBounds();

			// Determine the style to use for the button
			int stateIndex = 0;
			if (control.Enabled)
			{
				if (control.Depressed)
				{
					stateIndex = 3;
				}
				else if (control.MouseHovering)
				{
					stateIndex = 2;
				}
				else
				{
					stateIndex = 1;
				}
			}

			// Draw the button's frame
			graphics.DrawElement(control.StateFrames[stateIndex], controlBounds);
		}
	}
}
