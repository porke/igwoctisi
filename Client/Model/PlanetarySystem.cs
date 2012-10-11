namespace Client.Model
{
    using System.Collections.Generic;

    class PlanetarySystem
    {
        public List<Planet> Planets { get; set; }
        public int FleetBonusPerTurn { get; set; }
        public string Name { get; set; }
    }
}
