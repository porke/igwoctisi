namespace Client.Model
{
    using System.Collections.Generic;

    public class Player
    {
        public string Username { get; private set; }
        public int DeployableFleets { get; set; }
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

        public Player(string username)
        {
            Username = username;
        }

        public void AddPlanet(Planet planet)
        {
            _ownedPlanets.Add(planet);
        }

        public bool CanDeployFleets()
        {
            return DeployableFleets > 0;
        }

        public void EndRound()
        {
            // TODO: mock implementation
            DeployableFleets += FleetIncomePerTurn;
        }
    }
}
