namespace Client.Renderer.Controls
{
	using Client.View.Controls;
	using Nuclex.UserInterface.Visuals.Flat;

	public class FlatChoiceControlRenderer : IFlatControlRenderer<ImageChoiceControl> 
	{
		public void Render(ImageChoiceControl control, IFlatGuiGraphics graphics)
		{
			int stateIndex = (control.Selected ? 4 : 0);
			if(control.Enabled) 
			{
				if(control.Depressed) 
				{
					stateIndex += 3;
				}
				else if(control.MouseHovering) 
				{
					stateIndex += 2;
				}
				else 
				{
					stateIndex += 1;
				}
			}
	
			var controlBounds = control.GetAbsoluteBounds();
			graphics.DrawElement(control.States[stateIndex], controlBounds);
		}
	}
}
