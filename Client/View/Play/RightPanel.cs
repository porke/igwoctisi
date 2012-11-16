namespace Client.View.Play
{
	using Client.Common;
	using Client.View.Controls;
	using Nuclex.UserInterface;
	using Nuclex.UserInterface.Controls;
	using Nuclex.UserInterface.Controls.Arcade;
	using Nuclex.UserInterface.Controls.Desktop;
	using System;
	using System.Collections.Generic;
	using Client.Model;
	using System.Linq;

	class RightPanel : TabbedPaneControl
	{
		public event EventHandler CommandDeleted;
		public event EventHandler CommandsSent;

		public RightPanel()
			: base(new UniRectangle(new UniScalar(0.8f, 0), new UniScalar(64), new UniScalar(0.2f, 0), new UniScalar(0.3f, 0)), TabHeaderPosition.Left)
		{
			CreateOrdersTab();
			CreatePlayersTab();
		}

		#region Update functions

		public void UpdatePlayerList(List<Player> players)
		{
			_playerList.Items.Clear();
			_playerList.Items.AddRange(players.Select(player => player.Username));
		}

		public void EnableCommandButtons()
		{
			_sendCommands.Enabled = true;
			_deleteCommand.Enabled = true;
		}

		public void UpdateCommands(List<UserCommand> commands, int selectedCommand = -1)
		{
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

		private void SendCommands_Pressed(object sender, EventArgs e)
		{
			if (CommandsSent != null)
			{
				CommandsSent(_commandList, e);
				_sendCommands.Enabled = false;
				_deleteCommand.Enabled = false;
			}
		}

		private void DeleteCommand_Pressed(object sender, EventArgs e)
		{
			if (CommandDeleted != null)
			{
				CommandDeleted(_commandList, e);
			}
		}

		#endregion

		private void CreatePlayersTab()
		{
			_playerList = new ListControl()
			{
				SelectionMode = ListSelectionMode.None,
				Bounds = new UniRectangle(new UniScalar(0.05f, 0), new UniScalar(0.05f, 0), new UniScalar(0.9f, 0), new UniScalar(0.9f, 0))
			};

			AddTab("P", _playerList);
		}

		private void CreateOrdersTab()
		{
			var panel = new LabelControl
			{
				Bounds = new UniRectangle(new UniScalar(), new UniScalar(), new UniScalar(1.0f, 0), new UniScalar(1.0f, 0))
			};
			_commandList = new WrappableListControl()
			{
				Bounds = new UniRectangle(new UniScalar(0.05f, 0), new UniScalar(0.05f, 0), new UniScalar(0.9f, 0), new UniScalar(0.75f, 0))
			};

			_deleteCommand = new ButtonControl()
			{
				Text = "Delete",
				Bounds = new UniRectangle(new UniScalar(0.55f, 0), new UniScalar(0.82f, 0), new UniScalar(0.4f, 0), new UniScalar(0.15f, 0))
			};
			_deleteCommand.Pressed += DeleteCommand_Pressed;

			_sendCommands = new ButtonControl()
			{
				Text = "Send",
				Bounds = new UniRectangle(new UniScalar(0.05f, 0), new UniScalar(0.82f, 0), new UniScalar(0.4f, 0), new UniScalar(0.15f, 0))
			};
			_sendCommands.Pressed += SendCommands_Pressed;

			panel.Children.AddRange(new Control[] { _commandList, _deleteCommand, _sendCommands });
			AddTab("O", panel);
		}

		private WrappableListControl _commandList;
		private ListControl _playerList;
		private ButtonControl _sendCommands;
		private ButtonControl _deleteCommand;
	}
}
