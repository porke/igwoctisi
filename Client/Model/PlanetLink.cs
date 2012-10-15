namespace Client.Model
{
    using System;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    [Serializable] //XML
    [DataContract] //Json
    public class PlanetLink
    {
        [XmlAttribute("SourceId")]
        [DataMember] //Json
        public int SourcePlanet { get; set; }

        [XmlAttribute("TargetId")]
        [DataMember] //Json
        public int TargetPlanet { get; set; }
    }
}
