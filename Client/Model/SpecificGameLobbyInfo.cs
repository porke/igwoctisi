namespace Client.Model
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class SpecificGameLobbyInfo
    {
        public SpecificGameLobbyInfo(string name, Player host)
        {
            Name = name;
            Players = new List<string>();
            Players.Add(host.Username);
        }

        [DataMember]
        public List<string> Players { get; set; }

        [DataMember]
        public string Name { get; set; }

        [IgnoreDataMember]
        public string HostName { get; private set; }

        public void RemovePlayer(string username)
        {
            Players.Remove(username);
        }

        public void AddPlayer(string username)
        {
            Players.Add(username);
        }
    }
}
