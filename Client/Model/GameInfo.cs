namespace Client.Model
{
    using System.Collections.Generic;

    /// <summary>
    /// It's info being sent to player who's joining a Game Lobby.
    /// </summary>
    public class GameInfo
    {
        public List<string> Players { get; set; }
        public string Name { get; set; }
    }
}
