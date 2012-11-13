namespace Client.View.Controls
{
	using Nuclex.UserInterface.Controls;

	public class IconControl : Control
	{
		public IconControl()
		{			
			IconName = string.Empty;
		}

		public IconControl(string iconName)
		{
			IconName = iconName;
		}

		public string IconName { get; set; }
	}
}
