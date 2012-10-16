namespace Client.Model
{
    using System.Collections.Generic;
    using System;
    using System.Xml.Serialization;

    [Serializable]
    public class PlanetarySystem
    {        
        [XmlAttribute]
        public int FleetBonusPerTurn { get; set; }
        
        [XmlAttribute]
        public string Name { get; set; }

        [XmlArray("Planets")]
        [XmlArrayItem("PlanetId")]
        public int[] Planets { get; set; }
    }
}
