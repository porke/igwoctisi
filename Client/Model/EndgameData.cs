namespace Client.Model
{
	using System.Collections.Generic;
	using System.Runtime.Serialization;

	[DataContract]
	public class EndgameData
	{
		public EndgameData()
		{
			Stats = new List<GameStatistic>();
			Places = new List<string>();
		}

		[DataMember]
		public List<string> Places { get; set; }

		[DataMember]
		public int Rounds { get; set; }

		[DataMember]
		public int Time { get; set; }

		[DataMember]
		public List<GameStatistic> Stats { get; set; }

		[DataMember]
		public string EndType { get; set; }

		[DataMember]
		public int GameId { get; set; }
	}
}
