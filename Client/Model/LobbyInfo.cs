namespace Client.Model
{
    /// <summary>
    /// It's info about Game Lobby, being sent before player intends to join to any lobby.
    /// </summary>
    public class LobbyInfo
    {
        public int LobbyId { get; set; }
        public int PlayersCount { get; set; }
        public string Name { get; set; }
    }
}
