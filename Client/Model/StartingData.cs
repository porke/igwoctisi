namespace Client.Model
{
    using System.Runtime.Serialization;
    
    [DataContract]
    public class StartingData
    {
        [DataMember]
        public int PlanetId { get; set; }

        [DataMember]
        public int ColorId { get; set; }

        public StartingData(int planetId, int colorId)
        {
            PlanetId = planetId;
            ColorId = colorId;
        }
    }
}
