namespace Client.Model
{
    using System.Runtime.Serialization;

    [DataContract]
    public class LobbyListInfo
    {
        [DataMember]
        public int LobbyId { get; set; }

        [DataMember]
        public int PlayersCount { get; set; }

        [DataMember]
        public string Name { get; set; }
    }
}
