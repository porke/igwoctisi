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

        public override void OnEnter()
        {
            Client.Network.OnDisconnected += new Action<string>(OnDisconnected_EventHandler);
            //this.RefreshGameList(new SenderEventArgs(ViewMgr.PeekLayer()));
        }

        public override void OnExit()
        {
            Client.Network.OnDisconnected -= new Action<string>(OnDisconnected_EventHandler);
        }

        #region Event handlers

        private void LeaveGameLobby(EventArgs args)
        {
            Client.Network.BeginLeaveGame(OnLeaveGame, null);
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
            var ar = Client.Network.BeginLogout(OnLogout, null);
            ar.AsyncWaitHandle.WaitOne();
        }

        private void JoinGame(EventArgs args)
        {
            var joinGameArgs = args as JoinGameArgs;

            var messageBox = new MessageBox(MessageBoxButtons.None)
            {
                Title = "Join Game",
                Message = "Joining in..."
            };
            ViewMgr.PushLayer(messageBox);
            Client.Network.BeginJoinGameLobby(joinGameArgs.LobbyId, OnJoinLobby, messageBox);
        }

        private void BeginGame(EventArgs args)
        {
            Game.ChangeState(new PlayState(Game));
        }

        private void RefreshGameList(EventArgs args)
        {
            var messageBox = new MessageBox(MessageBoxButtons.None)
            {
                Title = "Loading Main Lobby",
                Message = "Downloading game list..."
            };            

            ViewMgr.PushLayer(messageBox);

            var mainLobbyView = (args as SenderEventArgs).Sender;
            Client.Network.BeginGetGameList(OnGetGameList, mainLobbyView);
        }

        #endregion

        #region Network async callbacks

        private void OnLogout(IAsyncResult result)
        {
            try
            {
                Client.Network.EndLogout(result);
            }
            catch { }
            finally
            {
                Client.Network.BeginDisconnect(OnDisconnect, null);
            }
        }
                
        private void OnGetGameList(IAsyncResult result)
        {            
            var gameNames = Client.Network.EndGetGameList(result);
            var lobbyWindow = result.AsyncState as MainLobbyView;
            lobbyWindow.Invoke(lobbyWindow.RefreshGameList, gameNames);

            ViewMgr.PopLayer(); // MessageBox            
        }

        private void OnJoinLobby(IAsyncResult result)
        {
            var messageBox = result.AsyncState as MessageBox;

            try
            {
                var gameInfo = Client.Network.EndJoinGameLobby(result);

                ViewMgr.PopLayer(); // MessageBox
                ViewMgr.PopLayer(); // MainLobbyView
                ViewMgr.PushLayer(new GameLobbyView(this));
            }
            catch (Exception exc)
            {
                messageBox.Buttons = MessageBoxButtons.OK;
                messageBox.Message = exc.Message;
                messageBox.OkPressed += (sender, e) => { ViewMgr.PopLayer(); };
            }
        }

        private void OnLeaveGame(IAsyncResult result)
        {
            try
            {
                Client.Network.EndLeaveGame(result);
            }
            catch { }
            finally
            {
                ViewMgr.PopLayer(); // pop game lobby
                ViewMgr.PushLayer(new MainLobbyView(this));
            }
        }

        private void OnDisconnect(IAsyncResult result)
        {
            try
            {
                Client.Network.EndDisconnect(result);
            }
            catch { }
            finally
            {
            }
        }

        private void OnDisconnected_EventHandler(string reason)
        {
            var menuState = new MenuState(Game);
            Client.ChangeState(menuState);
            menuState.HandleViewEvent("OnDisconnected", new MessageBoxArgs("Disconnection", "You were disconnected from the server."));
        }

        #endregion
    }
}
