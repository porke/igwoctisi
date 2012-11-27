namespace Client.Model
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using System.Xml.Serialization;
	using Client.Renderer;
	using Microsoft.Xna.Framework;
	using Newtonsoft.Json;

    [Serializable]
    [DataContract]
    public class Planet
    {
        [XmlAttribute]
        [DataMember]
        public string Name { get; set; }

        [XmlAttribute]
        [DataMember]
        public int BaseUnitsPerTurn { get; set; }

        [XmlAttribute]
        [DataMember]
        public int Id { get; set; }

        [XmlAttribute]
        [DataMember]
        public float X { get; set; }

        [XmlAttribute]
        [DataMember]
        public float Y { get; set; }

        [XmlAttribute]
        [DataMember]
        public float Z { get; set; }

        [XmlAttribute]
        [DataMember]
        public float Radius { get; set; }

        [XmlAttribute]
        [DataMember]
        public string Diffuse { get; set; }

        [XmlAttribute]
        [DataMember]
        public string Clouds { get; set; }

        [XmlAttribute]
        [DataMember]
        public string CloudsAlpha { get; set; }

		public int FleetChange { get; set; }
        public int NumFleetsPresent { get; set; }

		[XmlIgnore]
        public PlanetVisual Visual { get; set; }
        public Vector3 Position { get { return new Vector3(X, Y, Z); } }

        [XmlIgnore, JsonIgnore]
        public Player Owner { get; set; }

		[XmlIgnore, JsonIgnore]
		public List<Planet> NeighbourPlanets { get; private set; }

		public Planet()
		{
			NeighbourPlanets = new List<Planet>();
		}
		internal void SetNeighbours(List<Planet> neighbourPlanets)
		{
			NeighbourPlanets = neighbourPlanets;
		}
	}
}
