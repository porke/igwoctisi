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
			var statsWindow = new WindowControl()
			{
				Title = "Statistics",
				Bounds = new UniRectangle(new UniScalar(0.15f, 0), new UniScalar(0.7f, 0), new UniScalar(0.15f), new UniScalar(0.7f, 0))
			};

			var standingLabel = new LabelControl("Places:")
			{
				Bounds = new UniRectangle(new UniScalar(0.01f, 0), new UniScalar(0.2f, 0), new UniScalar(0.02f, 0), new UniScalar(0.05f, 0))
			};
			var roundsLabel = new LabelControl(string.Format("Rounds: {0}", stats.Rounds))
			{
				Bounds = new UniRectangle(new UniScalar(0.5f, 0), new UniScalar(0.2f, 0), new UniScalar(0.02f, 0), new UniScalar(0.05f, 0))
			};
			var timeLabel = new LabelControl(string.Format("Time: {0}", stats.Time))
			{
				Bounds = new UniRectangle(new UniScalar(0.5f, 0), new UniScalar(0.2f, 0), new UniScalar(0.07f, 0), new UniScalar(0.05f, 0))
			};
			statsWindow.Children.AddRange(new Control[] {standingLabel, roundsLabel, timeLabel});

			// Create the player headings
			foreach (var item in stats.Players)
			{
				// Create standings label
				var playerPlacesNameLabel = new LabelControl(item)
				{
					Bounds = new UniRectangle()
				};
				statsWindow.Children.Add(playerPlacesNameLabel);

				// Create first row of the stats table
				var playerStatsNameLabel = new LabelControl(item)
				{
					Bounds = new UniRectangle()
				};
				statsWindow.Children.Add(playerPlacesNameLabel);
			}

			// Create the stats table
			foreach (var statistic in stats.Statistics)
			{
				var statNameLabel = new LabelControl()
				{
					Bounds = new UniRectangle()
				};
				statsWindow.Children.Add(statNameLabel);

				for (int i = 0; i < statistic.StatisticPlayers.Count; i++)
				{
					var statValueForPlayerLabel = new LabelControl(statistic.StatisticValues[i].ToString())
					{
						Bounds = new UniRectangle()
					};
					statsWindow.Children.Add(statValueForPlayerLabel);
				}
			}

			screen.Desktop.Children.Add(statsWindow);
		}

		#region Event handlers

		#endregion

		public GameStats(PlayState state, EndgameData stats)
			: base(state)
		{
            IsTransparent = true;            
            InputReceiver = new NuclexScreenInputReceiver(screen, false);            
			
            CreateChildControls(stats);
			State = ViewState.Loaded;
		}
	}
}
