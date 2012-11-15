namespace Client.View.Controls
{
	using System;
	using Nuclex.UserInterface;
	using Nuclex.UserInterface.Controls.Desktop;

	public class ImageChoiceControl : ChoiceControl
	{
		public string[] States = new string[8];

		/// <summary>
		/// Creates the image choice controls (associates appropriate styles)
		/// </summary>
		/// <param name="onFrameNames">Frame names in the given order: disabled, normal, highlighted, depressed</param>
		/// <param name="offFrameNames">Frame names in the given order: disabled, normal, highlighted, depressed</param>
		public ImageChoiceControl(string[] onFrameNames, string[] offFrameNames)
		{
			for (int i = 0; i < 4; ++i)
			{
				States[i] = offFrameNames[i];
				States[i + 4] = onFrameNames[i];
			}
		}
	}
}
