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
            public int PlanetIndex { get; set; }

            [DataMember]
            public int Fleets { get; set; }
        }
    }
}
