namespace Client.Model
{
    using System.Collections.Generic;

    public class SpecificGameLobbyInfo
    {        
        public List<Player> Players { get; private set; }
        public string GameName { get; set; }

        public void RemovePlayer(string username)
        {
            var index = Players.FindIndex(player => player.Username == username);

            if (index > -1)
            {
                Players.RemoveAt(index);
            }
        }

        public void AddPlayer(string username)
        {
            Players.Add(new Player(username));
        }
    }
}
