namespace Client.View.Controls
{
	using Nuclex.UserInterface.Controls;

	public class IconControl : Control
	{
		public IconControl()
		{			
			IconFrameName = string.Empty;
		}

		public IconControl(string iconName)
		{
			IconFrameName = iconName;
		}

		public string IconFrameName { get; set; }
	}
}
