namespace Client.View.Controls
{
	using Nuclex.UserInterface.Controls;
	using System.Collections.Generic;
	using Nuclex.UserInterface;
	using System.Linq;

	public class ExtendedListControl : IconControl
	{
		#region Internal class - row

		public class ExtendedListRow : Control
		{
			public void AddSegment(Control control, UniScalar width)
			{
				var leftBorder = GetLastElementHorizontalEnd();
				control.Bounds = new UniRectangle(leftBorder, new UniScalar(), width, new UniScalar());
				Children.Add(control);
			}

			public void SetElementHeight(UniScalar height)
			{
				for (int i = 0; i < Children.Count; ++i)
				{
					Children[i].Bounds.Top = i * height;
					Children[i].Bounds.Bottom = i * (height + 1);
				}
			}

			private UniScalar GetLastElementHorizontalEnd()
			{
				return Children.OrderBy((control) => control.Bounds.Right).Last().Bounds.Right;				
			}
		}

		#endregion	

		public ExtendedListControl(UniScalar elementHeight, string panelBackground)
			: base(panelBackground)
		{
			_elementHeight = elementHeight;
			IconFrameName = panelBackground;
		}

		public void AddRow(ExtendedListRow row)
		{
			Children.Add(row);
		}

		private UniScalar _elementHeight;
	}
}
