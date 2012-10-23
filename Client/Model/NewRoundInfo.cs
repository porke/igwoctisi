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
        public List<string> Players { get; set; }
        public List<PlanetState> Map { get; set; }
        public List<int> Tech { get; set; }
        public int FleetsToDeploy { get; set; }

        /// <summary>
        /// Number of seconds.
        /// </summary>
        public int RoundTime { get; set; }

        public class PlanetState
        {
            public int PlanetId { get; set; }
            public int PlanetIndex { get; set; }
            public int Fleets { get; set; }
        }
    }
}
