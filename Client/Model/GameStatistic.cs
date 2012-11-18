namespace Client.Model
{
	using System.Runtime.Serialization;
	using System.Collections.Generic;

	[DataContract]
	class GameStatistic
	{
		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public List<int> Values { get; set; }
	}
}
