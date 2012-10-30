namespace Client.Model
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.Xna.Framework;
    using System.ComponentModel;

    [DataContract]
    public class Player
    {
        /// <summary>
        /// ARGB hex.
        /// </summary>
        public enum Color
        {
            Red = 0x00FC0300,
            Green = 0x0000FF00,
            Blue = 0x000000FF,
            Cyan = 0x0000FFFF,
            Yellow = 0x00FFEF00,
            Lime = 0x0093F600,
            White = 0x00FFFFFF,
            Orange = 0x00FE7F00
        }


        [DataMember]
        public string Username { get; private set; }

        public Color Color { get; private set; }
        public int DeployableFleets { get; set; }
        public bool CanDeployFleets { get { return DeployableFleets > 0; } }
        public int FleetIncomePerTurn
        {
            get
            {
                int fleets = 0;
                foreach (var planet in _ownedPlanets)
                {
                    fleets += planet.BaseUnitsPerTurn;
                }

                return fleets;
            }
        }

        private List<Planet> _ownedPlanets = new List<Planet>();
        public List<UserCommand> Commands { get; private set; }

        public Player(string username)
        {
            Username = username;
            Color = Color.White;
            Commands = new List<UserCommand>();
        }

        #region Command operations

        public void ClearCommandList()
        {
            Commands.Clear();
        }
        public void DeployFleet(Planet planet)
        {
            var command = Commands.Find(cmd => cmd.Type == UserCommand.CommandType.Deploy && cmd.TargetId == planet.Id);
            if (command == null)
            {
                command = new UserCommand(planet, 1);
                Commands.Add(command);
            }
            else
            {
                command.FleetCount++;
            }

            planet.NumFleetsPresent++;
            DeployableFleets--;
        }
        public void UndeployFleet(Planet planet)
        {
            var command = Commands.Find(cmd => cmd.Type == UserCommand.CommandType.Deploy && cmd.TargetId == planet.Id);
            command.FleetCount--;
            planet.NumFleetsPresent--;
            DeployableFleets++;

            if (command.FleetCount == 0)
            {
                Commands.Remove(command);
            }
        }
        public void MoveFleet(Planet source, Planet target)
        {
            var command = Commands.Find(cmd => cmd.SourceId == source.Id && cmd.TargetId == target.Id);
            if (command == null)
            {
                command = new UserCommand(source, target);
                command.FleetCount = 1;
                Commands.Add(command);
            }
            else
            {
                command.FleetCount++;
            }

            source.NumFleetsPresent--;
            target.NumFleetsPresent++;
        }
        public void RevertFleetMove(Planet source, Planet target)
        {
            var targetCommand = Commands.Find(cmd => cmd.SourceId == source.Id && cmd.TargetId == target.Id);
            targetCommand.FleetCount--;
            source.NumFleetsPresent++;
            target.NumFleetsPresent--;

            if (targetCommand.FleetCount == 0)
            {
                Commands.Remove(targetCommand);
            }
        }
        public void DeleteCommand(int commandIndex)
        {
            var deletedCommand = Commands[commandIndex];

            // Remove dependant commands
            // ex. a move dependant on an earlier deploy
            if (!deletedCommand.CanRevert())
            {
                var dependantCommands = Commands.FindAll(cmd => cmd.Type == UserCommand.CommandType.Move && cmd.SourceId == deletedCommand.TargetId);

                foreach (var item in dependantCommands)
                {
                    item.Revert();
                    Commands.Remove(item);
                }
            }

            deletedCommand.Revert();
            Commands.RemoveAt(commandIndex);
        }

        #endregion

        public void AddPlanet(Planet planet)
        {
            _ownedPlanets.Add(planet);
        }

        public void EndRound()
        {
            // TODO: mock implementation
            DeployableFleets += FleetIncomePerTurn;
        }
    }
}
