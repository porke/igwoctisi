namespace Client.Model
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.Xna.Framework;
    
    [DataContract]
    public class Player
    {
        #region Public Fields

        [DataMember]
        public string Username { get; private set; }

		public List<Planet> OwnedPlanets
		{
			set
			{
				_ownedPlanets.Clear();
				_ownedPlanets.AddRange(value);
			}
		}
		public int TechPoints { get; set; }
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
        public List<UserCommand> Commands { get; private set; }
        public PlayerColor Color { get; set; }

        #endregion

        #region Private Fields

        private readonly List<Planet> _ownedPlanets = new List<Planet>();

        #endregion

        public Player(string username, PlayerColor color)
        {
            Username = username;
            Color = color;
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
                var dependantCommands = Commands.FindAll(cmd => cmd.Type != UserCommand.CommandType.Deploy && cmd.SourceId == deletedCommand.TargetId);

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

        /// <summary>
        /// Add planet to the player. Planet's owner is changed to this player.
        /// </summary>
        /// <returns>true if player didn't own the planet before</returns>
        public bool TryAssignPlanet(Planet planet)
        {
            // If player doesn't own the planet then assign it to him.
            if (!_ownedPlanets.Exists(p => p.Id == planet.Id))
            {
                _ownedPlanets.Add(planet);
                planet.Owner = this;
                return true;
            }

            return false;
        }    
    }
}
