namespace Client.Model
{
    using System.Collections.Generic;
    using PlanetLink = System.Tuple<Planet, Planet>;

    public class GameState
    {
        List<Planet> Planets;
        List<PlanetLink> Links;
        List<PlanetarySystem> Systems;
    }
}
