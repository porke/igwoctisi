namespace Client.State
{
    using View;
    using View.Lobby;

    class LobbyState : GameState
    {
        public IGWOCTISI Game { get; protected set; }

        public LobbyState(IGWOCTISI game) : base(game)
        {
            Game = game;
            var menuBackground = new LobbyBackground(this);
            var lobbyMenu = new MainLobbyView(this);

            ViewMgr.PushLayer(menuBackground);
            ViewMgr.PushLayer(lobbyMenu);
        }
    }
}
