namespace Client.Model
{
	using System.Runtime.Serialization;
	using System.Collections.Generic;

	[DataContract]
	class GameStatistic
	{
		[DataMember]
		public string StaticsticName { get; set; }

		[DataMember]
		public List<int> StatisticValues { get; set; }
	}
}
