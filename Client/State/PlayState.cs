namespace Client.State
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Threading;
	using Client.Common;
	using Client.Renderer;
	using Client.Renderer.Particles;
	using Client.View;
	using Microsoft.Xna.Framework;
	using Model;
	using Nuclex.UserInterface.Visuals.Flat;
	using View.Play;

	class PlayState : GameState
	{
		#region Internal enum - HudState

		private enum HudState
		{
			Initializing,
			WaitingForRoundStart,
			WaitingForRoundEnd,
			AnimatingSimulationResult
		}

		#endregion

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

		#region View event handlers (viewport)
		
		private void View_SelectPlanet(object sender, EventArgs<Planet> e)
		{
			var selectedPlanet = e.Item;
			if (Scene.CanSelectPlanet(selectedPlanet, _clientPlayer))
			{
				Scene.SelectPlanet(selectedPlanet);
			}
		}
		private void View_OnHoverPlanet(object sender, EventArgs<Planet> e)
		{
			var hoverPlanet = e.Item;
			Scene.HoveredPlanet = hoverPlanet.Id;
		}
		private void View_UnhoverPlanets(object sender, EventArgs e)
		{
			Scene.HoveredPlanet = 0;
		}
		private void View_DeployFleet(object sender, EventArgs<Planet, int> e)
		{
			var planet = e.Item1;
			int count = e.Item2;

			// Deploment is only possible on clients own planet
			if (planet.Owner == null)
			{
				_gameHud.SetNotification("Cannot deploy fleet: you have to own the target planet.");
				return;
			}
			if (!planet.Owner.Username.Equals(_clientPlayer.Username))
			{
				_gameHud.SetNotification("Cannot deploy fleet: you have to own the target planet.");
				return;
			}
			if (_clientPlayer.DeployableFleets < count)
			{
				if (_clientPlayer.DeployableFleets == 0)
				{
					_gameHud.SetNotification("Cannot deploy fleet: Not enough deployable fleets.");
					return;
				}
				else
				{
					count = _clientPlayer.DeployableFleets;
				}
			}

			_clientPlayer.DeployFleet(planet, count);
			_gameHud.UpdateCommandList(_clientPlayer.Commands);
			_gameHud.UpdateResourceData(_clientPlayer);
		}
		private void View_RevertDeployFleet(object sender, EventArgs<Planet, int> e)
		{
			var planet = e.Item1;
			int count = e.Item2;

			var command = _clientPlayer.Commands.Find(cmd => cmd.Type == UserCommand.CommandType.Deploy && cmd.TargetId == planet.Id);
			if (command == null)
			{
				_gameHud.SetNotification("Cannot revert deploy: no fleets deployed to selected planet.");
				return;
			}

			_clientPlayer.UndeployFleet(planet, count);
			_gameHud.UpdateCommandList(_clientPlayer.Commands);
			_gameHud.UpdateResourceData(_clientPlayer);
		}
		private void View_OnHoverLink(object sender, EventArgs<PlanetLink> e)
		{
			var hoverLink = e.Item;
			Scene.HoveredLink = hoverLink;
		}
		private void View_UnhoverLinks(object sender, EventArgs e)
		{
			Scene.HoveredLink = null;
		}
		private void View_MoveFleet(object sender, EventArgs<PlanetLink, int> e)
		{
			var link = e.Item1;
			int count = e.Item2;
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
				_gameHud.SetNotification("Cannot move fleet: fleets can be sent only from owned planets.");
				return;
			}

			// The FleetChange can be negative, hence the absolute value
			if (Math.Abs(source.FleetChange - count) > source.NumFleetsPresent - 1)
			{
				if (source.NumFleetsPresent < 1
					|| source.FleetChange + source.NumFleetsPresent == 1)
				{
					_gameHud.SetNotification("Cannot move fleet: there must be at least one fleet remaining.");
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
		private void View_RevertMoveFleet(object sender, EventArgs<PlanetLink, int> e)
		{
			var link = e.Item1;
			int count = e.Item2;
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
				_gameHud.SetNotification("Cannot revert fleet move: no fleets moving.");
				return;
			}			

			_clientPlayer.RevertFleetMove(source, target, count);
			_gameHud.UpdateCommandList(_clientPlayer.Commands);
		}

		#endregion

		#region Hud event handlers

		private void HUD_SendChatMessage(object sender, EventArgs<string> arg)
		{
			var message = arg.Item;
			Client.Network.BeginSendChatMessage(message, (res) => { try { Client.Network.EndSendChatMessage(res); } catch { } }, null);
		}
		private void HUD_RaiseTechnology(object sender, EventArgs<TechnologyType> arg)
		{
			var techType = arg.Item;
			string reason = string.Empty;
			if (_clientPlayer.CanRaiseTech(techType, ref reason))
			{
				_clientPlayer.RaiseTech(techType);
				_gameHud.UpdateCommandList(_clientPlayer.Commands);
				_gameHud.UpdateResourceData(_clientPlayer);
			}
			else
			{
				_gameHud.SetNotification(reason);
			}
		}
		private void HUD_DeleteCommand(object sender, EventArgs<int> arg)
		{
			int orderIndex = arg.Item;
			_clientPlayer.DeleteCommand(orderIndex);
			_gameHud.UpdateCommandList(_clientPlayer.Commands, orderIndex);
			_gameHud.UpdateResourceData(_clientPlayer);
		}
		private void HUD_LeaveGame(object sender, EventArgs arg)
		{
			var messageBox = new MessageBox(this, MessageBoxButtons.None)
			{
				Message = "Leaving game..."
			};
			ViewMgr.PushLayer(messageBox);

			Client.Network.BeginLeaveGame(OnLeaveGame, messageBox);
		}
		private void HUD_SendCommands(object sender, EventArgs arg)
		{
			Client.Network.BeginSendCommands(_clientPlayer.Commands, OnSendOrders, null);
			_clientPlayer.ClearCommandList();
			_gameHud.UpdateCommandList(_clientPlayer.Commands);
		}
		private void HUD_ShowGameInBrowser(object sender, EventArgs<int> arg)
		{
			Process.Start(string.Format("http://zpi.modriv.net/game/{0}", arg.Item));
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
						Scene.Initialize(roundInfo, _players, _clientPlayer);
						_gameHud.UpdateResourceData(_clientPlayer);
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
						_clientPlayer.DeployableFleets = roundInfo.FleetsToDeploy;

						// Updates planet owners and fleet states
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

						// Update technology data
						_clientPlayer.Technologies[TechnologyType.Economic].CurrentLevel = roundInfo.Tech.Economic;
						_clientPlayer.Technologies[TechnologyType.Defensive].CurrentLevel = roundInfo.Tech.Defensive;
						_clientPlayer.Technologies[TechnologyType.Offensive].CurrentLevel = roundInfo.Tech.Offensive;
						_clientPlayer.TechPoints = roundInfo.Tech.Points;

						_gameHud.UpdateResourceData(_clientPlayer);
						_gameHud.EnableCommandButtons();

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
						_gameViewport.Enabled = false;
						_gameHud.EnterAnimationMode();
						List<UserCommand> commands = null;
						commands = Scene.AnimateChanges(simResults,
							 (startedCommandIndex) =>
							 {
								 InvokeOnMainThread(arg =>
								 {
									 _gameHud.UpdateCommandList(commands, startedCommandIndex);
								 });
							 },
							 () =>
							 {
								 // Animation is done.
								 InvokeOnMainThread(arg =>
								 {
									 commands.Clear();
									 _gameHud.UpdateCommandList(commands, -1);
									 _gameHud.ExitAnimationMode();
									 _gameViewport.Enabled = true;
									 _hudState = HudState.WaitingForRoundStart;
									 Client.Network.BeginSetReady(null, null);
								 });
							 });

						_gameHud.UpdateCommandList(commands, commands.Count > 0 ? 0 : -1);
					});

					// We have consumed that packet.
					return true;
				}

				return false;
			}
		}

		private void Network_OnGameEnded(EndgameData stats)
		{			
			InvokeOnMainThread(obj =>
			{
				var statsWindow = new GameStats(this, stats, _clientPlayer.Username);
				statsWindow.LeavePressed += HUD_LeaveGame;
				statsWindow.LadderPressed += HUD_ShowGameInBrowser;
				ViewMgr.PushLayer(statsWindow);
			});
		}

		private void Network_OnOtherPlayerLeft(string username, DateTime time)
		{
			InvokeOnMainThread(obj =>
			{
				_players.RemoveAll(player => player.Username.Equals(username));
				_gameHud.UpdatePlayerList(_players);
				_gameHud.SetNotification(string.Format("Player {0} has left.", username));
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
				_gameHud.AddMessage(string.Format("[{0}] {1}: {2}", msg.Time, msg.Username, msg.Message));
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
			
			// Remove particles
			int i = 0;
			while (i < Client.Components.Count)
			{
				if (Client.Components[i] is ParticleSystem)
				{
					Client.Components.RemoveAt(i);
				}
				else
				{
					i++;
				}
			}
		}		
		public override void Update(double delta, double time)
		{
			base.Update(delta, time);
			Scene.Update(delta, time);

			// Update timer
			if (_secondsLeft > 0)
			{
				if (_secondsLeft - delta <= 0)
				{
					_secondsLeft = 0;

					lock (_hudStateLocker)
					{
						_hudState = HudState.WaitingForRoundEnd;
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

		private void CreatePlayerColorIcons()
		{
			const int frameYPosition = 479;
			const int frameSize = 32;

			var flatGui = Client.Visualizer as FlatGuiVisualizer;
			var iconTexture = flatGui.flatGuiGraphics.bitmaps["hud_icons"];
			foreach (var color in _loadedMap.Colors)
			{
				// Valid Ids begin from 1				
				int frameXPosition = (frameSize + 1) * (color.ColorId - 1);
				uint[] pixels = new uint[frameSize * frameSize];

				for (int p = 0; p < pixels.Length; ++p)
				{
					int col = p % frameSize;
					int row = p / frameSize;

					// Circle inequality
					const int radius = frameSize / 4;
					const int center = frameSize / 2;
					int x = col - center;
					int y = row - center;					
					if (x*x + y*y < radius*radius)
					{
						pixels[p] = color.XnaColor.PackedValue;
					}
				}

				iconTexture.SetData<uint>(0, new Rectangle(frameXPosition, frameYPosition, frameSize, frameSize), pixels, 0, pixels.Length);
			}
		}

		public PlayState(IGWOCTISI game, Map loadedMap, Player clientPlayer)
			: base(game)
		{
			_loadedMap = loadedMap;
			_clientPlayer = clientPlayer;
			_clientPlayer.IsClientPlayer = true;

			CreatePlayerColorIcons();

			Scene = new Scene(_loadedMap);
			Scene.Visual = new SceneVisual(Client, Scene, ViewMgr.AnimationManager);
			_gameViewport = new GameViewport(this);
			_gameHud = new GameHud(this);

			ViewMgr.PushLayer(_gameViewport);
			ViewMgr.PushLayer(_gameHud);			

			Client.Network.OnRoundStarted += Network_OnRoundStarted;
			Client.Network.OnRoundEnded += Network_OnRoundEnded;
			Client.Network.OnGameEnded += Network_OnGameEnded;
			Client.Network.OnOtherPlayerLeft += Network_OnOtherPlayerLeft;
			Client.Network.OnDisconnected += Network_OnDisconnected;
			Client.Network.OnChatMessageReceived += Network_OnChatMessageReceived;

			_gameHud.RaiseTechPressed += HUD_RaiseTechnology;
			_gameHud.ChatMessageSent += HUD_SendChatMessage;
			_gameHud.DeleteCommandPressed += HUD_DeleteCommand;
			_gameHud.SendCommandsPressed += HUD_SendCommands;
			_gameHud.LeaveGamePressed += HUD_LeaveGame;

			_gameViewport.PlanetsUnhovered += View_UnhoverPlanets;
			_gameViewport.LinksUnhovered += View_UnhoverLinks;
			_gameViewport.PlanetHovered += View_OnHoverPlanet;
			_gameViewport.LinkHovered += View_OnHoverLink;
			_gameViewport.PlanetSelected += View_SelectPlanet;
			_gameViewport.FleetMoved += View_MoveFleet;
			_gameViewport.FleetMoveReverted += View_RevertMoveFleet;
			_gameViewport.FleetDeployReverted += View_RevertDeployFleet;
			_gameViewport.FleetDeployed += View_DeployFleet;
		}
	}
}
