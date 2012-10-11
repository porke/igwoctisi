namespace Client.View.Lobby
{
    using System;

    class JoinGameArgs : EventArgs
    {
        public int LobbyId { get; private set; }

        public JoinGameArgs(int lobbyId)
        {
            LobbyId = lobbyId;
        }
    }
}
