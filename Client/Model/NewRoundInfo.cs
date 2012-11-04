namespace Client.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
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
        public List<int> Tech { get; set; }

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
            public int PlayerIndex { get; set; }

            [DataMember]
            public int Fleets { get; set; }
        }

        /// <summary>
        /// Gets owner of the planet.
        /// </summary>
        /// <param name="planetId">id of planet</param>
        /// <param name="out_username">returned username of player owning the planet</param>
        /// <returns>true if out_username param was set, otherwise false</returns>
        public bool TryFindPlanetOwner(int planetId, ref string out_username)
        {
            int playerIndex = Map.First(ps => ps.PlanetId == planetId).PlayerIndex;
            if (playerIndex >= 0)
            {
                out_username = Players[playerIndex];
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets planet state.
        /// </summary>
        /// <param name="planetId"></param>
        /// <returns>planet state (fleet count)</returns>
        public PlanetState FindPlanetState(int planetId)
        {
            return Map.First(p => p.PlanetId.Equals(planetId));
        }
    }
}
