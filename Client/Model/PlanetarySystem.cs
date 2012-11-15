namespace Client.Model
{
	using System;
	using System.Runtime.Serialization;
	using System.Xml.Serialization;
	using Client.Renderer;
	using Microsoft.Xna.Framework;

    [Serializable] //XML
    [DataContract] //Json
    public class PlanetarySystem
    {
        [XmlAttribute]
        [DataMember]
        public int Id { get; set; }

        [XmlAttribute]
        [DataMember]
        public int FleetBonusPerTurn { get; set; }
        
        [XmlAttribute]
        [DataMember]
        public string Name { get; set; }

        [XmlArray("Planets")]
        [XmlArrayItem("PlanetId")]
        [DataMember]
        public int[] Planets { get; set; }
		
		[XmlArray("Bounds")]
		[DataMember]
		public Point3[] Bounds { get; set; }

		[XmlIgnore]
		public PlanetarySystemVisual Visual { get; set; }
    }
}
