
using System.Linq;
using System.Collections.Generic;
using Client.Renderer;
using Microsoft.Xna.Framework;

namespace Client.Model
{
    public class Scene
    {
        public Map Map { get; private set; }
		public int HoveredPlanet { get; set; }
		public int SelectedPlanet { get; set; }
		public PlanetLink HoveredLink { get; set; }

        private List<Player> _players;

        public Scene(Map map, List<Player> playerList)
        {
            Map = map;
			HoveredPlanet = SelectedPlanet = 0;
			HoveredLink = null;
            _players = playerList;

            // Assign planets
            for (int i = 0; i < _players.Count; i++)
            {
                var startingPlanet = Map.StartingPositions[i];
                startingPlanet.Owner = _players[i];
                _players[i].AddPlanet(startingPlanet);
            }
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
		public PlanetLink PickLink(Vector2 clickPosition, IRenderer renderer)
		{
			foreach (var link in Map.Links.Where(x => SelectedPlanet == x.SourcePlanet || SelectedPlanet == x.TargetPlanet))
			{
				var sourcePlanet = Map.GetPlanetById(link.SourcePlanet);
				var targetPlanet = Map.GetPlanetById(link.TargetPlanet);
				var sourcePos = new Vector3(sourcePlanet.X, sourcePlanet.Y, sourcePlanet.Z);
				var targetPos = new Vector3(targetPlanet.X, targetPlanet.Y, targetPlanet.Z);

				if (renderer.RayLinkIntersection(clickPosition, sourcePos, targetPos))
					return link;
			}
			return null;
		}
    }
}
