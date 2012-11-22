namespace Client.View.Play
{
	using System;
	using System.Collections.Generic;
	using Client.Common.AnimationSystem;
	using Client.Model;
	using Client.View.Controls;
	using Common;
	using Common.AnimationSystem.DefaultAnimations;
	using Input;
	using Nuclex.UserInterface;
	using Nuclex.UserInterface.Controls;
	using State;

    class GameHud : BaseView
	{
		public event EventHandler<EventArgs<TechnologyType>> RaiseTechPressed;
		public event EventHandler<EventArgs<string>> ChatMessageSent;
		public event EventHandler<EventArgs<int>> DeleteCommandPressed;
		public event EventHandler SendCommandsPressed;
		public event EventHandler LeaveGamePressed;

		#region BaseView members

		protected override void OnShow(double time)
		{
			_bottomPanel.Animate(this).MoveControlTo(_bottomPanel.DefaultPosition, 1.5).AddCallback(x => State = ViewState.Visible);
			_topPanel.Animate(this).MoveControlTo(TopPanel.DefaultPosition, 1.5);
			_rightPanel.Animate(this).MoveControlTo(_rightPanel.DefaultPosition, 1.5);
		}

		#endregion

		#region Protected members

		private TopPanel _topPanel;
		private RightPanel _rightPanel;
		private BottomPanel _bottomPanel;

        protected void CreateChildControls()
        {
			_topPanel = new TopPanel();
			_topPanel.TechRaised += RaiseTech_Pressed;
			_topPanel.LeftGame += LeaveGame_Pressed;

			_rightPanel = new RightPanel();
			_rightPanel.CommandDeleted += DeleteCommand_Pressed;
			_rightPanel.CommandsSent += SendCommands_Pressed;
			_rightPanel.Toggled += PanelToggle_Pressed;

			_bottomPanel = new BottomPanel();
			_bottomPanel.ChatMessageSent += ChatMessage_Execute;
			_bottomPanel.Toggled += PanelToggle_Pressed;

            screen.Desktop.Children.AddRange(
                new Control[] 
                {
					_topPanel,
					_rightPanel,
					_bottomPanel
                });
        }        

        #endregion

        #region Event handlers

		private void PanelToggle_Pressed(object sender, EventArgs e)
		{
			var panel = sender as TabbedPaneControl;
			UniVector destPos;
			if (panel.IsToggled)
			{
				destPos = panel.TogglePosition;
			}
			else
			{
				destPos = panel.DefaultPosition;
			}

			panel.Animate(this).MoveControlTo(destPos, 0.3, Interpolators.AccelerateDecelerate());
		}		

        private void LeaveGame_Pressed(object sender, EventArgs e)
        {
			if (LeaveGamePressed != null)
			{
				LeaveGamePressed(sender, EventArgs.Empty);
			}
        }

        private void SendCommands_Pressed(object sender, EventArgs e)
        {
			if (SendCommandsPressed != null)
			{
				SendCommandsPressed(sender, EventArgs.Empty);
			}
        }

        private void DeleteCommand_Pressed(object sender, EventArgs e)
        {
			var commandList = sender as WrappableListControl;
			if (commandList.SelectedItems.Count > 0)
            {
				var selectedOrderIndex = commandList.SelectedItems[0];
				if (DeleteCommandPressed != null)
				{
					DeleteCommandPressed(sender, DeleteCommandPressed.CreateArgs(selectedOrderIndex));
				}
            }
        }

        private void ChatMessage_Execute(object sender, EventArgs e)
        {
			var chatMessage = sender as CommandInputControl;
            if (chatMessage.Text.Trim().Length > 0)
            {
				if (ChatMessageSent != null)
				{
					ChatMessageSent(sender, ChatMessageSent.CreateArgs(chatMessage.Text));
				}
                
                chatMessage.Text = string.Empty;
            }
        }

		private void RaiseTech_Pressed(object sender, EventArgs e)
		{
			var senderName = (sender as Control).Name;			
			var techType = (TechnologyType) Enum.Parse(typeof(TechnologyType), senderName);

			if (RaiseTechPressed != null)
			{
				RaiseTechPressed(sender, RaiseTechPressed.CreateArgs(techType));				
			}
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

        public void AddMessage(string message)
        {
			_bottomPanel.AddMessage(message);
			BottomFlashAnimate();
        }

		private void BottomFlashAnimate()
		{
			if (_bottomPanel.IsToggled)
			{
				_bottomPanel.Animate(this).Wait(1.0f).AddCallback((ctrl) =>
				{
					_bottomPanel.ToggleChatButtonFlash();
					BottomFlashAnimate();
				});
			}
		}

		public void EnableCommandButtons()
		{
			_rightPanel.EnableCommandButtons();
		}

        #endregion

        public GameHud(PlayState state) : base(state)
        {		
            IsTransparent = true;            
            InputReceiver = new NuclexScreenInputReceiver(screen, false);            

            CreateChildControls();
			State = ViewState.Loaded;
        }
    }
}
