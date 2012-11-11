namespace Client.Model
{
	public enum TechnologyType
	{
		None,
		Offensive,
		Defensive,
		Economic
	}

	public class Technology
	{
		public TechnologyType TechnologyType { get; private set; }		
		public int CurrentLevel { get; set; }
		public int NextLevelCost 
		{
			get
			{				
				return (IsMaxLevel) ? int.MaxValue : RaiseCosts[CurrentLevel + 1];
			}
		}
		public int CurrentLevelCost
		{
			get
			{
				return RaiseCosts[CurrentLevel];
			}
		}
		public double BonusMultiplier 
		{
			get
			{
				return BonusMultipliersPerLevel[CurrentLevel];
			}
		}
		public bool IsMaxLevel
		{
			get
			{
				return CurrentLevel == RaiseCosts.Length - 1;
			}
		}

		public Technology(TechnologyType type)
		{
			TechnologyType = type;
			CurrentLevel = 0;			
		}

		private readonly double[] BonusMultipliersPerLevel = new double[]{ 1.0, 1.05, 1.1, 1.15 };
		private readonly int[] RaiseCosts = new int[]{ 0, 50, 150, 400 };
	}
}
