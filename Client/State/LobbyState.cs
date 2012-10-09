namespace Client.State
{
    using View;
    using View.Lobby;
    using View.Menu;

    class LobbyState : GameState
    {
        public IGWOCTISI Game { get; protected set; }

        public LobbyState(IGWOCTISI game) : base(game)
        {
            Game = game;
            var menuBackground = new BackgroundPlaceholder(this);
            var lobbyMenu = new LobbyMenu(this);

            ViewMgr.PushLayer(menuBackground);
            ViewMgr.PushLayer(lobbyMenu);
        }
    }
}
