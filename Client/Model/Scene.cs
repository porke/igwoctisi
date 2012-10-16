namespace Client.Model
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;

    public class Scene
    {
        public Map Map { get; private set; }
        private List<string> _players;

        public Scene(Map map, List<string> playerList)
        {
            Map = map;
            _players = playerList;
        }
        public Planet PickPlanet(Vector2 clickPosition)
        {
            // TODO: Mocked picking
            Random rand = new Random();
            return Map.Planets[rand.Next(Map.Planets.Count)];
        }
    }
}
