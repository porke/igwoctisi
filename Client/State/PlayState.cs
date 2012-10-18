namespace Client.State
{
    using System;
    using System.Collections.Generic;
    using Model;
    using View.Play;
    using Client.View;

    class PlayState : GameState
    {
        public Scene Scene { get; protected set; }

        private GameViewport _gameViewport;
        private GameHud _gameHud;

        private List<UserCommand> _commands = new List<UserCommand>();
        private Player _clientPlayer;
        private double _secondsLeft = 0;

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
            eventHandlers.Add("OnHoverPlanet", OnHoverPlanet);
            eventHandlers.Add("UnhoverPlanets", UnhoverPlanets);

            Client.Network.OnRoundStarted += new Action<SimulationResult>(Network_OnRoundStarted);
            Client.Network.OnRoundEnded += new Action(Network_OnRoundEnded);
            Client.Network.OnGameEnded += new Action(Network_OnGameEnded);
            Client.Network.OnOtherPlayerLeft += new Action<string, DateTime>(Network_OnOtherPlayerLeft);
            Client.Network.OnDisconnected += new Action<string>(Network_OnDisconnected);
        }

        public override void OnUpdate(double delta, double time)
        {
            base.OnUpdate(delta, time);
            
            // Update timer
            if (_secondsLeft > 0)
            {
                if (_secondsLeft - delta <= 0)
                {
                    _secondsLeft = 0;

                    // Create message box that will be shown until server's roundEnd or gameEnd message arrives.
                    var messageBox = new MessageBox(MessageBoxButtons.OK)
                    {
                        Title = "Round simulating",
                        Message = "Waiting for server to simulate the turn."
                            + Environment.NewLine + Environment.NewLine
                            + "(This OK button will disappear)"
                    };
                    messageBox.OkPressed += (sender, e) => { ViewMgr.PopLayer(); };//TODO to be removed (no OK button!!)
                    ViewMgr.PushLayer(messageBox);
                }
                else
                {
                    _secondsLeft -= delta;
                }
            }

            _gameHud.UpdateTimer((int)_secondsLeft);
        }

        #region View event handlers

        private void LeaveGame(EventArgs args)
        {
            var messageBox = new MessageBox(MessageBoxButtons.None)
            {
                Message = "Leaving game..."
            };
            ViewMgr.PushLayer(messageBox);

            Client.Network.BeginLeaveGame(OnLeaveGame, messageBox);
        }

        private void SendOrders(EventArgs args)
        {
            // TODO: Send orders
            //Client.Network.BeginSendCommands

            _commands.Clear();
        }

        private void SelectPlanet(EventArgs args)
        {
            var selectedPlanet = (args as SelectPlanetArgs).Planet;
            _gameHud.UpdateSelectedPlanet(selectedPlanet);
            selectedPlanet.SelectionState = PlanetSelection.Selected;
            
            foreach (var deselectedPlanet in Scene.Map.Planets)
            {
                if (deselectedPlanet != selectedPlanet)
                {
                    deselectedPlanet.SelectionState = PlanetSelection.NotSelected;
                }
            }
        }

        private void OnHoverPlanet(EventArgs args)
        {
            var hoverPlanet = (args as SelectPlanetArgs).Planet;
            if (hoverPlanet.SelectionState != PlanetSelection.Selected)
            {
                hoverPlanet.SelectionState = PlanetSelection.Hover;
            }

            foreach (var deselectedPlanet in Scene.Map.Planets)
            {
                if (deselectedPlanet != hoverPlanet 
                    && deselectedPlanet.SelectionState != PlanetSelection.Selected)
                {
                    deselectedPlanet.SelectionState = PlanetSelection.NotSelected;
                }
            }
        }

        private void UnhoverPlanets(EventArgs args)
        {
            foreach (var deselectedPlanet in Scene.Map.Planets)
            {
                if (deselectedPlanet.SelectionState != PlanetSelection.Selected)
                {
                    deselectedPlanet.SelectionState = PlanetSelection.NotSelected;
                    deselectedPlanet.Visual.Period = 2.0f;
                }
            }
        }

        #endregion

        #region Async network callbacks

        void OnLeaveGame(IAsyncResult result)
        {
            InvokeOnMainThread(obj =>
            {
                var messageBox = result.AsyncState as MessageBox;

                try
                {
                    Client.Network.EndLeaveGame(result);
                    ViewMgr.PopLayer(); // MessageBox
                    Client.ChangeState(new LobbyState(Game, _clientPlayer));
                }
                catch (Exception exc)
                {
                    messageBox.Buttons = MessageBoxButtons.OK;
                    messageBox.Message = exc.Message;
                    messageBox.OkPressed += (sender, e) =>
                    {
                        ViewMgr.PopLayer();
                        Client.ChangeState(new LobbyState(Game, _clientPlayer));
                    };
                }
            });
        }

        void Network_OnRoundStarted(SimulationResult simRes)
        {
            _secondsLeft = simRes.RoundTime;
            _gameHud.UpdateTimer((int)_secondsLeft);

            // TODO update gui to enable it for making new moves
        }

        void Network_OnRoundEnded(/*moves here!*/)
        {
            // TODO collect info about moves and animate them
            throw new NotImplementedException();
        }

        void Network_OnGameEnded(/*game result here!*/)
        {
            // TODO show game result and statistics
            throw new NotImplementedException();
        }

        void Network_OnOtherPlayerLeft(string username, DateTime time)
        {
            throw new NotImplementedException();
        }

        void Network_OnDisconnected(string reason)
        {
            InvokeOnMainThread(obj =>
            {
                var menuState = new MenuState(Game);
                Client.ChangeState(menuState);
                menuState.HandleViewEvent("OnDisconnected", new MessageBoxArgs("Disconnection", "You were disconnected from the server."));
            });
        }

        #endregion
    }
}
