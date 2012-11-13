namespace Client.View.Play
{
	using Client.Input;
	using Client.Model;
	using Client.State;
	using Client.Common;
	using Nuclex.UserInterface;
	using Nuclex.UserInterface.Controls;
	using Nuclex.UserInterface.Controls.Desktop;

	class GameStats : BaseView
	{
		private void CreateChildControls(EndgameData stats)
		{
			float width = 150 + 100 * stats.Players.Count;
			float height = 15 * (stats.Statistics.Count + stats.Players.Count);
			var statsWindow = new WindowControl()
			{
				Title = "Statistics",
				Bounds = new UniRectangle(new UniScalar(0.15f, 0), new UniScalar(0.15f, 0), new UniScalar(width), new UniScalar(height))
			};

			var standingLabel = new LabelControl("Places:")
			{
				Bounds = new UniRectangle(new UniScalar(0.025f, 0), new UniScalar(0.1f, 0), new UniScalar(0.02f, 0), new UniScalar(0.05f, 0))
			};
			var roundsLabel = new LabelControl(string.Format("Rounds: {0}", stats.Rounds))
			{
				Bounds = new UniRectangle(new UniScalar(0.5f, 0), new UniScalar(0.1f, 0), new UniScalar(0.02f, 0), new UniScalar(0.05f, 0))
			};
			var timeLabel = new LabelControl(string.Format("Time: {0}", stats.Time))
			{
				Bounds = new UniRectangle(new UniScalar(0.5f, 0), new UniScalar(0.15f, 0), new UniScalar(0.07f, 0), new UniScalar(0.05f, 0))
			};
			var endTypeLabel = new LabelControl(string.Format("End type: {0}", stats.EndType))
			{
				Bounds = new UniRectangle(new UniScalar(0.5f, 0), new UniScalar(0.2f, 0), new UniScalar(0.07f, 0), new UniScalar(0.05f, 0))
			};
			statsWindow.Children.AddRange(new Control[] { standingLabel, roundsLabel, timeLabel, endTypeLabel });

			// Create the player headings
			int columnCount = stats.Players.Count + 1;
			float statColumnWidth = (1.0f - 0.3f) / (float)columnCount;
			for (int p = 0; p < stats.Players.Count; ++p)
			{
				var item = stats.Players[p];
				// Create standings label
				var playerPlacesNameLabel = new LabelControl(item)
				{
					Bounds = new UniRectangle(new UniScalar(0.125f, 0), new UniScalar(0.1f + 0.05f * p, 0), new UniScalar(0.02f, 0), new UniScalar(0.05f, 0))
				};
				statsWindow.Children.Add(playerPlacesNameLabel);

				// Create first row of the stats table								
				var playerStatsNameLabel = new LabelControl(item)
				{
					Bounds = new UniRectangle(new UniScalar(0.3f + p * statColumnWidth, 0), new UniScalar(0.1f + 0.05f * (columnCount + 1), 0), new UniScalar(0.02f, 0), new UniScalar(0.05f, 0))
				};
				statsWindow.Children.Add(playerStatsNameLabel);
			}


			// Create the stats table
			var statHeaderLabel = new LabelControl("Statistic name")
			{
				Bounds = new UniRectangle(new UniScalar(0.05f, 0), new UniScalar(0.4f, 0), new UniScalar(0.02f, 0), new UniScalar(0.05f, 0))
			};
			statsWindow.Children.Add(statHeaderLabel);

			int statCount = stats.Statistics.Count;
			for (int s = 0; s < statCount; ++s)
			{
				var statistic = stats.Statistics[s];
				var statNameLabel = new LabelControl(statistic.StaticsticName)
				{
					Bounds = new UniRectangle(new UniScalar(0.05f, 0), new UniScalar(0.45f + 0.05f * s, 0), new UniScalar(0.02f, 0), new UniScalar(0.05f, 0))
				};
				statsWindow.Children.Add(statNameLabel);

				for (int i = 0; i < statistic.StatisticPlayers.Count; i++)
				{
					var statValueForPlayerLabel = new LabelControl(statistic.StatisticValues[i].ToString())
					{
						Bounds = new UniRectangle(new UniScalar(0.3f + i * statColumnWidth, 0), new UniScalar(0.45f + 0.05f * s, 0), new UniScalar(0.02f, 0), new UniScalar(0.05f, 0))
					};
					statsWindow.Children.Add(statValueForPlayerLabel);
				}
			}

			screen.Desktop.Children.Add(statsWindow);
		}

		#region Event handlers

		#endregion

		public GameStats(GameState state, EndgameData stats)
			: base(state)
		{
            IsTransparent = true;            
            InputReceiver = new NuclexScreenInputReceiver(screen, false);

			screen.Desktop.Bounds = new UniRectangle(new UniScalar(), new UniScalar(), new UniScalar(1.0f, 0.0f), new UniScalar(1.0f, 0));
            CreateChildControls(stats);
			State = ViewState.Loaded;
		}
	}
}
