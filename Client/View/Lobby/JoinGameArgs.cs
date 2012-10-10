namespace Client.View.Lobby
{
    using System;

    class JoinGameArgs : EventArgs
    {
        public string GameId { get; private set; }

        public JoinGameArgs(string gameId)
        {
            GameId = gameId;
        }
    }
}
