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
			public ExtendedListRow()
			{
				Bounds = new UniRectangle(new UniScalar(), new UniScalar(), new UniScalar(1.0f, 0), new UniScalar());
			}

			public void AddSegment(Control control, UniScalar width)
			{
				var leftBorder = GetLastElementHorizontalEnd();
				control.Bounds = new UniRectangle(leftBorder, new UniScalar(), width, new UniScalar(1.0f, 0));
				Children.Add(control);
			}

			private UniScalar GetLastElementHorizontalEnd()
			{
				if (Children.Count == 0) return UniScalar.Zero;

				return Children.OrderBy((control) => control.Bounds.Right.Fraction).Last().Bounds.Right;				
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
			row.Bounds.Top = new UniScalar(_elementHeight.Fraction * Children.Count, _elementHeight.Offset * Children.Count);
			row.Bounds.Bottom = row.Bounds.Top + _elementHeight;
			Children.Add(row);						
		}

		public void Clear()
		{
			Children.Clear();
		}

		private UniScalar _elementHeight;
	}
}
