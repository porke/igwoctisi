namespace Client.State
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using Client.Common;
	using Client.Renderer;
	using Client.View;
	using Model;
	using View.Play;

	class PlayState : GameState
	{
		#region Private members

		private Map _loadedMap;
		private Player _clientPlayer;
		private List<Player> _players;
		private double _secondsLeft = 0;
		private HudState _hudState = HudState.Initializing;
		private object _hudStateLocker = new object();
		private GameViewport _gameViewport;
		private GameHud _gameHud;

		#endregion

		#region View event handlers

		internal void DeleteCommand(int orderIndex)
		{
			_clientPlayer.DeleteCommand(orderIndex);
			_gameHud.UpdateCommandList(_clientPlayer.Commands, orderIndex);
			_gameHud.UpdateClientPlayerResourceData(_clientPlayer);
		}
		internal void LeaveGame()
		{
			var messageBox = new MessageBox(this, MessageBoxButtons.None)
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
			if (Scene.CanSelectPlanet(selectedPlanet, _clientPlayer))
			{
				Scene.SelectPlanet(selectedPlanet);
			}
		}
		internal void OnHoverPlanet(Planet hoverPlanet)
		{
			Scene.HoveredPlanet = hoverPlanet.Id;
		}
		internal void UnhoverPlanets()
		{
			Scene.HoveredPlanet = 0;
		}
		internal void DeployFleet(Planet planet, int count)
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
			if (_clientPlayer.DeployableFleets < count)
			{
				if (_clientPlayer.DeployableFleets == 0)
				{
					_gameHud.AddMessage("Cannot deploy fleet: Not enough deployable fleets.");
					return;
				}
				else
				{
					count = _clientPlayer.DeployableFleets;
				}
			}

			_clientPlayer.DeployFleet(planet, count);
			_gameHud.UpdateCommandList(_clientPlayer.Commands);
			_gameHud.UpdateClientPlayerResourceData(_clientPlayer);
		}
		internal void UndeployFleet(Planet planet, int count)
		{
			var command = _clientPlayer.Commands.Find(cmd => cmd.Type == UserCommand.CommandType.Deploy && cmd.TargetId == planet.Id);
			if (command == null)
			{
				_gameHud.AddMessage("Cannot revert deploy: no fleets deployed to selected planet.");
				return;
			}

			_clientPlayer.UndeployFleet(planet, count);
			_gameHud.UpdateCommandList(_clientPlayer.Commands);
			_gameHud.UpdateClientPlayerResourceData(_clientPlayer);
		}
		internal void OnHoverLink(PlanetLink hoverLink)
		{
			Scene.HoveredLink = hoverLink;
		}
		internal void UnhoverLinks()
		{
			Scene.HoveredLink = null;
		}
		internal void MoveFleet(PlanetLink link, int count)
		{
			var source = Scene.Map.GetPlanetById(link.SourcePlanet);
			var target = Scene.Map.GetPlanetById(link.TargetPlanet);

			// The links are bidirectional, but represented only once in the list
			// Swap source and target to compensate for this			
			if (Scene.SelectedPlanet == target.Id)
			{
				Utils.SwapRef(ref source, ref target);
			}

			if (source.Owner == null || !_clientPlayer.Username.Equals(source.Owner.Username))
			{
				_gameHud.AddMessage("Cannot move fleet: fleets can be sent only from owned planets.");
				return;
			}

			// The FleetChange can be negative, hence the absolute value
			if (Math.Abs(source.FleetChange - count) > source.NumFleetsPresent - 1)
			{
				if (source.NumFleetsPresent == 1
					|| source.FleetChange + source.NumFleetsPresent == 1)
				{
					_gameHud.AddMessage("Cannot move fleet: there must be at least one fleet remaining.");
					return;
				}
				else
				{
					count = source.FleetChange + source.NumFleetsPresent - 1;
				}
			}

			_clientPlayer.MoveFleet(source, target, count);
			_gameHud.UpdateCommandList(_clientPlayer.Commands);
		}
		internal void RevertMoveFleet(PlanetLink link, int count)
		{
			var source = Scene.Map.GetPlanetById(link.SourcePlanet);
			var target = Scene.Map.GetPlanetById(link.TargetPlanet);

			// The links are bidirectional, but represented only once in the list
			// Swap source and target to compensate for this			
			if (Scene.SelectedPlanet == target.Id)
			{
				Utils.SwapRef(ref source, ref target);
			}

			var targetCommand = _clientPlayer.Commands.Find(cmd => cmd.SourceId == source.Id && cmd.TargetId == target.Id);
			if (targetCommand == null)
			{
				_gameHud.AddMessage("Cannot revert fleet move: no fleets moving.");
				return;
			}			

			_clientPlayer.RevertFleetMove(source, target, count);
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
				if (_hudState == HudState.Initializing)
				{
					// Collect player list. Don't forget about existing player reference.
					_players = roundInfo.Players
						.Select(username =>
							username.Equals(_clientPlayer.Username)
								? _clientPlayer
								: new Player(username, null)
							)
						.ToList();

					// Assign players to the planets.
					var locker = new ManualResetEvent(false);
					InvokeOnMainThread(obj =>
					{
						Scene.Initialize(roundInfo, _players);
						Scene.Map.UpdatePlanetShowDetails(_clientPlayer);
						_gameHud.UpdateClientPlayerResourceData(_clientPlayer);
						_gameHud.UpdatePlayerList(_players);
						locker.Set();
					});
					locker.WaitOne();

					// Use the rest of round info by going to next state.
					_hudState = HudState.WaitingForRoundStart;
				}

				// Don't add "else" here!
				// Initializing happens right before using round info.
				if (_hudState == HudState.WaitingForRoundStart)
				{
					InvokeOnMainThread(obj =>
					{
						// Update timer
						_secondsLeft = roundInfo.RoundTime;
						_gameHud.UpdateTimer((int)_secondsLeft);

						// Update world info.
						_clientPlayer.DeployableFleets += roundInfo.FleetsToDeploy;

						// TODO update tech info due to `roundInfo.Tech'
						foreach (var planetUpdateData in roundInfo.Map)
						{
							var planet = _loadedMap.Planets.Find(p => p.Id == planetUpdateData.PlanetId);
							planet.NumFleetsPresent = planetUpdateData.Fleets;
							planet.FleetChange = 0;

							if (!string.IsNullOrEmpty(planetUpdateData.Player))
							{
								planet.Owner = _players.Find(p => p.Username.Equals(planetUpdateData.Player));
							}
							else
							{
								planet.Owner = null;
							}
						}

						// Update the underlying planets for the player
						foreach (var player in _players)
						{
							var planetList = _loadedMap.Planets.FindAll(p => p.Owner != null && p.Owner.Username == player.Username);
							player.OwnedPlanets = planetList;							
							player.FleetIncomePerTurn = roundInfo.FleetsToDeploy;
						}

						_gameHud.UpdateClientPlayerResourceData(_clientPlayer);
						_loadedMap.UpdatePlanetShowDetails(_clientPlayer);

						// Now wait to the end of the round.
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

                        // Begin animation and wait until it is done.
						_hudState = HudState.AnimatingSimulationResult;
                        Scene.AnimateChanges(simResults, () =>
                        {
                            _hudState = HudState.WaitingForRoundStart;
                            Client.Network.BeginSetReady(null, null);
                        });
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
			InvokeOnMainThread(obj =>
			{
				// Mock stats
				var stats = new EndgameData();
				var statsWnd = new GameStats(this, stats);
				ViewMgr.PushLayer(statsWnd);
			});
		}

		private void Network_OnOtherPlayerLeft(string username, DateTime time)
		{
			InvokeOnMainThread(obj =>
			{
				_players.RemoveAll(player => player.Username.Equals(username));
				_gameHud.UpdatePlayerList(_players);
				_gameHud.AddMessage(string.Format("Player {0} has left.", username));
			});
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

		#region GameState members

		public override void OnEnter()
		{
			base.OnEnter();
			Client.Network.BeginSetReady(null, null);			
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
		public override void Update(double delta, double time)
		{
			base.Update(delta, time);

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
							var messageBox = new MessageBox(this, MessageBoxButtons.OK)
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

		#endregion

		public readonly Scene Scene;

		private enum HudState
		{
			Initializing,
			WaitingForRoundStart,
			WaitingForRoundEnd,
			AnimatingSimulationResult
		}

		public PlayState(IGWOCTISI game, Map loadedMap, Player clientPlayer)
			: base(game)
		{
			_loadedMap = loadedMap;
			_clientPlayer = clientPlayer;

			Scene = new Scene(_loadedMap);
			Scene.Visual = new SceneVisual(Scene, loadedMap.Colors, Client.Content, ViewMgr.AnimationManager);
			_gameViewport = new GameViewport(this);
			_gameHud = new GameHud(this);

			ViewMgr.PushLayer(_gameViewport);
			ViewMgr.PushLayer(_gameHud);
			
			// TODO: Testing stats window
			//var stats = new EndgameData();
			//ViewMgr.PushLayer(new GameStats(this, stats));

			Client.Network.OnRoundStarted += Network_OnRoundStarted;
			Client.Network.OnRoundEnded += Network_OnRoundEnded;
			Client.Network.OnGameEnded += Network_OnGameEnded;
			Client.Network.OnOtherPlayerLeft += Network_OnOtherPlayerLeft;
			Client.Network.OnDisconnected += Network_OnDisconnected;
			Client.Network.OnChatMessageReceived += Network_OnChatMessageReceived;
		}
	}
}
