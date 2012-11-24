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

		private void CreateChildControls(string clientName, EndgameData stats)
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
				Title = string.Format(WindowTitle, clientName.Equals(stats.Places[0]) ? "won" : "lost"),
				Bounds = new UniRectangle(new UniScalar(windowXPosition, 0), new UniScalar(windowYPosition, 0), new UniScalar(windowWidthInPx), new UniScalar(windowHeightInPx))
			};

			var span = new TimeSpan(0, 0, 0, stats.Time);
			var hoursLbl = span.Hours > 0 ? Convert.ToString(span.Hours) + "h " : string.Empty;
			var minutesLbl = span.Hours > 0 || span.Minutes > 0 ? Convert.ToString(span.Minutes) + "m " : string.Empty;
			var secondsLbl = span.Seconds + "s";
			var roundsLabel = new LabelControl(string.Format(TopLabelText, stats.Rounds, hoursLbl, minutesLbl, secondsLbl, 12345656))
			{
				Bounds = new UniRectangle(new UniScalar(0.0f, BorderWidthInPx), new UniScalar(0.0f, TitleBarHeightInPx), new UniScalar(1.0f, 0), new UniScalar(0.4f, 0))
			};			
			statsWindow.Children.AddRange(new Control[] {roundsLabel });

			// Create player headings
			int firstStatsRow = (stats.Places.Count >= MinPlayerRowCount) ? stats.Places.Count : MinPlayerRowCount;			
			for (int plr = 0; plr < stats.Places.Count; ++plr)
			{
				var item = stats.Places[plr];

				// Create first row of the stats table								
				var playerStatsNameLabel = new LabelControl(string.Format("{0}. {1}", plr + 1, item))
				{
					Bounds = new UniRectangle(new UniScalar((plr + 1) * columnWidth, BorderWidthInPx), new UniScalar(firstStatsRow * rowHeight, TitleBarHeightInPx), new UniScalar(0.02f, 0), new UniScalar(0.05f, 0))
				};
				statsWindow.Children.Add(playerStatsNameLabel);
			}

			// Create the stats table
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

			var btnLeave = new ButtonControl
			{
				Text = "Leave",
				Bounds = new UniRectangle(new UniScalar(0.95f, 0), new UniScalar(0.02f, 0), new UniScalar(0.05f, 0), new UniScalar(0.1f, 0))
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

		public GameStats(GameState state, EndgameData stats, string clientUsername)
			: base(state)
		{
            IsTransparent = true;
            InputReceiver = new NuclexScreenInputReceiver(screen, true);

			screen.Desktop.Bounds = new UniRectangle(new UniScalar(), new UniScalar(), new UniScalar(1.0f, 0.0f), new UniScalar(1.0f, 0));
			CreateChildControls(clientUsername, stats);
			State = ViewState.Loaded;
		}

		private const string WindowTitle = "You have {0} the game";
		private const string TopLabelText = "You played for {0} rounds which lasted {1}{2}{3} in total.\n" +
											"You finished the game on the {4} place.";

		private const int TitleBarHeightInPx = 0;
		private const int BorderWidthInPx = 8;
		private const int RowHeightInPx = 24;
		private const int ColumnWidthInPx = 128;
		private const int MinPlayerRowCount = 3;
	}
}
