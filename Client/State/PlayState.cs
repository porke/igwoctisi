namespace Client.State
{
	using System;
	using System.Collections.Generic;
	using Client.View;
	using Model;
	using View.Play;

	class PlayState : GameState
	{
		public Scene Scene { get; protected set; }

		private GameViewport _gameViewport;
		private GameHud _gameHud;

		private Player _clientPlayer;
		private List<Player> _players;
		private double _secondsLeft = 0;
		private HudState _hudState = HudState.Initializing;
		private object _hudStateLocker = new object();

		private enum HudState
		{
			Initializing,
			WaitingForRoundStart,
			WaitingForRoundEnd,
			AnimatingSimulationResult
		}

		public PlayState(IGWOCTISI game, Map loadedMap, Player clientPlayer, List<Player> players)
			: base(game)
		{
			_clientPlayer = clientPlayer;
			_players = players;

			Scene = new Scene(loadedMap, _players);
			_gameViewport = new GameViewport(this);
			_gameHud = new GameHud(this);
            _gameHud.UpdateClientPlayerFleetData(_clientPlayer);
			_gameHud.UpdatePlayerList(_players);

			ViewMgr.PushLayer(_gameViewport);
			ViewMgr.PushLayer(_gameHud);

			Client.Network.OnRoundStarted += Network_OnRoundStarted;
			Client.Network.OnRoundEnded += Network_OnRoundEnded;
			Client.Network.OnGameEnded += Network_OnGameEnded;
			Client.Network.OnOtherPlayerLeft += Network_OnOtherPlayerLeft;
			Client.Network.OnDisconnected += Network_OnDisconnected;
            Client.Network.OnChatMessageReceived += Network_OnChatMessageReceived;

			InvokeOnMainThread((obj) => { _hudState = HudState.WaitingForRoundStart; });
		}

        public override void OnExit()
        {
            Client.Network.OnRoundStarted -= Network_OnRoundStarted;
            Client.Network.OnRoundEnded -= Network_OnRoundEnded;
            Client.Network.OnGameEnded -= Network_OnGameEnded;
            Client.Network.OnOtherPlayerLeft -= Network_OnOtherPlayerLeft;
            Client.Network.OnDisconnected -= Network_OnDisconnected;
            Client.Network.OnChatMessageReceived -= Network_OnChatMessageReceived;
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

					lock (_hudStateLocker)
					{
						if (_hudState == HudState.WaitingForRoundEnd)
						{
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
					}
				}
				else
				{
					_secondsLeft -= delta;
				}
			}

			_gameHud.UpdateTimer((int)_secondsLeft);
		}

		#region View event handlers

		internal void DeleteCommand(int orderIndex)
		{
            _clientPlayer.DeleteCommand(orderIndex);
            _gameHud.UpdateCommandList(_clientPlayer.Commands, orderIndex);
            _gameHud.UpdateClientPlayerFleetData(_clientPlayer);
		}
		internal void LeaveGame()
		{
			var messageBox = new MessageBox(MessageBoxButtons.None)
			{
				Message = "Leaving game..."
			};
			ViewMgr.PushLayer(messageBox);

			Client.Network.BeginLeaveGame(OnLeaveGame, messageBox);
		}
		internal void SendCommands()
		{
			Client.Network.BeginSendCommands(_clientPlayer.Commands, OnSendOrders, null);
            _clientPlayer.ClearCommandList();
            _gameHud.UpdateCommandList(_clientPlayer.Commands);

			InvokeOnMainThread((obj) =>
			{
                if (_secondsLeft > 0)
                {
                    _secondsLeft = 0.001;
                }
			});
		}
		internal void SelectPlanet(Planet selectedPlanet)
		{
			Scene.SelectedPlanet = selectedPlanet.Id;
		}
		internal void OnHoverPlanet(Planet hoverPlanet)
		{
			Scene.HoveredPlanet = hoverPlanet.Id;
		}
		internal void UnhoverPlanets()
		{
			Scene.HoveredPlanet = 0;
		}
		internal void DeployFleet(Planet planet)
		{
			// Deploment is only possible on clients own planet
            if (planet.Owner == null)
            {
                _gameHud.AddMessage("Cannot deploy fleet: you have to own the target planet.");
                return;
            }
            if (!planet.Owner.Username.Equals(_clientPlayer.Username))
            {
                _gameHud.AddMessage("Cannot deploy fleet: you have to own the target planet.");
                return;
            }
            if (_clientPlayer.DeployableFleets == 0)
            {
                _gameHud.AddMessage("Cannot deploy fleet: Not enough deployable fleets.");
                return;
            }

            _clientPlayer.DeployFleet(planet);
            _gameHud.UpdateCommandList(_clientPlayer.Commands);
			_gameHud.UpdateClientPlayerFleetData(_clientPlayer);
		}
		internal void UndeployFleet(Planet planet)
		{
			var command = _clientPlayer.Commands.Find(cmd => cmd.Type == UserCommand.CommandType.Deploy && cmd.TargetId == planet.Id);
            if (command == null)
            {
                _gameHud.AddMessage("Cannot revert deploy: no fleets deployed to selected planet.");
                return;
            }

            _clientPlayer.UndeployFleet(planet);
            _gameHud.UpdateCommandList(_clientPlayer.Commands);
            _gameHud.UpdateClientPlayerFleetData(_clientPlayer);
		}
		internal void OnHoverLink(PlanetLink hoverLink)
		{
			Scene.HoveredLink = hoverLink;
		}
		internal void UnhoverLinks()
		{
			Scene.HoveredLink = null;
		}
		internal void MoveFleet(PlanetLink link)
		{
            var source = Scene.Map.GetPlanetById(link.SourcePlanet);
            var target = Scene.Map.GetPlanetById(link.TargetPlanet);

            if (source.Owner == null || !_clientPlayer.Username.Equals(source.Owner.Username))
            {
                _gameHud.AddMessage("Cannot move fleet: fleets can be sent only from owned planets.");
                return;
            }
            if (source.NumFleetsPresent < 2)
            {
                _gameHud.AddMessage("Cannot move fleet: there must be at least one fleet remaining.");
                return;
            }

            _clientPlayer.MoveFleet(source, target);
            _gameHud.UpdateCommandList(_clientPlayer.Commands);
		}
        internal void RevertMoveFleet(PlanetLink link)
        {
            var source = Scene.Map.GetPlanetById(link.SourcePlanet);
            var target = Scene.Map.GetPlanetById(link.TargetPlanet);

            var targetCommand = _clientPlayer.Commands.Find(cmd => cmd.SourceId == source.Id && cmd.TargetId == target.Id);
            if (targetCommand == null)
            {
                _gameHud.AddMessage("Cannot revert fleet move: no fleets moving.");
                return;
            }

            _clientPlayer.RevertFleetMove(source, target);
            _gameHud.UpdateCommandList(_clientPlayer.Commands);
        }
        internal void SendChatMessage(string message)
        {            
            Client.Network.BeginSendChatMessage(message, (res) => { try { Client.Network.EndSendChatMessage(res); } catch { } }, null);
        }

		#endregion

		#region Async network callbacks

		private void OnLeaveGame(IAsyncResult result)
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

        private void OnSendOrders(IAsyncResult result)
		{
			InvokeOnMainThread(obj =>
			{
				Client.Network.EndSendCommands(result);
			});
		}

        private bool Network_OnRoundStarted(NewRoundInfo roundInfo)
		{
			lock (_hudStateLocker)
			{
				if (_hudState == HudState.WaitingForRoundStart)
				{
                    InvokeOnMainThread(obj =>
                    {
                        _secondsLeft = roundInfo.RoundTime;
                        _gameHud.UpdateTimer((int)_secondsLeft);
                        _hudState = HudState.WaitingForRoundEnd;
                    });

					// We have consumed that packet.
					return true;
				}

				return false;
			}
		}

        private bool Network_OnRoundEnded(List<SimulationResult> simResults)
		{
			lock (_hudStateLocker)
			{
				if (_hudState == HudState.WaitingForRoundEnd)
				{
                    InvokeOnMainThread(obj =>
                    {
                        if (ViewMgr.PeekLayer() is MessageBox)
                        {
                            // Pop MessageBox "Waiting for server to simulate the turn."
                            ViewMgr.PopLayer();
                        }

                        _hudState = HudState.AnimatingSimulationResult;
                        // TODO do some animations using simulation results and then set _hudState to WaitingForRoundStart.

                        foreach (var simResult in simResults)
                        {
                            Scene.ImplementChange(simResult);
                        }

                        // TODO when animation is done that line should be moved to the end of animation.
                        _hudState = HudState.WaitingForRoundStart;
                    });

					// We have consumed that packet.
					return true;
				}

				return false;
			}
		}

        private void Network_OnGameEnded(/*game result here!*/)
		{
			// TODO show game result and statistics
			throw new NotImplementedException();
		}

        private void Network_OnOtherPlayerLeft(string username, DateTime time)
		{
            InvokeOnMainThread(obj =>
            {
                _players.RemoveAll(player => player.Username.Equals(username));
                _gameHud.UpdatePlayerList(_players);
            });
			// TODO print info (somewhere) about it!
		}

        private void Network_OnDisconnected(string reason)
		{
			InvokeOnMainThread(obj =>
			{
				var menuState = new MenuState(Game);
				Client.ChangeState(menuState);
				menuState.OnDisconnected("Disconnection", "You were disconnected from the server.");
			});
		}

        private void Network_OnChatMessageReceived(ChatMessage msg)
        {
            InvokeOnMainThread(obj =>
            {
                _gameHud.AddMessage(string.Format("<{0}/{1}>: {2}", msg.Username, msg.Time, msg.Message));
            }, msg);
        }

		#endregion
	}
}
