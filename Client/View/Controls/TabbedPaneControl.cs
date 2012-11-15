namespace Client.View.Controls
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Client.Common;
	using Nuclex.UserInterface;
	using Nuclex.UserInterface.Controls;
	using Nuclex.UserInterface.Controls.Desktop;

	public class TabbedPaneControl : Control
	{
		#region Internal class - TabTextHeaderControl

		private class TabTextHeaderControl : ButtonControl
		{
			public Control ActivatedTab { get; set; }
		}

		#endregion

		#region Internal class - TabImageHeaderControl

		

		#endregion

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
			_tabHeaderPanel.Bounds = new UniRectangle(new UniScalar(), new UniScalar(), new UniScalar(TabHeaderWidth), new UniScalar(TabHeaderHeight));
		}

		public void AddTab(string tabText, Control content)
		{
			var tab = new TabTextHeaderControl
			{
				Text = tabText,
				ActivatedTab = content
			};

			float tabWidth = tabText.Length * 12 + 10;
			float maxTabWidth = (_tabHeaderPanel.Children.Count > 0) ? _tabHeaderPanel.Children.Max(bc => (bc as TabTextHeaderControl).Text.Length) * 8 : 0;
			tabWidth = tabWidth > maxTabWidth ? tabWidth : maxTabWidth;

			if (_tabHeaderPosition == TabHeaderPosition.Top)
			{
				tab.Bounds = new UniRectangle(new UniScalar(TabPadding + 32 * TabCount), new UniScalar(TabPadding), new UniScalar(tabWidth), new UniScalar(24));
				_contentPanel.Bounds = new UniRectangle(new UniScalar(), new UniScalar(TabHeaderWidth), new UniScalar(1.0f, 0.0f), new UniScalar(1.0f, -TabHeaderHeight));
				_tabHeaderPanel.Bounds.Right = new UniScalar(_tabHeaderPanel.Bounds.Right.Fraction, (tabWidth + 16) * (TabCount + 1));
			}
			else if (_tabHeaderPosition == TabHeaderPosition.Left)
			{
				tab.Bounds = new UniRectangle(new UniScalar(TabPadding), new UniScalar(TabPadding + 32 * TabCount), new UniScalar(tabWidth), new UniScalar(24));
				_contentPanel.Bounds = new UniRectangle(new UniScalar(TabHeaderWidth), new UniScalar(), new UniScalar(1.0f, -TabHeaderHeight), new UniScalar(1.0f, 0.0f));
				_tabHeaderPanel.Bounds.Bottom = new UniScalar(_tabHeaderPanel.Bounds.Bottom.Fraction, TabHeaderHeight * (TabCount + 1));
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
			var tabHeader = sender as TabTextHeaderControl;
			_contentPanel.Children.Remove(ActiveTab);
			ActiveTab = tabHeader.ActivatedTab;
		}

		private TabHeaderPosition _tabHeaderPosition;
		private IconControl _tabHeaderPanel;
		private IconControl _contentPanel;
		private List<Control> _tabs = new List<Control>();

		private const int TabPadding = 8;
		private const int TabHeaderHeight = 40;
		private const int TabHeaderWidth = 40;
	}
}
