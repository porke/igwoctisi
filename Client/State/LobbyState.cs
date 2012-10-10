namespace Client.State
{
    using View;
    using View.Lobby;
    using System;
    using Common;
    using System.Collections.Generic;
    using Client.Model;

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

        public override void OnEnter()
        {
            var network = Client.Network;

            var messageBox = new MessageBox(MessageBoxButtons.None)
            {
                Title = "Entering Main Lobby",
                Message = "Downloading game list..."
            };

            ViewMgr.PushLayer(messageBox);
            network.BeginGetGameList(OnGetGameList, messageBox);
        }
        
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
            Client.Network.BeginGetGameList(OnReceiveGameList, (args as SenderEventArgs).Sender);
        }

        #endregion

        #region Network async callbacks

        private void OnLogout(IAsyncResult result)
        {
            Client.Network.EndDisconnect(result);
            Client.ChangeState(new MenuState(Game));
        }
                
        private void OnGetGameList(IAsyncResult ar)
        {
            var asyncResult = result as AsyncResult<List<GameInfo>>;
            var gameNames = asyncResult.Result as List<GameInfo>;

            var lobbyWindow = asyncResult.AsyncState as MainLobbyView;
            lobbyWindow.Invoke(lobbyWindow.RefreshGameList, gameNames);
                var games = network.EndGetGameList(ar);

                ViewMgr.PopLayer(); // MessageBox
            }
            catch (Exception exc)
            {
                messageBox.Message = exc.Message;
                messageBox.Buttons = MessageBoxButtons.OK;
                messageBox.OkPressed += (sender, e) => ViewMgr.PopLayer();
            }
        }

        #endregion
    }
}
