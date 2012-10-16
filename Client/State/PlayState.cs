namespace Client.State
{
    using System;
    using Model;
    using View.Play;

    class PlayState : GameState
    {
        public Scene Scene { get; protected set; }

        public PlayState(IGWOCTISI game)
            : base(game)
        {
            Scene = new Scene();
            Scene.Map = new Map("TestMap");

            var gameViewport = new GameViewport(this);
            var gameHud = new GameHud(this);

            ViewMgr.PushLayer(gameViewport);
            //ViewMgr.PushLayer(gameHud);

            eventHandlers.Add("LeaveGame", LeaveGame);
            eventHandlers.Add("SendOrders", SendOrders);
        }

        #region View event handlers

        private void LeaveGame(EventArgs args)
        {
            Client.ChangeState(new LobbyState(Game));
        }

        private void SendOrders(EventArgs args)
        {
            // TODO: Send orders
        }

        #endregion

        #region Async netowrk callbacks

        #endregion
    }
}
