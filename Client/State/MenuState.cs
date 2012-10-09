namespace Client.State
{
    using View;
    using View.Menu;

    public class MenuState : GameState
    {
        public IGWOCTISI Game { get; protected set; }

        public MenuState(IGWOCTISI game) : base(game)
        {
            Game = game;
            var intro1 = new SplashScreen(this, "Textures\\SplashScreen1", 1);
            var intro2 = new SplashScreen(this, "Textures\\SplashScreen2", 1);
            var menuBackground = new MenuBackground(this);
            var mainMenu = new MainMenu(this);

            intro1.NextLayers = new[] { intro2 };
            intro2.NextLayers = new IView[] { menuBackground, mainMenu };
            ViewMgr.PushLayer(intro1);
        }
    }
}
