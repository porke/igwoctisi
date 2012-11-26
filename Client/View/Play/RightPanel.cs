namespace Client.View.Play
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Client.Common;
	using Client.Model;
	using Client.View.Controls;
	using Nuclex.UserInterface;
	using Nuclex.UserInterface.Controls;
	using Nuclex.UserInterface.Controls.Desktop;

	public class RightPanel : TabbedPaneControl
	{
		public event EventHandler CommandDeleted;		
	
		public RightPanel()
			: base(new UniRectangle(new UniVector(new UniScalar(1.0f, 0), new UniScalar(64)), new UniVector(new UniScalar(0.2f, 0), new UniScalar(0.6f, 0))), TabHeaderPosition.Left)
		{
			TogglePosition = new UniVector(new UniScalar(1.0f, -40), new UniScalar(64));
			DefaultPosition = new UniVector(new UniScalar(0.8f, 0), new UniScalar(64));

			CreateCommandsTab();
			CreatePlayersTab();
			TabChanged += RightPanel_TabChanged;
		}

		#region Update functions

		public void UpdatePlayerList(List<Player> players)
		{
			_playerList.Clear();

			foreach (var item in players)
			{
				var row = new ExtendedListControl.ExtendedListRow();
				var colorIcon = new IconControl(string.Format("color_{0}", item.Color.ColorId))
				{
					Bounds = new UniRectangle(new UniScalar(), new UniScalar(4), new UniScalar(), new UniScalar(24))
				};
				var nickLabel = new LabelControl(item.Username)
				{
					Bounds = new UniRectangle(new UniScalar(), new UniScalar(4), new UniScalar(), new UniScalar())
				};

				row.AddSegment(colorIcon, new UniScalar(32));
				row.AddSegment(nickLabel, new UniScalar());
				_playerList.AddRow(row);				
			}
		}

		public void SetEnableButtons(bool enable)
		{			
			_deleteCommand.Enabled = enable;
		}

		public void UpdateCommands(List<UserCommand> commands, int selectedCommand = -1)
		{
			// If this is the wring tab, save the command list, it will be updated once the command tab is activated
			if (ActiveTab.Name != CommandsTabName)
			{
				_lastCommandListAndSelectedOrder = new Tuple<List<UserCommand>, int>(commands, selectedCommand);
				return;
			}

			_commandList.Clear();

			// Not using sort, because it's unstable
			var orderedCmds = commands.OrderByDescending(cmd => (int)cmd.Type);
			commands = new List<UserCommand>(orderedCmds);
			foreach (var cmd in commands)
			{
				if (cmd.Type == UserCommand.CommandType.Deploy)
				{
					_commandList.AddItem(string.Format("D: {0} to {1}", cmd.FleetCount, cmd.TargetPlanet.Name));
				}
				else if (cmd.Type == UserCommand.CommandType.Move)
				{
					_commandList.AddItem(string.Format("M: {0} from {1} to {2}", cmd.FleetCount, cmd.SourcePlanet.Name, cmd.TargetPlanet.Name));
				}
				else if (cmd.Type == UserCommand.CommandType.Attack)
				{
					_commandList.AddItem(string.Format("A: {0} from {1} to {2}", cmd.FleetCount, cmd.SourcePlanet.Name, cmd.TargetPlanet.Name));
				}
				else if (cmd.Type == UserCommand.CommandType.Tech)
				{
					_commandList.AddItem(string.Format("T: Research {0} tech", cmd.TechType));
				}
			}

			_commandList.SelectItem(selectedCommand);
		}

		#endregion

		#region Event handlers

		private void DeleteCommand_Pressed(object sender, EventArgs e)
		{
			if (CommandDeleted != null)
			{
				CommandDeleted(_commandList, e);
			}
		}

		private void RightPanel_TabChanged(object sender, EventArgs e)
		{
			if ((sender as Control).Name.Equals(CommandsTabName))
			{
				if (_lastCommandListAndSelectedOrder != null)
				{
					UpdateCommands(_lastCommandListAndSelectedOrder.Item1, _lastCommandListAndSelectedOrder.Item2);
					_lastCommandListAndSelectedOrder = null;
				}
			}
		}

		#endregion

		private void CreatePlayersTab()
		{
			_playerList = new ExtendedListControl(new UniScalar(32), "input.normal")
			{
				Name = PlayersTabName,
				Bounds = new UniRectangle(new UniScalar(0.05f, 0), new UniScalar(0.02f, 0), new UniScalar(0.9f, 0), new UniScalar(0.93f, 0))
			};

			var off = new string[] { "playersIconInactive", "playersIconInactive", "playersIconHover", "playersIconInactive" };
			var on = new string[] { "playersIconActive", "playersIconActive", "playersIconHover", "playersIconActive" };
			AddTab(on, off, _playerList);
		}

		private void CreateCommandsTab()
		{
			var panel = new LabelControl
			{
				Name = CommandsTabName,
				Bounds = new UniRectangle(new UniScalar(), new UniScalar(), new UniScalar(1.0f, 0), new UniScalar(1.0f, 0))
			};
			_commandList = new WrappableListControl()
			{
				Bounds = new UniRectangle(new UniScalar(0.05f, 0), new UniScalar(0.05f, 0), new UniScalar(0.9f, 0), new UniScalar(0.83f, 0))
			};

			_deleteCommand = new ButtonControl()
			{
				Text = "Delete",
				Bounds = new UniRectangle(new UniScalar(0.3f, 0), new UniScalar(0.9f, 0), new UniScalar(0.4f, 0), new UniScalar(0.08f, 0))
			};
			_deleteCommand.Pressed += DeleteCommand_Pressed;

			panel.Children.AddRange(new Control[] { _commandList, _deleteCommand });

			var off = new string[] { "commandsIconInactive", "commandsIconInactive", "commandsIconHover", "commandsIconInactive" };
			var on = new string[] { "commandsIconActive", "commandsIconActive", "commandsIconHover", "commandsIconActive" };
			AddTab(on, off, panel);
		}

		private WrappableListControl _commandList;
		private ExtendedListControl _playerList;
		private ButtonControl _deleteCommand;
		private Tuple<List<UserCommand>, int> _lastCommandListAndSelectedOrder;

		private const string CommandsTabName = "CommandsTab";
		private const string PlayersTabName = "PlayersTab";
	}
}
