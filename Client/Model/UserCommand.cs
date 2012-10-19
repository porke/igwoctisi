using System.Runtime.Serialization;
using System;
namespace Client.Model
{
    [DataContract]
    public class UserCommand
    {
        [DataMember]
        public string PlayerUsername { get; set; }

        [DataMember]
        public int SourcePlanetId { get; set; }

        [DataMember]
        public int TargetPlanetId { get; set; }

        [DataMember]
        public int UnitCount { get; set; }

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
            SourcePlanetId = sourcePlanet.Id;
            TargetPlanetId = targetPlanet.Id;
            UnitCount = 1;
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
            TargetPlanetId = planet.Id;
            UnitCount = deployUnitCount;
        }

        public void SubtractUnit()
        {
            if (UnitCount == 0) return;

            // TODO: temp unit subtraction implementation
            --UnitCount;
        }

        public enum CommandType
        {
            Move,   //Attack inside
            Deploy,
            Tech
        }

        public CommandType Type
        {
            get
            {
                if (SourcePlanetId > 0 && TargetPlanetId > 0)
                {
                    return CommandType.Move;
                }
                else if (TargetPlanetId > 0)
                {
                    return CommandType.Deploy;
                }
                // TODO
                //else if ... 
                // return CommandType.Tech
                else
                    throw new NotImplementedException();
            }
        }
    }
}
