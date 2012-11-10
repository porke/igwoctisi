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
        public int FleetIncomePerTurn {get; set;}

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
        public void DeployFleet(Planet planet, int count)
        {
            var command = Commands.Find(cmd => cmd.Type == UserCommand.CommandType.Deploy && cmd.TargetId == planet.Id);
            if (command == null)
            {
                command = new UserCommand(planet, count);
                Commands.Add(command);
            }
            else
            {
                command.FleetCount += count;
            }

            planet.NumFleetsPresent += count;
            DeployableFleets -= count;
        }
        public void UndeployFleet(Planet planet, int count)
        {
            var command = Commands.Find(cmd => cmd.Type == UserCommand.CommandType.Deploy && cmd.TargetId == planet.Id);
			if (command.FleetCount < count)
			{
				count = command.FleetCount;
			}
			
			command.FleetCount -= count;
            planet.NumFleetsPresent -= count;
            DeployableFleets += count;

            if (command.FleetCount == 0)
            {
                Commands.Remove(command);
            }
        }
        public void MoveFleet(Planet source, Planet target, int count)
        {
			// Check if movement between these two planets exists, if so update it, otherwise, create a new command
            var command = Commands.Find(cmd => cmd.SourceId == source.Id && cmd.TargetId == target.Id);
            if (command == null)
            {
                command = new UserCommand(source, target);
				command.FleetCount = count;
                Commands.Add(command);
            }
            else
            {
				command.FleetCount += count;
            }

			source.FleetChange -= count;
			target.FleetChange += count;
        }
        public void RevertFleetMove(Planet source, Planet target, int count)
        {
            var targetCommand = Commands.Find(cmd => cmd.SourceId == source.Id && cmd.TargetId == target.Id);
			if (targetCommand.FleetCount < count)
			{
				count = targetCommand.FleetCount;
			}
			
			targetCommand.FleetCount -= count;
            source.FleetChange += count;
			target.FleetChange -= count;

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
