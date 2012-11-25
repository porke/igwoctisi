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
		/// <summary>
		/// Sender: This.
		/// EventArgs: empty.
		/// </summary>
		public event EventHandler Toggled;
		
		/// <summary>
		/// Sender: Activated tab.
		/// EventArgs: empty.
		/// </summary>
		public event EventHandler TabChanged;

		#region Internal interface - TabHeaderContainer

		private interface TabHeaderContainer
		{
			Control ActivatedTab { get; set; }
		}

		#endregion

		#region Internal class - TabTextHeaderControl

		protected internal class TabTextHeaderControl : ButtonControl, TabHeaderContainer
		{
			public event EventHandler Activated
			{
				add { Pressed += value; }
				remove { Pressed -= value; }
			}

			public Control ActivatedTab { get; set; }
		}

		#endregion

		#region Internal class - TabImageHeaderControl

		protected internal class TabImageHeaderControl : ImageChoiceControl, TabHeaderContainer
		{
			/// <summary>
			/// Creates the image choice controls (associates appropriate styles)
			/// </summary>
			/// <param name="onFrameNames">Frame names in the given order: disabled, normal, highlighted, depressed</param>
			/// <param name="offFrameNames">Frame names in the given order: disabled, normal, highlighted, depressed</param>
			public TabImageHeaderControl(string[] onFrameNames, string[] offFrameNames)
				: base(onFrameNames, offFrameNames)
			{
				Changed += HandleChanged;
			}
			
			public event EventHandler Activated;
			public Control ActivatedTab { get; set; }

			private void HandleChanged(object sender, EventArgs e)
			{
				if (Selected && Activated != null)
				{
					Activated(sender, e);
				}
			}
		}

		#endregion

		#region Internal enum - TabHeaderPosition

		public enum TabHeaderPosition
		{
			Top,
			Left
		}

		#endregion		

		public UniVector DefaultPosition { get; set; }
		public UniVector TogglePosition { get; set; }		
		public bool IsToggled { get; set; }
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
			_tabHeaderPanel = new IconControl(_tabHeaderPosition == TabHeaderPosition.Left ? "right_tab_background" : "chat_tab_background");
			_contentPanel = new IconControl("hud_background");
			Children.AddRange(new Control[] { _tabHeaderPanel, _contentPanel });
			_contentPanel.Bounds = new UniRectangle(new UniScalar(), new UniScalar(), new UniScalar(1.0f, 0), new UniScalar(1.0f, 0));
			_tabHeaderPanel.Bounds = new UniRectangle(new UniScalar(), new UniScalar(), new UniScalar(TabHeaderWidth), new UniScalar(TabHeaderHeight));
			
			_hidePanelButton = new ImageButtonControl();
			_hidePanelButton.Pressed += TogglePanel;
			_hidePanelButton.Text = string.Empty;

			if (tabHeaderPosition == TabHeaderPosition.Left)
			{
				_hidePanelButton.Bounds = new UniRectangle(new UniScalar((TabHeaderWidth - HideButtonMaxSize) / 2), new UniScalar(1.0f, -TabPadding - HideButtonMinSize), new UniScalar(HideButtonMaxSize), new UniScalar(HideButtonMinSize));
			}
			else
			{
				_hidePanelButton.Bounds = new UniRectangle(new UniScalar(1.0f, -TabPadding - HideButtonMinSize), new UniScalar((TabHeaderWidth - HideButtonMaxSize) / 2), new UniScalar(HideButtonMinSize), new UniScalar(HideButtonMaxSize));
			}
			_tabHeaderPanel.Children.Add(_hidePanelButton);
			UpdateHideButtonStyle();
		}

		// TODO: fix awful code duplication in AddTab
		public void AddTab(string tabText, Control content)
		{
			var tab = new TabTextHeaderControl
			{
				Text = tabText,
				ActivatedTab = content
			};

			float tabWidth = tabText.Length * 12 + 10;
			float maxTabWidth = (_tabHeaderPanel.Children.Count > 0) ? _tabHeaderPanel.Children.Max(bc => (bc as ButtonControl).Text.Length) * 8 : 0;
			tabWidth = tabWidth > maxTabWidth ? tabWidth : maxTabWidth;

			if (_tabHeaderPosition == TabHeaderPosition.Top)
			{
				tab.Bounds = new UniRectangle(new UniScalar(TabPadding + 32 * TabCount), new UniScalar(TabPadding), new UniScalar(tabWidth), new UniScalar(24));
				_contentPanel.Bounds = new UniRectangle(new UniScalar(), new UniScalar(TabHeaderWidth), new UniScalar(1.0f, 0.0f), new UniScalar(1.0f, -TabHeaderHeight));
				_tabHeaderPanel.Bounds.Right = new UniScalar(_tabHeaderPanel.Bounds.Right.Fraction, (tabWidth + 16) * (TabCount + 1) + HideButtonMinSize + TabPadding);
			}
			else if (_tabHeaderPosition == TabHeaderPosition.Left)
			{
				tab.Bounds = new UniRectangle(new UniScalar(TabPadding), new UniScalar(TabPadding + 32 * TabCount), new UniScalar(tabWidth), new UniScalar(24));
				_contentPanel.Bounds = new UniRectangle(new UniScalar(TabHeaderWidth), new UniScalar(), new UniScalar(1.0f, -TabHeaderHeight), new UniScalar(1.0f, 0.0f));
				_tabHeaderPanel.Bounds.Bottom = new UniScalar(_tabHeaderPanel.Bounds.Bottom.Fraction, TabHeaderHeight * (TabCount + 1) + HideButtonMinSize + TabPadding);
			}

			tab.Activated += SwitchTab;
			_tabHeaderPanel.Children.Add(tab);
			_tabs.Add(content);

			if (ActiveTab == null)
			{
				SwitchTab(tab, null);
			}
		}

		/// <summary>
		/// Adds an image tab.
		/// </summary>
		/// <param name="onFrameNames">Frame names in the given order: disabled, normal, highlighted, depressed</param>
		/// <param name="offFrameNames">Frame names in the given order: disabled, normal, highlighted, depressed</param>
		public void AddTab(string[] onFrames, string[] offFrames, Control content)
		{
			// TODO: the tab width is actually calculated in a pretty ugly way
			var tab = new TabImageHeaderControl(onFrames, offFrames)
			{
				ActivatedTab = content
			};

			float tabWidth = 32;
			if (_tabHeaderPosition == TabHeaderPosition.Top)
			{
				tab.Bounds = new UniRectangle(new UniScalar(TabPadding + (32 + TabSpacing) * TabCount), new UniScalar(TabPadding), new UniScalar(tabWidth), new UniScalar(24));
				_contentPanel.Bounds = new UniRectangle(new UniScalar(), new UniScalar(TabHeaderWidth), new UniScalar(1.0f, 0.0f), new UniScalar(1.0f, -TabHeaderHeight));
				_tabHeaderPanel.Bounds.Right = new UniScalar(_tabHeaderPanel.Bounds.Right.Fraction, (tabWidth + 16) * (TabCount + 1) + HideButtonMinSize + TabPadding);
			}
			else if (_tabHeaderPosition == TabHeaderPosition.Left)
			{
				tab.Bounds = new UniRectangle(new UniScalar(TabPadding / 2), new UniScalar(TabPadding + (32 + TabSpacing) * TabCount), new UniScalar(tabWidth), new UniScalar(24));
				_contentPanel.Bounds = new UniRectangle(new UniScalar(TabHeaderWidth), new UniScalar(), new UniScalar(1.0f, -TabHeaderHeight), new UniScalar(1.0f, 0.0f));
				_tabHeaderPanel.Bounds.Bottom = new UniScalar(_tabHeaderPanel.Bounds.Bottom.Fraction, TabHeaderHeight * (TabCount + 1) + HideButtonMinSize + TabPadding);
			}

			tab.Activated += SwitchTab;
			_tabHeaderPanel.Children.Add(tab);
			_tabs.Add(content);

			if (ActiveTab == null)
			{
				SwitchTab(tab, null);
				(_tabHeaderPanel.Children[1] as TabImageHeaderControl).Selected = true;
			}
		}

		private void SwitchTab(object sender, EventArgs e)
		{
			var tabHeader = sender as TabHeaderContainer;

			if (ActiveTab != tabHeader.ActivatedTab)
			{
				_contentPanel.Children.Remove(ActiveTab);
				ActiveTab = tabHeader.ActivatedTab;

				if (TabChanged != null)
				{
					TabChanged(ActiveTab, e);
				}
			}
		}

		private void TogglePanel(object sender, EventArgs args)
		{
			IsToggled = !IsToggled;
			UpdateHideButtonStyle();

			if (Toggled != null)
			{
				Toggled(this, args);
			}
		}

		private void UpdateHideButtonStyle()
		{			
			string frameNormal;

			if (_tabHeaderPosition == TabHeaderPosition.Left)
			{
				frameNormal = IsToggled ? "arrowLeft" : "arrowRight";
			}
			else
			{
				frameNormal = IsToggled ? "arrowUp" : "arrowDown";
			}

			_hidePanelButton.StateFrames[0] = string.Format("{0}{1}", frameNormal, "Normal");
			_hidePanelButton.StateFrames[1] = string.Format("{0}{1}", frameNormal, "Normal");
			_hidePanelButton.StateFrames[2] = string.Format("{0}{1}", frameNormal, "Pressed");
			_hidePanelButton.StateFrames[3] = string.Format("{0}{1}", frameNormal, "Hover");
		}

		private TabHeaderPosition _tabHeaderPosition;
		private ImageButtonControl _hidePanelButton;		
		private IconControl _contentPanel;
		private List<Control> _tabs = new List<Control>();
		protected IconControl _tabHeaderPanel;

		private const int TabPadding = 8;
		private const int TabSpacing = 5;
		private const int TabHeaderHeight = 40;
		private const int TabHeaderWidth = 40;
		private const int HideButtonMaxSize = 32;
		private const int HideButtonMinSize = 19;
	}
}
