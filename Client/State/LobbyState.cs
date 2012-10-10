namespace Client.State
{
    using View;
    using View.Lobby;
    using System;
    using Common;

    class LobbyState : GameState
    {
        public LobbyState(IGWOCTISI game) : base(game)
        {
            var menuBackground = new LobbyBackground(this);
            var lobbyMenu = new MainLobbyView(this);

            ViewMgr.PushLayer(menuBackground);
            ViewMgr.PushLayer(lobbyMenu);

            eventHandlers.Add("LeaveGameLobby", LeaveGameLobby);
            eventHandlers.Add("CreateGame", CreateGame);
            eventHandlers.Add("CancelCreateGame", CancelCreateGame);
            eventHandlers.Add("EnterCreateGameView", EnterCreateGameView);
            eventHandlers.Add("Logout", Logout);
            eventHandlers.Add("JoinGame", JoinGame);
            eventHandlers.Add("BeginGame", BeginGame);
            eventHandlers.Add("RefreshGameList", RefreshGameList);
        }

        #region Event handlers
        
        private void LeaveGameLobby(EventArgs args)
        {
            ViewMgr.PopLayer(); // pop game lobby
            ViewMgr.PushLayer(new MainLobbyView(this));
        }

        private void CreateGame(EventArgs args)
        {
            ViewMgr.PopLayer();     // pop main lobby window
            ViewMgr.PushLayer(new GameLobbyView(this));
        }

        private void CancelCreateGame(EventArgs args)
        {
            ViewMgr.PopLayer();     // pop create game view
            ViewMgr.PushLayer(new MainLobbyView(this));
        }

        private void EnterCreateGameView(EventArgs args)
        {
            ViewMgr.PopLayer();     // pop main lobby view
            ViewMgr.PushLayer(new CreateGameView(this));
        }

        private void Logout(EventArgs args)
        {
            Client.Network.BeginDisconnect(OnLogout, null);
        }

        private void JoinGame(EventArgs args)
        {
            //TODO: Implement join game
        }

        private void BeginGame(EventArgs args)
        {
            Game.ChangeState(new PlayState(Game));
        }

        private void RefreshGameList(EventArgs args)
        {
            Client.Network.BeginGetGameList(OnReceiveGameList, null);
        }

        #endregion

        #region Network async callbacks

        private void OnLogout(IAsyncResult result)
        {
            Client.Network.EndDisconnect(result);
            Client.ChangeState(new MenuState(Game));
        }

        private void OnReceiveGameList(IAsyncResult result)
        {
            var asyncResult = result as AsyncResult<object>;
            var gameNames = asyncResult.Result as string[];
            
            // TODO push refresh to views
            // Refresh must be called on the main thread or else it can break the ui
            // (weird exceptions resulting from concurrent collection modifications) 

            Client.Network.EndGetGameList(result);
        }

        #endregion
    }
}
