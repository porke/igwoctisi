namespace Client.View.Play
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Client.Model;
	using Client.View.Controls;
	using Common;
	using Input;
	using Nuclex.UserInterface;
	using Nuclex.UserInterface.Controls;
	using Nuclex.UserInterface.Controls.Desktop;
	using State;

    class GameHud : BaseView
	{
		#region Protected members

        private WrappableListControl _messageList;
        private CommandInputControl _chatMessage;

		private TopPanel _topPanel;
		private RightPanel _rightPanel;

        protected void CreateChildControls()
        {
			_topPanel = new TopPanel();
			_topPanel.TechRaised += RaiseTech_Pressed;
			_topPanel.LeftGame += LeaveGame_Pressed;

			_rightPanel = new RightPanel();
			_rightPanel.CommandDeleted += DeleteCommand_Pressed;
			_rightPanel.CommandsSent += SendCommands_Pressed;

            #region Message box & chat

            _chatMessage = new CommandInputControl
            {
                Bounds = new UniRectangle(new UniScalar(0.3f, 0), new UniScalar(0.93f, 0), new UniScalar(0.6f, 0), new UniScalar(0.05f, 0))
            };
            _chatMessage.OnCommandHandler += new EventHandler(ChatMessage_Execute);

            _messageList = new WrappableListControl
            {
                SelectionMode = ListSelectionMode.None,
                Bounds = new UniRectangle(new UniScalar(0.3f, 0), new UniScalar(0.75f, 0), new UniScalar(0.675f, 0), new UniScalar(0.16f, 0))
            };

            var btnClearMessage = new ButtonControl
            {
                Text = "C",
                Bounds = new UniRectangle(new UniScalar(0.925f, 0), new UniScalar(0.93f, 0), new UniScalar(0.05f, 0), new UniScalar(0.05f, 0))
            };
            btnClearMessage.Pressed += ClearMessageList;

            #endregion

            screen.Desktop.Children.AddRange(
                new Control[] 
                {
					_topPanel,
					_rightPanel,
                    
                    _messageList,
                    _chatMessage,
                    btnClearMessage,
                });
        }        

        #endregion

        #region Event handlers

        private void LeaveGame_Pressed(object sender, EventArgs e)
        {
			PlayState.LeaveGame();
        }

        private void SendCommands_Pressed(object sender, EventArgs e)
        {
			PlayState.SendCommands();
        }

        private void DeleteCommand_Pressed(object sender, EventArgs e)
        {
			var commandList = sender as WrappableListControl;
			if (commandList.SelectedItems.Count > 0)
            {
				var selectedOrderIndex = commandList.SelectedItems[0];
				PlayState.DeleteCommand(selectedOrderIndex);
            }
        }

        private void ChatMessage_Execute(object sender, EventArgs e)
        {
            if (_chatMessage.Text.Trim().Length > 0)
            {
                PlayState.SendChatMessage(_chatMessage.Text);
                _chatMessage.Text = string.Empty;
            }
        }

		private void RaiseTech_Pressed(object sender, EventArgs e)
		{
			var senderName = (sender as Control).Name;			
			var techType = (TechnologyType) Enum.Parse(typeof(TechnologyType), senderName);
			PlayState.RaiseTechnology(techType);
		}

        #endregion

        #region Update requests

        public void UpdatePlayerList(List<Player> players)
        {
			_rightPanel.UpdatePlayerList(players);
        }

        public void UpdateResourceData(Player player)
        {
			_topPanel.UpdateResources(player);
        }

        public void UpdateTimer(int secondsLeft)
        {
			_topPanel.UpdateTimer(secondsLeft);
        }

        public void UpdateCommandList(List<UserCommand> commands, int selectedCommand = -1)
        {
			_rightPanel.UpdateCommands(commands, selectedCommand);
        }

        public void ClearMessageList(object sender, EventArgs args)
        {
            _messageList.Clear();
        }

        public void AddMessage(string message)
        {
            _messageList.AddItem(message);
        }

		public void EnableCommandButtons()
		{
			_rightPanel.EnableCommandButtons();
		}

        #endregion

		public PlayState PlayState { get; protected set; }

        public GameHud(PlayState state) : base(state)
        {
			PlayState = state;
            IsTransparent = true;            
            InputReceiver = new NuclexScreenInputReceiver(screen, false);            

            CreateChildControls();
			State = ViewState.Loaded;
        }
    }
}
