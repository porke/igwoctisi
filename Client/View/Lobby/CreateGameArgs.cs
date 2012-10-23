namespace Client.View.Lobby
{
    using System;

    class CreateGameArgs : EventArgs
    {
        public string GameName { get; set; }
        public string MapName { get; set; }

        public CreateGameArgs(string gameName, string mapName)
        {
            GameName = gameName;
            MapName = mapName;
        }
    }
}
