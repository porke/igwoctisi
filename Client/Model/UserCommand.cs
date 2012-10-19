namespace Client.Model
{
    using System;
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
        public string PlayerUsername { get; set; }

        [DataMember]
        public int SourceId { get; set; }

        [DataMember]
        public int TargetId { get; set; }

        [DataMember]
        public int UnitCount { get; set; }

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
        /// <param name="player"></param>
        /// <param name="sourcePlanet"></param>
        /// <param name="targetPlanet"></param>
        public UserCommand(Player player, Planet sourcePlanet, Planet targetPlanet)
        {
            SourcePlanet = sourcePlanet;
            TargetPlanet = targetPlanet;

            PlayerUsername = player.Username;
            SourceId = sourcePlanet.Id;
            TargetId = targetPlanet.Id;
            UnitCount = 1;
            Type = CommandType.Move;
        }

        /// <summary>
        /// Creates Deploy Command.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="deployUnitCount">units to deploy</param>
        public UserCommand(Player player, Planet planet, int deployUnitCount)
        {
            TargetPlanet = planet;

            PlayerUsername = player.Username;
            TargetId = planet.Id;
            UnitCount = deployUnitCount;
            Type = CommandType.Deploy;
        }

        // TODO Create constructor for Tech Command

        public void SubtractUnit()
        {
            if (UnitCount == 0) return;

            // TODO: temp unit subtraction implementation
            --UnitCount;
        }   
    }
}
