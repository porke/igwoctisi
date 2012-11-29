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

        [DataMember]
        public int SourceId { get; set; }

        [DataMember]
        public int TargetId { get; set; }

        [DataMember]
        public int FleetCount { get; set; }

        [DataMember]
        public CommandType Type { get; set; }

		[DataMember]
        public TechnologyType TechType { get; set; }

		private Player _issuer;

        public Planet SourcePlanet { get; private set; }
        public Planet TargetPlanet { get; private set; }

        /// <summary>
        /// Constructor needed by Json deserialization.
        /// </summary>
        public UserCommand()
        {

        }

        /// <summary>
        /// Creates Move Command or Offensive Command if Source and Target owner are not equal.
        /// </summary>
        public UserCommand(Planet sourcePlanet, Planet targetPlanet)
        {
            SourcePlanet = sourcePlanet;
            TargetPlanet = targetPlanet;

            SourceId = sourcePlanet.Id;
            TargetId = targetPlanet.Id;
            FleetCount = 1;
            Type = (sourcePlanet.Owner == targetPlanet.Owner) ? CommandType.Move : CommandType.Attack;
			_issuer = sourcePlanet.Owner;
        }

        /// <summary>
        /// Creates Deploy Command.
        /// </summary>
        public UserCommand(Planet planet, int deployUnitCount)
        {
            TargetPlanet = planet;
			_issuer = planet.Owner;

            TargetId = planet.Id;
            FleetCount = deployUnitCount;
            Type = CommandType.Deploy;
        }

        /// <summary>
        /// Creates Tech Command.
        /// </summary>
        public UserCommand(TechnologyType improvedTech, Player issuer)
        {
            Type = CommandType.Tech;
            TechType = improvedTech;
			_issuer = issuer;
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
			else if (Type == CommandType.Tech)
			{
				_issuer.TechPoints += _issuer.Technologies[TechType].CurrentLevelCost;
				_issuer.Technologies[TechType].CurrentLevel--;
			}
        }
    }
}
