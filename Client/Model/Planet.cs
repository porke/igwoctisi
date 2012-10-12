namespace Client.Model
{
    using Microsoft.Xna.Framework;
    using System;
    using System.Xml.Serialization;

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
    }
}
