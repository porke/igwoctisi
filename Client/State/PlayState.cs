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

		private List<UserCommand> _commands = new List<UserCommand>();
		private Player _clientPlayer;
		private List<Player> _players;
		private double _secondsLeft = 0;

		public PlayState(IGWOCTISI game, Map loadedMap, Player clientPlayer, List<Player> players)
			: base(game)
		{
			_clientPlayer = clientPlayer;
			_players = players;

            Scene = new Scene(loadedMap, players);
			_gameViewport = new GameViewport(this);
			_gameHud = new GameHud(this);
			_gameHud.UpdateClientPlayerFleetData(clientPlayer);
			_gameHud.UpdatePlayerList(players);

			ViewMgr.PushLayer(_gameViewport);
			ViewMgr.PushLayer(_gameHud);

			Client.Network.OnRoundStarted += Network_OnRoundStarted;
			Client.Network.OnRoundEnded += Network_OnRoundEnded;
			Client.Network.OnGameEnded += Network_OnGameEnded;
			Client.Network.OnOtherPlayerLeft += Network_OnOtherPlayerLeft;
			Client.Network.OnDisconnected += Network_OnDisconnected;
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

        internal void DeleteCommand(int orderIndex)
        {
            _commands.RemoveAt(orderIndex);
            (ViewMgr.PeekLayer() as GameHud).UpdateCommandList(_commands);
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
			Client.Network.BeginSendCommands(_commands, OnSendOrders, null);
			_commands.Clear();
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
			var gameHud = ViewMgr.PeekLayer() as GameHud;

            // Deploment is only possible on clients own planet
            if (planet.Owner == null || !planet.Owner.Username.Equals(_clientPlayer.Username)) return;

            var command = _commands.Find(cmd => cmd.Type == UserCommand.CommandType.Deploy && cmd.TargetId == planet.Id);
            if (command == null)
            {
                command = new UserCommand(planet, 1);
                _commands.Add(command);
            }
            else
            {
                command.FleetCount++;
            }

            planet.NumFleetsPresent++;
            _clientPlayer.DeployableFleets--;
            _gameHud.UpdateCommandList(_commands);
            _gameHud.UpdateClientPlayerFleetData(_clientPlayer);
		}
		internal void UndeployFleet(Planet planet)
		{
			var gameHud = ViewMgr.PeekLayer() as GameHud;

            var command = _commands.Find(cmd => cmd.Type == UserCommand.CommandType.Deploy && cmd.TargetId == planet.Id);
            if (command != null)
            {
                command.FleetCount--;
                planet.NumFleetsPresent--;
                _clientPlayer.DeployableFleets++;

                if (command.FleetCount == 0)
                {
                    _commands.Remove(command);
                }

                _gameHud.UpdateCommandList(_commands);
                _gameHud.UpdateClientPlayerFleetData(_clientPlayer);
            }
		}
		internal void OnHoverLink(PlanetLink hoverLink)
		{
			Scene.HoveredLink = hoverLink;
		}
		internal void UnhoverLinks()
		{
			Scene.HoveredLink = null;
		}
		internal void SelectLink()
		{
            // TODO: Implement deployment command
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

		void OnSendOrders(IAsyncResult result)
		{
			InvokeOnMainThread(obj =>
			{
				Client.Network.EndSendCommands(result);
				_secondsLeft = 0.001;
			});
		}

		void Network_OnRoundStarted(NewRoundInfo roundInfo)
		{
			_secondsLeft = roundInfo.RoundTime;
			_gameHud.UpdateTimer((int)_secondsLeft);

			// TODO update gui to enable it for making new moves
		}

        void Network_OnRoundEnded(List<SimulationResult> simRes)
		{
			// TODO collect info about moves and animate them
			throw new NotImplementedException("Handle simulation result.");
		}

		void Network_OnGameEnded(/*game result here!*/)
		{
			// TODO show game result and statistics
			throw new NotImplementedException();
		}

		void Network_OnOtherPlayerLeft(string username, DateTime time)
		{
			_players.RemoveAll(player => player.Username.Equals(username));
			_gameHud.UpdatePlayerList(_players);

			// TODO print info (somewhere) about it!
		}

		void Network_OnDisconnected(string reason)
		{
			InvokeOnMainThread(obj =>
			{
				var menuState = new MenuState(Game);
				Client.ChangeState(menuState);
				menuState.OnDisconnected("Disconnection", "You were disconnected from the server.");
			});
		}

		#endregion
	}
}
