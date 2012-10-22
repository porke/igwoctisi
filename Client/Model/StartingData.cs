namespace Client.Model
{
    using System.Runtime.Serialization;
    
    [DataContract]
    public class StartingData
    {
        [DataMember]
        public int PlanetId { get; set; }

        public StartingData(int planetId)
        {
            PlanetId = planetId;
        }
    }
}
