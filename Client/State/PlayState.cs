namespace Client.State
{
    using System;
    using System.Collections.Generic;
    using Model;
    using View.Play;

    class PlayState : GameState
    {
        public Scene Scene { get; protected set; }

        private GameViewport _gameViewport;
        private GameHud _gameHud;

        private List<UserCommand> _commands = new List<UserCommand>();
        private Player _clientPlayer;

        public PlayState(IGWOCTISI game, Map loadedMap, Player clientPlayer)
            : base(game)
        {
            Scene = new Scene(loadedMap, new List<Player> { _clientPlayer});
            _clientPlayer = clientPlayer;

            _gameViewport = new GameViewport(this);
            _gameHud = new GameHud(this);

            ViewMgr.PushLayer(_gameViewport);
            ViewMgr.PushLayer(_gameHud);

            eventHandlers.Add("LeaveGame", LeaveGame);
            eventHandlers.Add("SendOrders", SendOrders);
            eventHandlers.Add("SelectPlanet", SelectPlanet);
        }

        #region View event handlers

        private void LeaveGame(EventArgs args)
        {
            Client.ChangeState(new LobbyState(Game, _clientPlayer));
        }

        private void SendOrders(EventArgs args)
        {
            // TODO: Send orders
            _commands.Clear();
        }

        private void SelectPlanet(EventArgs args)
        {
            var selectedPlanetArg = args as SelectPlanetArgs;
            _gameHud.UpdateSelectedPlanet(selectedPlanetArg.Planet);
        }

        #endregion

        #region Async network callbacks

        #endregion
    }
}
