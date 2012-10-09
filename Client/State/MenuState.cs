namespace Client.State
{
    using View;
    using View.Menu;
    using View.Lobby;

    public class MenuState : GameState
    {
        public IGWOCTISI Game { get; protected set; }

        public MenuState(IGWOCTISI game) : base(game)
        {
            Game = game;
            var menuBackground = new MenuBackground(this);
            var mainMenu = new MainMenu(this);

            ViewMgr.PushLayer(menuBackground);
            ViewMgr.PushLayer(mainMenu);
        }
    }
}
