namespace Client.View.Controls
{
	using Nuclex.UserInterface.Controls.Desktop;
	using Nuclex.UserInterface;
	using System;

	public class ImageCheckButtonControl : OptionControl
	{
		public ImageCheckButtonControl(string checkedFrame, string uncheckedFrame)
		{
			_checkedIcon = new IconControl()
			{
				Bounds = new UniRectangle(new UniScalar(), new UniScalar(), new UniScalar(1.0f, 0), new UniScalar(1.0f, 0)),
				IconFrameName = checkedFrame
			};
			
			_uncheckedIcon = new IconControl
			{
				Bounds = new UniRectangle(new UniScalar(), new UniScalar(), new UniScalar(1.0f, 0), new UniScalar(1.0f, 0)),
				IconFrameName = uncheckedFrame
			};

			OnChanged();
		}

		protected override void OnChanged()
		{
			base.OnChanged();
			Console.WriteLine(Selected);
			Children.Clear();
			if (!Selected)
			{
				Children.Add(_uncheckedIcon);
			}
			else
			{
				Children.Add(_checkedIcon);
			}
		}

		private IconControl _checkedIcon;
		private IconControl _uncheckedIcon;
	}
}
