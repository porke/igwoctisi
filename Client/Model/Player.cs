namespace Client.Model
{
    using System.Collections.Generic;

    public class Player
    {
        public string Username { get; private set; }

        private List<Planet> _ownedPlanets = new List<Planet>();

        public Player(string username)
        {
            Username = username;
        }
    }
}
