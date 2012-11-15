namespace Client.Model
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Runtime.Serialization;
	using System.Xml;
	using System.Xml.Serialization;
	using Client.Common;
	using Client.Common.AnimationSystem;
	using Client.Renderer;
	using Microsoft.Xna.Framework;

    [DataContract]
    public class Map
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public List<Planet> Planets { get; set; }

        [DataMember]
        public List<PlanetLink> Links { get; set; }

        [DataMember]
        public List<PlanetarySystem> PlanetarySystems { get; set; }

        [DataMember]
        public List<StartingData> PlayerStartingData { get; set; }

        [DataMember]
        public List<PlayerColor> Colors { get; set; }

		[DataMember]
		public SimpleCamera Camera { get; set; }

        public MapVisual Visual { get; set; }

        public List<Planet> StartingPositions { get { return PlayerStartingData.Select(data => GetPlanetById(data.PlanetId)).ToList(); } }
        public int MaxPlayersCount { get { return StartingPositions.Count; } }

		// Map path template
        private const string MapsPath = "Content/Maps/{0}.xml";

        // Attributes names
        private const string NameAttribute = "Name";
        private const string PlanetIdAttribute = "PlanetId";
        private const string ColorIdAttribute = "ColorId";
        private const string ValueAttribute = "Value";
        private const string IdAttribute = "Id";
		private const string MinAttribute = "Min";
		private const string MaxAttribute = "Max";

        // Element names
        private const string MapElement = "Map";
        private const string PlanetsElement = "Planets";
        private const string LinksElement = "Links";
        private const string SystemsElement = "Systems";
        private const string PlayerStartingDataElement = "PlayerStartingData";
        private const string StartingDataElement = "StartingData";
        private const string ColorsElement = "Colors";
        private const string ColorElement = "Color";
		private const string CameraElement = "Camera";

        /// <summary>
        /// Reads localWorld map from XML file (no extension required). 
        /// </summary>
        public Map(string mapFileName) : this()
        {
            string path = string.Format(Map.MapsPath, mapFileName);
            using (FileStream fileStream = File.OpenRead(path))
            {
                var reader = XmlReader.Create(fileStream);
                
                // Read map name
                reader.ReadToFollowing(MapElement);
                this.Name = reader.GetAttribute(NameAttribute);

                // Read planets
                reader.ReadToDescendant(PlanetsElement);
                reader.ReadToDescendant(typeof(Planet).Name);
                var planetSerializer = new XmlSerializer(typeof(Planet));                
                do
                {
                    var planet = (Planet) planetSerializer.Deserialize(reader);

                    if (planet != null)
                    {
                        planet.NumFleetsPresent = 1;
                        Planets.Add(planet);
                    }
                } while (reader.ReadToNextSibling(typeof(Planet).Name));

                // Read links
                reader.ReadToFollowing(LinksElement);
                reader.ReadToDescendant(typeof(PlanetLink).Name);
                var linkSerializer = new XmlSerializer(typeof(PlanetLink));
                do
                {
                    var planetLink = (PlanetLink) linkSerializer.Deserialize(reader);

                    if (planetLink != null)
                    {
                        Links.Add(planetLink);
                    }
                } while (reader.ReadToNextSibling(typeof(PlanetLink).Name));

                // Read systems
                reader.ReadToFollowing(SystemsElement);
                reader.ReadToDescendant(typeof(PlanetarySystem).Name);
                var systemSerializer = new XmlSerializer(typeof(PlanetarySystem));                
                do
                {
                    var planetarySystem = (PlanetarySystem) systemSerializer.Deserialize(reader);

                    if (planetarySystem != null)
                    {
                        PlanetarySystems.Add(planetarySystem);
                    }
                } while (reader.ReadToNextSibling(typeof(PlanetarySystem).Name));

                // Read starting positions
                reader.ReadToFollowing(PlayerStartingDataElement);
                reader.ReadToDescendant(StartingDataElement);
                do
                {
                    if (reader.HasAttributes)
                    {
                        int planetId = Convert.ToInt32(reader.GetAttribute(PlanetIdAttribute));
                        int colorId = Convert.ToInt32(reader.GetAttribute(ColorIdAttribute));
                        PlayerStartingData.Add(new StartingData(planetId, colorId));
                    }
                } while (reader.ReadToNextSibling(StartingDataElement));

                // Read available colors
                reader.ReadToFollowing(ColorsElement);
                reader.ReadToDescendant(ColorElement);
                do
                {
                    if (reader.HasAttributes)
                    {
                        int colorId = Convert.ToInt32(reader.GetAttribute(IdAttribute));
                        string colorHex = reader.GetAttribute(ValueAttribute);
                        int value = Convert.ToInt32(colorHex, 16);
                        Colors.Add(new PlayerColor(colorId, value));
                    }
                } while (reader.ReadToNextSibling(ColorElement));

				// Camera
				reader.ReadToFollowing(CameraElement);
				Camera = new SimpleCamera();
				Camera.Min = XnaExtensions.ParseVector3(reader.GetAttribute(MinAttribute));
				Camera.Max = XnaExtensions.ParseVector3(reader.GetAttribute(MaxAttribute));
				Camera.SetPosition((Camera.Min + Camera.Max) / 2.0f);
            }

			PlanetarySystems[0].Color = Color.Blue;
			PlanetarySystems[1].Color = Color.Red;
        }

        [OnDeserialized]
        public void OnJsonDeserialized(StreamingContext context)
        {
            // TODO Make Player (& Planet) references to be the same objects?
        }

        public Map()
        {            
            Planets = new List<Planet>();
            Links = new List<PlanetLink>();
            PlanetarySystems = new List<PlanetarySystem>();
            PlayerStartingData = new List<StartingData>();
            Colors = new List<PlayerColor>();
        }
		public void Update(double delta, double time)
		{
			Camera.Update(delta, time);
		}
        public Planet GetPlanetById(int planetId)
        {
            return Planets.Find(planet => planet.Id == planetId);
        }
		public PlanetarySystem GetSystemByPlanetid(int planetId)
		{
			return PlanetarySystems.FirstOrDefault(x => x.Planets.Contains(planetId));
		}
        /// <summary>
        /// The function determines which planets should have details visible, based on their owner
        /// and proximity to the client players planets (that is only neighbouring ones).
        /// </summary>        
        public void UpdatePlanetShowDetails(Player clientPlayer)
        {
            var bidirectionalLinks = Links.SelectMany(linkElement =>
            {
                var swappedLink = new PlanetLink
                {
                    SourcePlanet = linkElement.TargetPlanet,
                    TargetPlanet = linkElement.SourcePlanet
                };
                return new List<PlanetLink> { linkElement, swappedLink };
            });

            foreach (var planet in Planets)
            {
                planet.ShowDetails = false;

                // Check if the client player is the owner
                if (planet.Owner == clientPlayer)
                {
                    planet.ShowDetails = true;
                }
                // Find any neighbouring planets also owned by the client player
                else
                {
                    // Select the links and duplicate them with swapped target and source
                    // because the graph edges are bidirectional
                    var neighbourIds = bidirectionalLinks.Where(link => link.SourcePlanet == planet.Id);                    
                    var neighbours = neighbourIds.Select(link => GetPlanetById(link.TargetPlanet));
                    foreach (var neighbour in neighbours)
                    {
                        if (neighbour.Owner == clientPlayer)
                        {
                            planet.ShowDetails = true;
                            break;
                        }
                    }
                }
            }
        }
        public PlayerColor GetColorById(int colorId)
        {
            return Colors.First(c => c.ColorId == colorId);
        }
    }
}
