namespace Client.Model
{
    using System;
    using System.Xml.Serialization;
    using Client.Renderer;
    using Newtonsoft.Json;
    using System.Runtime.Serialization;

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
        
        
        public int NumFleetsPresent { get; set; }
        public PlanetVisual Visual { get; set; }

        [XmlIgnore, JsonIgnore]
        public Player Owner { get; set; }
    }
}
