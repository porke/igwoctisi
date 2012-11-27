namespace Client.Model
{
	using System.Collections.Generic;
    using System.Linq;
	using System.Runtime.Serialization;

	/// <summary>
    /// Data being received from server in RoundStart packet.
    /// </summary>
    [DataContract]
    public class NewRoundInfo
    {
        [DataMember]
        public List<string> Players { get; set; }

        [DataMember]
        public List<PlanetState> Map { get; set; }

        [DataMember]
        public TechState Tech { get; set; }

        [DataMember]
        public int FleetsToDeploy { get; set; }

        /// <summary>
        /// Number of seconds.
        /// </summary>
        [DataMember]
        public int RoundTime { get; set; }

        [DataContract]
        public class PlanetState
        {
            [DataMember]
            public int PlanetId { get; set; }

            [DataMember]
            public string Player { get; set; }

            [DataMember]
            public int Fleets { get; set; }
        }

		[DataContract]
		public class TechState
		{
			[DataMember]
			public int Economic { get; set; }

			[DataMember]
			public int Offensive { get; set; }

			[DataMember]
			public int Defensive { get; set; }

			[DataMember]
			public int Points { get; set; }
		}

        /// <summary>
        /// Gets owner of the planet.
        /// </summary>
        /// <param name="planetId">id of planet</param>
        /// <param name="out_username">returned username of player owning the planet</param>
        /// <returns>true if out_username param was set, otherwise false</returns>
        public bool TryFindPlanetOwner(int planetId, ref string out_username)
        {
            var ownerName = Map.First(ps => ps.PlanetId == planetId).Player;
            if (!string.IsNullOrEmpty(ownerName))
            {
				out_username = ownerName;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets planet state.
        /// </summary>
        /// <returns>planet state (fleet count)</returns>
        public PlanetState FindPlanetState(int planetId)
        {
            return Map.First(p => p.PlanetId.Equals(planetId));
        }
    }
}
