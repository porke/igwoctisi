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

			eventHandlers.Add("LeaveGame", LeaveGame);
            eventHandlers.Add("DeleteCommand", DeleteCommand);
			eventHandlers.Add("SendCommands", SendCommands);
			eventHandlers.Add("SelectPlanet", SelectPlanet);
			eventHandlers.Add("OnHoverPlanet", OnHoverPlanet);
			eventHandlers.Add("UnhoverPlanets", UnhoverPlanets);
			eventHandlers.Add("DeployFleet", DeployFleet);
			eventHandlers.Add("UndeployFleet", UndeployFleet);
			eventHandlers.Add("OnHoverLink", OnHoverLink);
			eventHandlers.Add("UnhoverLinks", UnhoverLinks);
			eventHandlers.Add("SelectLink", SelectLink);

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

        private void DeleteCommand(EventArgs args)
        {
            var orderIndex = (args as DeleteCommandArgs).OrderListIndex;
            _commands.RemoveAt(orderIndex);
            (ViewMgr.PeekLayer() as GameHud).UpdateCommandList(_commands);
        }

		private void LeaveGame(EventArgs args)
		{
			var messageBox = new MessageBox(MessageBoxButtons.None)
			{
				Message = "Leaving game..."
			};
			ViewMgr.PushLayer(messageBox);

			Client.Network.BeginLeaveGame(OnLeaveGame, messageBox);
		}

		private void SendCommands(EventArgs args)
		{
			Client.Network.BeginSendCommands(_commands, OnSendOrders, null);
			_commands.Clear();
		}

		private void SelectPlanet(EventArgs args)
		{
			var selectedPlanet = (args as SelectPlanetArgs).Planet;
			Scene.SelectedPlanet = selectedPlanet.Id;
		}

		private void OnHoverPlanet(EventArgs args)
		{
			var hoverPlanet = (args as SelectPlanetArgs).Planet;
			Scene.HoveredPlanet = hoverPlanet.Id;
		}

		private void UnhoverPlanets(EventArgs args)
		{
			Scene.HoveredPlanet = 0;
		}

		private void DeployFleet(EventArgs args)
		{
			var planet = (args as SelectPlanetArgs).Planet;
			var gameHud = ViewMgr.PeekLayer() as GameHud;

            // Deploment is only possible on clients own planet
            if (planet.Owner == null || !planet.Owner.Username.Equals(_clientPlayer.Username)) return;

            var command = _commands.Find(cmd => cmd.Type == UserCommand.CommandType.Deploy && cmd.TargetId == planet.Id);
            if (command == null)
            {
                command = new UserCommand(_clientPlayer, planet, 1);
                _commands.Add(command);
            }
            else
            {
                command.UnitCount++;
            }

            planet.NumFleetsPresent++;
            _clientPlayer.DeployableFleets--;
            _gameHud.UpdateCommandList(_commands);
            _gameHud.UpdateClientPlayerFleetData(_clientPlayer);
		}

		private void UndeployFleet(EventArgs args)
		{
			var planet = (args as SelectPlanetArgs).Planet;
			var gameHud = ViewMgr.PeekLayer() as GameHud;

            var command = _commands.Find(cmd => cmd.Type == UserCommand.CommandType.Deploy && cmd.TargetId == planet.Id);
            if (command != null)
            {
                command.UnitCount--;
                planet.NumFleetsPresent--;
                _clientPlayer.DeployableFleets++;

                if (command.UnitCount == 0)
                {
                    _commands.Remove(command);
                }

                _gameHud.UpdateCommandList(_commands);
                _gameHud.UpdateClientPlayerFleetData(_clientPlayer);
            }
		}

		private void OnHoverLink(EventArgs args)
		{
			var hoverLink = (args as SelectLinkArgs).Link;
			Scene.HoveredLink = hoverLink;
		}

		private void UnhoverLinks(EventArgs args)
		{
			Scene.HoveredLink = null;
		}

		private void SelectLink(EventArgs args)
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
				menuState.HandleViewEvent("OnDisconnected", new MessageBoxArgs("Disconnection", "You were disconnected from the server."));
			});
		}

		#endregion
	}
}
