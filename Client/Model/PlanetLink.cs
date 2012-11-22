namespace Client.Model
{
    using System;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
	using Newtonsoft.Json;

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

		[JsonIgnore]
		[IgnoreDataMember]
		public PlanetLink OppositeLink { get; set; }

		[OnDeserialized]
		public void OnJsonDeserialized(StreamingContext context)
		{
			OnXmlDeserialized();
		}

		public void OnXmlDeserialized()
		{
			OppositeLink = new PlanetLink();
			OppositeLink.SourcePlanet = TargetPlanet;
			OppositeLink.TargetPlanet = SourcePlanet;
		}
    }
}
