namespace Client.Model
{
    using System.Runtime.Serialization;

    [DataContract]
    public class UserCommand
    {
        public enum CommandType
        {
            Move,   //Attack inside
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

        public Planet SourcePlanet { get; private set; }
        public Planet TargetPlanet { get; private set; }

        /// <summary>
        /// Constructor needed by Json deserialization.
        /// </summary>
        public UserCommand()
        {
        }

        /// <summary>
        /// Creates Move Command.
        /// </summary>
        public UserCommand(Planet sourcePlanet, Planet targetPlanet)
        {
            SourcePlanet = sourcePlanet;
            TargetPlanet = targetPlanet;

            SourceId = sourcePlanet.Id;
            TargetId = targetPlanet.Id;
            FleetCount = 1;
            Type = CommandType.Move;
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

        // TODO Create constructor for Tech Command
    }
}
