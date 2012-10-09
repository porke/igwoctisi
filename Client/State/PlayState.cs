namespace Client.State
{
    using View;
    using View.Play;
    using Model;

    class PlayState : GameState
    {
        public IGWOCTISI Game { get; protected set; }
        public Scene Scene { get; protected set; }

        public PlayState(IGWOCTISI game)
            : base(game)
        {
            Game = game;
            Scene = new Scene();

            var gameViewport = new GameViewport(this);
            var gameHud = new GameHud(this);

            ViewMgr.PushLayer(gameViewport);
            ViewMgr.PushLayer(gameHud);
        }
    }
}
