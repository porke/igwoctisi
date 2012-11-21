namespace Client.View.Play
{
	using System;
	using Client.Common;
	using Client.Input;
	using Client.Model;
	using Client.State;
	using Client.View.Controls;
	using Nuclex.UserInterface;
	using Nuclex.UserInterface.Controls;
	using Nuclex.UserInterface.Controls.Desktop;

	class GameStats : BaseView
	{
		public event EventHandler LeavePressed;

		private void CreateChildControls(EndgameData stats)
		{
			int windowGridColumnCount = stats.Places.Count + 1;
			int windowGridRowCount = ((stats.Places.Count >= MinPlayerRowCount) ? stats.Places.Count : MinPlayerRowCount) + stats.Stats.Count + 1;

			float windowWidthInPx = windowGridColumnCount * ColumnWidthInPx;
			float windowHeightInPx = windowGridRowCount * RowHeightInPx + TitleBarHeightInPx;
			float windowXPosition = (screen.Desktop.GetAbsoluteBounds().Width - windowWidthInPx) / screen.Desktop.GetAbsoluteBounds().Width / 2;
			float windowYPosition = (screen.Desktop.GetAbsoluteBounds().Height - windowHeightInPx) / screen.Desktop.GetAbsoluteBounds().Height / 2;			
			float rowHeight = (1.0f - TitleBarHeightInPx / windowHeightInPx) / (float)windowGridRowCount;
			float columnWidth = 1.0f / (float)windowGridColumnCount;
			var statsWindow = new WindowControl()
			{
				EnableDragging = false,
				Title = WindowTitle,
				Bounds = new UniRectangle(new UniScalar(windowXPosition, 0), new UniScalar(windowYPosition, 0), new UniScalar(windowWidthInPx), new UniScalar(windowHeightInPx))
			};

			var standingLabel = new LabelControl(PlacesLabelText)
			{
				Bounds = new UniRectangle(new UniScalar(0.0f, BorderWidthInPx), new UniScalar(0.0f, TitleBarHeightInPx), new UniScalar(0.02f, 0), new UniScalar(0.05f, 0))
			};
			var roundsLabel = new LabelControl(string.Format(RoundsLabelText, stats.Rounds))
			{
				Bounds = new UniRectangle(new UniScalar(0.5f, BorderWidthInPx), new UniScalar(0.0f, TitleBarHeightInPx), new UniScalar(0.02f, 0), new UniScalar(0.05f, 0))
			};
			var timeLabel = new LabelControl(string.Format(TimeLabelText, stats.Time))
			{
				Bounds = new UniRectangle(new UniScalar(0.5f, BorderWidthInPx), new UniScalar(rowHeight, TitleBarHeightInPx), new UniScalar(0.07f, 0), new UniScalar(0.05f, 0))
			};
			var endTypeLabel = new LabelControl(string.Format(EndgameTypeLabelText, stats.EndType))
			{				
				Bounds = new UniRectangle(new UniScalar(0.5f, BorderWidthInPx), new UniScalar(2 * rowHeight, TitleBarHeightInPx), new UniScalar(0.07f, 0), new UniScalar(0.05f, 0))
			};
			statsWindow.Children.AddRange(new Control[] { standingLabel, roundsLabel, timeLabel, endTypeLabel });

			// Create the player headings						
			int firstStatsRow = (stats.Places.Count >= MinPlayerRowCount) ? stats.Places.Count : MinPlayerRowCount;
			for (int plr = 0; plr < stats.Places.Count; ++plr)
			{
				var item = stats.Places[plr];
				// Create standings label
				var playerPlacesNameLabel = new LabelControl(string.Format("{0}. {1}", plr + 1, item))
				{
					Bounds = new UniRectangle(new UniScalar(0.1f, BorderWidthInPx), new UniScalar(rowHeight * plr, TitleBarHeightInPx), new UniScalar(0.02f, 0), new UniScalar(0.05f, 0))
				};
				statsWindow.Children.Add(playerPlacesNameLabel);

				// Create first row of the stats table								
				var playerStatsNameLabel = new LabelControl(item)
				{
					Bounds = new UniRectangle(new UniScalar((plr + 1) * columnWidth, BorderWidthInPx), new UniScalar(firstStatsRow * rowHeight, TitleBarHeightInPx), new UniScalar(0.02f, 0), new UniScalar(0.05f, 0))
				};
				statsWindow.Children.Add(playerStatsNameLabel);
			}
			
			// Create the stats table
			var statHeaderLabel = new LabelControl(StatisticNameHeader)
			{
				Bounds = new UniRectangle(new UniScalar(0.0f, BorderWidthInPx + columnWidth / 2), new UniScalar(firstStatsRow * rowHeight, TitleBarHeightInPx), new UniScalar(0.02f, 0), new UniScalar(0.05f, 0))
			};
			statsWindow.Children.Add(statHeaderLabel);

			for (int stat = 0; stat < stats.Stats.Count; ++stat)
			{
				var statistic = stats.Stats[stat];
				var statNameLabel = new LabelControl(statistic.Name)
				{
					Bounds = new UniRectangle(new UniScalar(0, BorderWidthInPx), new UniScalar(rowHeight * (firstStatsRow + stat + 1), TitleBarHeightInPx), new UniScalar(0.02f, 0), new UniScalar(0.05f, 0))
				};
				statsWindow.Children.Add(statNameLabel);

				for (int plr = 0; plr < statistic.Values.Count; plr++)
				{
					var statValueForPlayerLabel = new LabelControl(statistic.Values[plr].ToString())
					{
						Bounds = new UniRectangle(new UniScalar((plr + 1) * columnWidth, BorderWidthInPx), new UniScalar(rowHeight * (firstStatsRow + stat + 1), TitleBarHeightInPx), new UniScalar(0.02f, 0), new UniScalar(0.05f, 0))
					};
					statsWindow.Children.Add(statValueForPlayerLabel);
				}
			}

			var btnLeave = new ImageButtonControl
			{
				StateFrames = new string[] { "closeButtonNormal", "closeButtonNormal", "closeButtonHover", "closeButtonPushed" },
				Bounds = new UniRectangle(new UniScalar(0.95f, 0), new UniScalar(0.04f, 0), new UniScalar(0.05f, 0), new UniScalar(0.03f, 0))
			};
			btnLeave.Pressed += Leave_Pressed;
			statsWindow.Children.Add(btnLeave);

			screen.Desktop.Children.Add(statsWindow);
		}

		#region Event handlers

		private void Leave_Pressed(object sender, EventArgs e)
		{
			if (LeavePressed != null)
			{
				LeavePressed(this, EventArgs.Empty);
			}
		}

		#endregion

		public GameStats(GameState state, EndgameData stats)
			: base(state)
		{
            IsTransparent = false;            
            InputReceiver = new NuclexScreenInputReceiver(screen, true);

			screen.Desktop.Bounds = new UniRectangle(new UniScalar(), new UniScalar(), new UniScalar(1.0f, 0.0f), new UniScalar(1.0f, 0));
            CreateChildControls(stats);
			State = ViewState.Loaded;
		}

		private const string StatisticNameHeader = "Statistic name";
		private const string WindowTitle = "Statistics";
		private const string PlacesLabelText = "Places";
		private const string RoundsLabelText = "Rounds: {0}";
		private const string TimeLabelText = "Time: {0}";
		private const string EndgameTypeLabelText = "Endgame type: {0}";

		private const int TitleBarHeightInPx = 28;
		private const int BorderWidthInPx = 8;
		private const int RowHeightInPx = 24;
		private const int ColumnWidthInPx = 128;
		private const int MinPlayerRowCount = 3;
	}
}
