namespace Client.Model
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// Packet being received in RoundEnd message.
    /// </summary>
    [DataContract]
    public class SimulationResult
    {
        //{players:[], map:[{planetId, playerIndex, fleets}], tech:[], fleetsToDeploy, roundTime}
        [DataMember]
        public List<string> Players { get; set; }

        [DataMember]
        public List<FleetSizeChange> Map { get; set; }

        [DataMember]
        public List<int> Tech { get; set; }

        [DataMember]
        public int FleetsToDeploy { get; set; }

        [DataMember]
        public int RoundTime { get; set; }


        private SimulationResult() { }

        public class FleetSizeChange
        {
            public int PlanetId { get; set; }
            public int PlayerIndex { get; set; }
            public int Fleets { get; set; }
        }
    }
}
