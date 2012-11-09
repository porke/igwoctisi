namespace Client.Model
{
	public enum TechType
	{
		Attack,
		Defense,
		Economy
	}

	public class Technology
	{
		public TechType TechnologyType { get; private set; }
		public double BonusMultiplier { get; private set; }
		public int CurrentLevel { get; private set; }
		public int NextLevelCost { get; private set; }

		public Technology(TechType type)
		{
			TechnologyType = type;
			CurrentLevel = 0;
			BonusMultiplier = FirstLevelMultiplier;
			NextLevelCost = FirstLevelRaiseCost;
		}

		private const double FirstLevelMultiplier = 1.0;
		private const int FirstLevelRaiseCost = 100;
	}
}
