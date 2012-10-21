namespace Client.Model
{
    using System.Collections.Generic;
    using Client.Renderer;
    using Microsoft.Xna.Framework;

    public class Scene
    {
        public Map Map { get; private set; }
        private List<Player> _players;

        public Scene(Map map, List<Player> playerList)
        {
            Map = map;
            _players = playerList;
        }
        public Planet PickPlanet(Vector2 clickPosition, IRenderer renderer)
        {
            foreach (var item in Map.Planets)
            {
                if (renderer.RaySphereIntersection(clickPosition, new Vector3(item.X, item.Y, item.Z), item.Radius))
                {
                    return item;
                }
            }

            return null;
        }
    }
}
