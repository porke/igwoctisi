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
		public TopPanel()
		{
			IconName = FrameName;
			Bounds = new UniRectangle(new UniScalar(), new UniScalar(), new UniScalar(1.0f, 0.0f), new UniScalar(0.0f, 40));

			CreateIcons();
			CreateButtons();
			CreateLabels();
		}

		private void CreateIcons()
		{
			var fleetIcon = new IconControl
			{
				IconName = "fleetIcon",
				Bounds = new UniRectangle(new UniScalar(8), new UniScalar(8), new UniScalar(24), new UniScalar(24))
			};

			var techIcon = new IconControl
			{
				IconName = "techIcon",
				Bounds = new UniRectangle(new UniScalar(128), new UniScalar(8), new UniScalar(24), new UniScalar(24))
			};

			var timerIcon = new IconControl
			{
				IconName = "timerIcon",
				Bounds = new UniRectangle(new UniScalar(1.0f, -128), new UniScalar(8), new UniScalar(24), new UniScalar(24))
			};

			Children.AddRange(
				new Control[] 
				{
					fleetIcon, 
					techIcon, 
					timerIcon
				});
		}

		private void CreateLabels()
		{
			_timer = new LabelControl
			{
				Bounds = new UniRectangle(new UniScalar(1.0f, -96), new UniScalar(16), new UniScalar(), new UniScalar())
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
				Bounds = new UniRectangle(new UniScalar(194), new UniScalar(4), new UniScalar(48), new UniScalar(32))
			};

			_defensiveTech = new ButtonControl
			{
				Name = TechnologyType.Defensive.ToString(),
				Bounds = new UniRectangle(new UniScalar(248), new UniScalar(4), new UniScalar(48), new UniScalar(32))
			};

			_economicTech = new ButtonControl
			{
				Name = TechnologyType.Economic.ToString(),
				Bounds = new UniRectangle(new UniScalar(302), new UniScalar(4), new UniScalar(48), new UniScalar(32))
			};

			Children.AddRange(
				new Control[]
				{
					_offensiveTech,
					_defensiveTech,
					_economicTech
				});
		}

		#region Update functions

		public void UpdateResources(Player player)
		{
			_fleetIncomeAndCountValue.Text = string.Format("{0}/{1}", player.FleetIncomePerTurn, player.DeployableFleets);
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

		private LabelControl _fleetIncomeAndCountValue;
		private LabelControl _timer;
		private LabelControl _techPointsValue;
		private ButtonControl _offensiveTech;
		private ButtonControl _defensiveTech;
		private ButtonControl _economicTech;

		private const string FrameName = "topPanel";
	}
}
