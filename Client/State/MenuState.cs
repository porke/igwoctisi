namespace Client.State
{
    using View;
    using View.Menu;

    public class MenuState : GameState
    {
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
