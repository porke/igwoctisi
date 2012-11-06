namespace Client.Model
{
    using System.Runtime.Serialization;

    [DataContract]
    public class UserCommand
    {
        public enum CommandType
        {
            Move = 0,
            Attack,
            Deploy,
            Tech
        }

        public enum TechType
        {
            None,
            Offense,
            Defense,
            Economy
        }

        [DataMember]
        public int SourceId { get; set; }

        [DataMember]
        public int TargetId { get; set; }

        [DataMember]
        public int FleetCount { get; set; }

        [DataMember]
        public CommandType Type { get; set; }

        public TechType TechImproved { get; set; }

        public Planet SourcePlanet { get; private set; }
        public Planet TargetPlanet { get; private set; }

        /// <summary>
        /// Constructor needed by Json deserialization.
        /// </summary>
        public UserCommand()
        {
        }

        /// <summary>
        /// Creates Move Command or Attack Command if Source and Target owner are not equal.
        /// </summary>
        public UserCommand(Planet sourcePlanet, Planet targetPlanet)
        {
            SourcePlanet = sourcePlanet;
            TargetPlanet = targetPlanet;

            SourceId = sourcePlanet.Id;
            TargetId = targetPlanet.Id;
            FleetCount = 1;
            Type = (sourcePlanet.Owner == targetPlanet.Owner) ? CommandType.Move : CommandType.Attack;
        }

        /// <summary>
        /// Creates Deploy Command.
        /// </summary>
        public UserCommand(Planet planet, int deployUnitCount)
        {
            TargetPlanet = planet;

            TargetId = planet.Id;
            FleetCount = deployUnitCount;
            Type = CommandType.Deploy;
        }

        /// <summary>
        /// Creates Tech Command.
        /// </summary>
        public UserCommand(TechType improvedTech)
        {
            Type = CommandType.Tech;
            TechImproved = improvedTech;
        }

        public bool CanRevert()
        {
            if (Type == CommandType.Deploy)
            {
                return TargetPlanet.NumFleetsPresent - FleetCount > 0;
            }

            return true;
        }

        public void Revert()
        {
            if (Type == CommandType.Deploy)
            {
                TargetPlanet.NumFleetsPresent -= FleetCount;
                TargetPlanet.Owner.DeployableFleets += FleetCount;
                FleetCount = 0;
            }
            else if (Type == CommandType.Move || Type == CommandType.Attack)
            {
                SourcePlanet.FleetChange += FleetCount;
				TargetPlanet.FleetChange -= FleetCount;
            }
        }
    }
}
