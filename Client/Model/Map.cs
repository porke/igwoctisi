namespace Client.Model
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;

    public class Map
    {
        private List<Planet> Planets = new List<Planet>();
        private List<PlanetLink> Links = new List<PlanetLink>();
        private List<PlanetarySystem> PlanetarySystems = new List<PlanetarySystem>();
        private List<Planet> StartingPositions = new List<Planet>();

        private const string MapsPath = "Content/Maps/{0}.xml";

        public Map(string mapName)
        {
            string path = string.Format(Map.MapsPath, mapName);
            using (FileStream fileStream = File.OpenRead(path))
            {
                var reader = XmlReader.Create(fileStream);

                // Read planets
                reader.ReadToDescendant("Planets");
                reader.ReadToDescendant(typeof(Planet).Name);
                var planetSerializer = new XmlSerializer(typeof(Planet));                
                do
                {
                    var planet = (Planet) planetSerializer.Deserialize(reader);
                    Planets.Add(planet);
                } while (reader.ReadToNextSibling(typeof(Planet).Name));

                // Read links
                reader.ReadToFollowing("Links");
                reader.ReadToDescendant(typeof(PlanetLink).Name);
                var linkSerializer = new XmlSerializer(typeof(PlanetLink));
                do
                {
                    var planetLink = (PlanetLink) linkSerializer.Deserialize(reader);
                    Links.Add(planetLink);
                } while (reader.ReadToNextSibling(typeof(PlanetLink).Name));

                // Read systems
                reader.ReadToFollowing("Systems");
                reader.ReadToDescendant(typeof(PlanetarySystem).Name);
                var systemSerializer = new XmlSerializer(typeof(PlanetarySystem));                
                do
                {
                    var planetarySystem = (PlanetarySystem) systemSerializer.Deserialize(reader);
                    PlanetarySystems.Add(planetarySystem);
                } while (reader.ReadToNextSibling(typeof(PlanetarySystem).Name));

                // Read starting positions
                reader.ReadToFollowing("PlayerStartingData");
                reader.ReadToDescendant("StartingData");
                do
                {
                    var planetId = Convert.ToInt32(reader.GetAttribute("PlanetId"));
                    var foundPlanet = Planets.Find((planet) => planet.Id == planetId);                    
                    StartingPositions.Add(foundPlanet);
                } while (reader.ReadToNextSibling("StaringData"));
            }
        }
    }
}
