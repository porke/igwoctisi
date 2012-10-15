namespace Client.Model
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;

    public class Scene
    {
        private Map _map;
        private List<Player> _players;

        public Scene(Map map, List<Player> playerList)
        {
            _map = map;
            _players = playerList;
        }

        public Planet PickPlanet(Vector2 clickPosition)
        {
            // TODO: Mocked picking
            Random rand = new Random();
            return _map.Planets[rand.Next(_map.Planets.Count)];
        }
    }
}
