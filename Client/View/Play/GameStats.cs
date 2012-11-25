namespace Client.View.Play
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
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
		public event EventHandler LadderPressed;

		private void CreateChildControls(string clientUsername, EndgameData stats)
		{
			CreateGrid(clientUsername, stats);
			CreateTopSection(stats.Time, stats.Rounds, stats.Places.IndexOf(clientUsername) + 1);
			CreatePlayersSection(stats.Places, clientUsername);
			CreateBottomSection();
			CreateStatsSection(stats.Stats, stats.Places, clientUsername);
		}

		#region Grid construction

		private void CreateGrid(string clientName, EndgameData stats)
		{
			// One additional column for stat names
			_horzCells = stats.Places.Count + 1; 
			// 1 space, 2 rows for top and 2 rows for bottom
			_vertCells = stats.Stats.Count + 2 + 2 + 1;

			// If the window is too thin, add two additional rows for ladder labels
			_isCompact = _horzCells < 4;
			if (_isCompact)
			{
				_vertCells += 2;
			}

			float windowWidthInPx = _horzCells * ColumnWidthInPx + BorderWidthInPx;
			float windowHeightInPx = _vertCells * RowHeightInPx + TitleBarHeightInPx;
			float windowXPosition = (screen.Desktop.GetAbsoluteBounds().Width - windowWidthInPx) / screen.Desktop.GetAbsoluteBounds().Width / 2;
			float windowYPosition = (screen.Desktop.GetAbsoluteBounds().Height - windowHeightInPx) / screen.Desktop.GetAbsoluteBounds().Height / 2;	
			_gridHost = new WindowControl
			{
				Title = string.Format(WindowTitle, clientName.Equals(stats.Places[0]) ? "won" : "lost"),
				EnableDragging = false, 
				Bounds = new UniRectangle(new UniScalar(windowXPosition, 0), new UniScalar(windowYPosition, 0), new UniScalar(windowWidthInPx), new UniScalar(windowHeightInPx))
			};
			screen.Desktop.Children.Add(_gridHost);
		}

		private void CreateTopSection(int timeInSeconds, int rounds, int place)
		{
			var span = new TimeSpan(0, 0, 0, timeInSeconds);
			var hoursLbl = span.Hours > 0 ? Convert.ToString(span.Hours) + "h " : string.Empty;
			var minutesLbl = span.Hours > 0 || span.Minutes > 0 ? Convert.ToString(span.Minutes) + "m " : string.Empty;
			var secondsLbl = span.Seconds + "s";
			var roundsLabel = new LabelControl(string.Format(TopLabelTimeText, rounds, hoursLbl, minutesLbl, secondsLbl));
			var placeLabel = new LabelControl(string.Format(TopLabelPlaceText, place));
			InsertToCell(roundsLabel, 0, 0, _horzCells);
			SetPadding(roundsLabel, 4, 0, 0, 0);
			InsertToCell(placeLabel, 0, 1, _horzCells);
			SetPadding(placeLabel, 4, 0, 0, 4);
		}

		private void CreatePlayersSection(List<string> players, string clientUsername)
		{
			for (int plr = 0; plr < players.Count; ++plr)
			{
				var playerStatsNameLabel = new LabelControl(string.Format("{0}. {1}", plr + 1, players[plr]));
				InsertToCell(playerStatsNameLabel, plr + 1, 2);
				SetPadding(playerStatsNameLabel, ColumnWidthInPx / 2 - playerStatsNameLabel.Text.Length * 4 - StatCellPadding / 2, 0, 0, 0);
			}
		}

		private void CreateStatsSection(List<GameStatistic> stats, List<string> places, string clientName)
		{
			for (int stat = 0; stat < stats.Count; ++stat)
			{
				var statistic = stats[stat];
				var statName = new LabelControl(statistic.Name);
				InsertToCell(statName, 0, stat + 3);
				SetPadding(statName, 12, 0, 0, 0);

				for (int plr = 0; plr < places.Count; plr++)
				{
					// If this value is the maximum, highlight it or the current player is the client
					var frameName = places[plr].Equals(clientName) ? "stats_column_highlight" : "null_frame";
					var maxValue = statistic.Values.Max();
					if (maxValue == statistic.Values[plr])
					{
						frameName = "stats_cell_highlight";
					}

					var statValueForPlayerLabel = new LabelControl(statistic.Values[plr].ToString());
					InsertToHighlightedCell(statValueForPlayerLabel, frameName, plr + 1, stat + 3);
					SetPadding(statValueForPlayerLabel, ColumnWidthInPx / 2 - statistic.Values[plr].ToString().Length * 4 - StatCellPadding / 2, 0, 0, 0);
					SetPadding(statValueForPlayerLabel.Parent, StatCellPadding, 0, StatCellPadding, 0);
				}
			}
		}

		private void CreateBottomSection()
		{
			var bottomDesc = new LabelControl("You can view the scores on the ladder page.");			
			var linkDesc = new LabelControl("Press the Ladder button to open the browser.");
			if (_isCompact)
			{
				InsertToCell(bottomDesc, 0, _vertCells - 4, _horzCells - 2);
				InsertToCell(linkDesc, 0, _vertCells - 3, _horzCells - 2);
			}
			else
			{
				InsertToCell(bottomDesc, 0, _vertCells - 2, _horzCells - 2);
				InsertToCell(linkDesc, 0, _vertCells - 1, _horzCells - 2);
			}
			SetPadding(linkDesc, 4, 0, 0, 4);
			SetPadding(bottomDesc, 4, 0, 0, 0);

			var btnLeave = new ButtonControl
			{
				Text = "Leave",
			};
			InsertToCell(btnLeave, _horzCells - 1, _vertCells - 2, 1, 2);
			SetPadding(btnLeave, 8);
			btnLeave.Pressed += Leave_Pressed;

			var btnLadder = new ButtonControl
			{
				Text = "Ladder",
			};
			InsertToCell(btnLadder, _horzCells - 2, _vertCells - 2, 1, 2);
			SetPadding(btnLadder, 8);
			btnLadder.Pressed += Ladder_Pressed;
		}

		private void InsertToCell(Control content, int x, int y, int colspan = 1, int rowspan = 1)
		{
			content.Bounds = new UniRectangle(new UniScalar(x * ColumnWidthInPx + BorderWidthInPx), 
											  new UniScalar(y * RowHeightInPx + TitleBarHeightInPx), 
											  new UniScalar(ColumnWidthInPx * colspan), 
											  new UniScalar(RowHeightInPx * rowspan ));
			_gridHost.Children.Add(content);
		}

		private void InsertToHighlightedCell(Control content, string frameName, int x, int y, int colspan = 1, int rowspan = 1)
		{
			var icon = new IconControl(frameName);
			content.Bounds = new UniRectangle(new UniScalar(), new UniScalar(), new UniScalar(1.0f, 0), new UniScalar(1.0f, 0));
			icon.Children.Add(content);
			InsertToCell(icon, x, y, colspan, rowspan);
		}

		private void SetPadding(Control ctrl, int paddingL, int paddingT, int paddingR, int paddingB)
		{
			ctrl.Bounds.Left = new UniScalar(ctrl.Bounds.Left.Fraction, ctrl.Bounds.Left.Offset + paddingL);
			ctrl.Bounds.Right = new UniScalar(ctrl.Bounds.Right.Fraction, ctrl.Bounds.Right.Offset - 2 * paddingR);
			ctrl.Bounds.Top = new UniScalar(ctrl.Bounds.Top.Fraction, ctrl.Bounds.Top.Offset + paddingT);
			ctrl.Bounds.Bottom = new UniScalar(ctrl.Bounds.Bottom.Fraction, ctrl.Bounds.Bottom.Offset - 2 * paddingB);
		}

		private void SetPadding(Control ctrl, int padding)
		{
			SetPadding(ctrl, padding, padding, padding, padding);
		}

		#endregion

		#region Event handlers

		private void Leave_Pressed(object sender, EventArgs e)
		{
			if (LeavePressed != null)
			{
				LeavePressed(this, EventArgs.Empty);
			}
		}

		private void Ladder_Pressed(object sender, EventArgs e)
		{
			if (LadderPressed != null)
			{
				LadderPressed(this, EventArgs.Empty);
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

		private int _horzCells;
		private int _vertCells;
		private WindowControl _gridHost;
		private bool _isCompact;

		private const string WindowTitle = "You have {0} the game";
		private const string TopLabelTimeText = "You played for {0} rounds which lasted {1}{2}{3} in total.";
		private const string TopLabelPlaceText = "You finished the game on the {0} place.";

		private const int TitleBarHeightInPx = 24;
		private const int BorderWidthInPx = 4;
		private const int StatCellPadding = 8;
		private const int RowHeightInPx = 24;
		private const int ColumnWidthInPx = 128;
	}
}
