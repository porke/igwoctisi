namespace Client.Model
{
    using System;
    using System.Xml.Serialization;

    [Serializable]
    public class PlanetLink
    {
        [XmlAttribute("SourceId")]
        public int SourcePlanet { get; set; }

        [XmlAttribute("TargetId")]
        public int TargetPlanet { get; set; }
    }
}
