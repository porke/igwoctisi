namespace Client.Model
{
    using System.Collections.Generic;
    using PlanetLink = System.Tuple<Planet, Planet>;

    public class GameState
    {
        List<Planet> Planets;
        List<PlanetLink> Links;
        List<PlanetarySystem> Systems;
        List<Planet> StartingPositions;

        public GameState(string mapName)
        {
            string path = string.Format("Content/Maps/{0}.xml", mapName);

        }
    }
}
