namespace Client.View.Controls
{
	using System;
	using System.Collections.Generic;
	using Client.Common;
	using Nuclex.UserInterface;
	using Nuclex.UserInterface.Controls;
	using Nuclex.UserInterface.Controls.Desktop;

	public class TabbedPaneControl : Control
	{
		public enum TabHeaderPosition
		{
			Top,
			Left
		}

		public Control ActiveTab
		{
			get
			{
				return (_contentPanel.Children.Count > 0) ? _contentPanel.Children[0] : null;
			}
			private set
			{
				_contentPanel.Children.Add(value);
			}
		}
		public int TabCount
		{
			get
			{
				return _tabs.Count;
			}
		}

		public TabbedPaneControl(UniRectangle bounds, TabHeaderPosition tabHeaderPosition = TabHeaderPosition.Top)
		{
			Bounds = bounds;
			_tabHeaderPosition = tabHeaderPosition;
			_tabHeaderPanel = new IconControl("topPanel");
			_contentPanel = new IconControl("topPanel");
			Children.AddRange(new Control[] { _tabHeaderPanel, _contentPanel });
			_contentPanel.Bounds = new UniRectangle(new UniScalar(), new UniScalar(), new UniScalar(1.0f, 0), new UniScalar(1.0f, 0));
			_tabHeaderPanel.Bounds = new UniRectangle(new UniScalar(), new UniScalar(), new UniScalar(40), new UniScalar(40));
		}

		public void AddTab(string tabText, Control content)
		{
			var tab = new TabHeaderControl
			{
				Text = tabText,
				ActivatedTab = content
			};
			if (_tabHeaderPosition == TabHeaderPosition.Top)
			{
				tab.Bounds = new UniRectangle(new UniScalar(8 + 32 * TabCount), new UniScalar(8), new UniScalar(24), new UniScalar(24));
				_contentPanel.Bounds = new UniRectangle(new UniScalar(), new UniScalar(0.0f, 40), new UniScalar(1.0f, 0.0f), new UniScalar(1.0f, -40));
				_tabHeaderPanel.Bounds.Right = new UniScalar(_tabHeaderPanel.Bounds.Right.Fraction, 40 * (TabCount + 1));
			}
			else if (_tabHeaderPosition == TabHeaderPosition.Left)
			{
				tab.Bounds = new UniRectangle(new UniScalar(8), new UniScalar(8 + 32 * TabCount), new UniScalar(24), new UniScalar(24));
				_contentPanel.Bounds = new UniRectangle(new UniScalar(0.0f, 40), new UniScalar(), new UniScalar(1.0f, -40.0f), new UniScalar(1.0f, 0.0f));
				_tabHeaderPanel.Bounds.Bottom = new UniScalar(_tabHeaderPanel.Bounds.Bottom.Fraction, 40 * (TabCount + 1));
			}

			tab.Pressed += SwitchTab;
			_tabHeaderPanel.Children.Add(tab);
			_tabs.Add(content);

			if (ActiveTab == null)
			{
				SwitchTab(tab, null);
			}
		}

		private void SwitchTab(object sender, EventArgs e)
		{
			var tabHeader = sender as TabHeaderControl;
			_contentPanel.Children.Remove(ActiveTab);
			ActiveTab = tabHeader.ActivatedTab;
		}

		private TabHeaderPosition _tabHeaderPosition;
		private IconControl _tabHeaderPanel;
		private IconControl _contentPanel;
		private List<Control> _tabs = new List<Control>();
	}
}
