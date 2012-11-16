namespace Client.View.Controls
{
	using Nuclex.UserInterface.Controls.Desktop;
	using Nuclex.UserInterface.Visuals.Flat;
	using Nuclex.UserInterface;

	public class ImageButtonControl : ButtonControl
	{
		public string[] StateFrames = new string[4];

		public ImageButtonControl(string disabledFrame, string normalFrame, string highlighedFrame, string depressedFrame)
		{
			StateFrames[0] = disabledFrame;
			StateFrames[1] = normalFrame;
			StateFrames[2] = highlighedFrame;
			StateFrames[3] = depressedFrame;
		}
	}
}
