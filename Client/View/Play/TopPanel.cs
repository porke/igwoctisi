namespace Client.View.Play
{
	using System;
	using Client.Common;
	using Client.Model;
	using Client.View.Controls;
	using Nuclex.UserInterface;
	using Nuclex.UserInterface.Controls;
	using Nuclex.UserInterface.Controls.Desktop;

	public class TopPanel : IconControl
	{
		public event EventHandler TechRaised;
		public event EventHandler LeftGame;

		public TopPanel()
		{
			IconFrameName = FrameName;
			Bounds = new UniRectangle(new UniScalar(), new UniScalar(), new UniScalar(1.0f, 0.0f), new UniScalar(0.0f, 40));

			CreateIcons();
			CreateButtons();
			CreateLabels();
		}

		#region Update functions

		public void UpdateResources(Player player)
		{
			_fleetIncomeAndCountValue.Text = string.Format("{0}/{1}", player.FleetIncomePerTurn - player.DeployableFleets, player.FleetIncomePerTurn);
			_techPointsValue.Text = Convert.ToString(player.TechPoints);
			_offensiveTech.Text = string.Format("Att: {0}", player.Technologies[TechnologyType.Offensive].CurrentLevel);
			_defensiveTech.Text = string.Format("Def: {0}", player.Technologies[TechnologyType.Defensive].CurrentLevel);
			_economicTech.Text = string.Format("Eco: {0}", player.Technologies[TechnologyType.Economic].CurrentLevel);
		}

		public void UpdateTimer(int secondsLeft)
		{
			int mins = secondsLeft / 60;
			int secs = secondsLeft % 60;

			// Display Timer in format 0:00
			_timer.Text = mins.ToString() + (secs < 10 ? ":0" : ":") + secs.ToString();
		}

		#endregion

		#region Event handlers

		private void RaiseTech_Pressed(object sender, EventArgs e)
		{
			if (TechRaised != null)
			{
				TechRaised(sender, e);
			}
		}

		private void LeaveGame_Pressed(object sender, EventArgs e)
		{
			if (LeftGame != null)
			{
				LeftGame(sender, e);
			}
		}

		#endregion

		private void CreateLabels()
		{
			_timer = new LabelControl
			{
				Bounds = new UniRectangle(new UniScalar(1.0f, -64), new UniScalar(16), new UniScalar(), new UniScalar())
			};

			_fleetIncomeAndCountValue = new LabelControl
			{
				Bounds = new UniRectangle(new UniScalar(40), new UniScalar(16), new UniScalar(), new UniScalar())
			};

			_techPointsValue = new LabelControl
			{
				Bounds = new UniRectangle(new UniScalar(160), new UniScalar(16), new UniScalar(), new UniScalar())
			};

			Children.AddRange(
				new Control[] 
				{
					_timer, 
					_techPointsValue, 
					_fleetIncomeAndCountValue
				});
		}

		private void CreateButtons()
		{
			_offensiveTech = new ButtonControl
			{
				Name = TechnologyType.Offensive.ToString(),
				Bounds = new UniRectangle(new UniScalar(224), new UniScalar(4), new UniScalar(48), new UniScalar(32))
			};
			_offensiveTech.Pressed += RaiseTech_Pressed;

			_defensiveTech = new ButtonControl
			{
				Name = TechnologyType.Defensive.ToString(),
				Bounds = new UniRectangle(new UniScalar(280), new UniScalar(4), new UniScalar(48), new UniScalar(32))
			};
			_defensiveTech.Pressed += RaiseTech_Pressed;

			_economicTech = new ButtonControl
			{
				Name = TechnologyType.Economic.ToString(),
				Bounds = new UniRectangle(new UniScalar(334), new UniScalar(4), new UniScalar(48), new UniScalar(32))
			};
			_economicTech.Pressed += RaiseTech_Pressed;

			var leaveGame = new ButtonControl
			{
				Text = "Leave",
				Bounds = new UniRectangle(new UniScalar(1.0f, -192), new UniScalar(4), new UniScalar(64), new UniScalar(32))
			};
			leaveGame.Pressed += LeaveGame_Pressed;

			Children.AddRange(
				new Control[]
				{
					_offensiveTech,
					_defensiveTech,
					_economicTech,
					leaveGame
				});
		}

		private void CreateIcons()
		{
			var fleetIcon = new IconControl
			{
				IconFrameName = "fleetIcon",
				Bounds = new UniRectangle(new UniScalar(8), new UniScalar(8), new UniScalar(24), new UniScalar(24))
			};

			var techIcon = new IconControl
			{
				IconFrameName = "techIcon",
				Bounds = new UniRectangle(new UniScalar(128), new UniScalar(8), new UniScalar(24), new UniScalar(24))
			};

			var timerIcon = new IconControl
			{
				IconFrameName = "timerIcon",
				Bounds = new UniRectangle(new UniScalar(1.0f, -96), new UniScalar(8), new UniScalar(24), new UniScalar(24))
			};

			Children.AddRange(
				new Control[] 
				{
					fleetIcon, 
					techIcon, 
					timerIcon
				});
		}

		private LabelControl _fleetIncomeAndCountValue;
		private LabelControl _timer;
		private LabelControl _techPointsValue;
		private ButtonControl _offensiveTech;
		private ButtonControl _defensiveTech;
		private ButtonControl _economicTech;

		private const string FrameName = "topPanel";
	}
}
