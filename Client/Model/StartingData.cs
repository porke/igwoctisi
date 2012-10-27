namespace Client.Model
{
    using System.Runtime.Serialization;
    
    [DataContract]
    public class StartingData
    {
        [DataMember]
        public int PlanetId { get; set; }

        [DataMember]
        public int Color { get; set; }

        public StartingData(int planetId, int color)
        {
            PlanetId = planetId;
            Color = color;
        }
    }
}
