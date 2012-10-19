namespace Client.Model
{
    using System;
    using System.Xml.Serialization;
    using Client.Renderer;

    [Serializable]
    public class Planet
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public int BaseUnitsPerTurn { get; set; }

        [XmlAttribute]
        public int Id { get; set; }

        [XmlAttribute]
        public float X { get; set; }

        [XmlAttribute]
        public float Y { get; set; }

        [XmlAttribute]
        public float Z { get; set; }

        [XmlAttribute]
        public float Radius { get; set; }

        [XmlAttribute]
        public string Diffuse { get; set; }

        [XmlAttribute]
        public string Clouds { get; set; }

        [XmlAttribute]
        public string CloudsAlpha { get; set; }

        public PlanetVisual Visual { get; set; }
    }
}
