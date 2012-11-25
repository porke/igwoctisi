namespace Client.View.Lobby
{
	using System;
	using System.Collections.Generic;
	using Client.Model;
	using Client.View.Controls;
	using Common;
	using Common.AnimationSystem.DefaultAnimations;
	using Input;
	using Nuclex.UserInterface;
	using Nuclex.UserInterface.Controls;
	using Nuclex.UserInterface.Controls.Desktop;
	using State;

	class GameLobbyView : BaseView
    {
		public event EventHandler GameLeavePressed;
		public event EventHandler<EventArgs<string>> PlayerKicked;
		public event EventHandler BeginGamePressed;
		public event EventHandler<EventArgs<string>> ChatMessageSent;

        #region Protected members

        private WrappableListControl _messageList;
        private ListControl _playerList;
        private CommandInputControl _currentMessage;

        protected void CreateChildControls(bool showHostButtons)
        {
            var btnBeginGame = new ButtonControl()
            {
                Text = "Begin Game",
                Bounds = new UniRectangle(new UniScalar(0.7f, 0), new UniScalar(0.05f, 0), new UniScalar(0.25f, 0), new UniScalar(0.1f, 0))
            };
            btnBeginGame.Pressed += BeginGame_Pressed;

            var btnLeaveGame = new ButtonControl()
            {
                Text = "Leave Game",
                Bounds = new UniRectangle(new UniScalar(0.7f, 0), new UniScalar(0.35f, 0), new UniScalar(0.25f, 0), new UniScalar(0.1f, 0))
            };
            btnLeaveGame.Pressed += LeaveGame_Pressed;

            var btnKickPlayer = new ButtonControl()
            {
                Text = "Kick player",
                Bounds = new UniRectangle(new UniScalar(0.7f, 0), new UniScalar(0.2f, 0), new UniScalar(0.25f, 0), new UniScalar(0.1f, 0))
            };
            btnKickPlayer.Pressed += KickPlayer_Pressed;

            var btnSendChatMessage = new ButtonControl()
            {
                Text = "Send",
                Bounds = new UniRectangle(new UniScalar(0.85f, 0), new UniScalar(0.925f, 0), new UniScalar(0.1f, 0), new UniScalar(0.1f, 0))
            };
            btnSendChatMessage.Pressed += SendChatMessage_Pressed;

            _messageList = new WrappableListControl()
            {
                Bounds = new UniRectangle(new UniScalar(0.05f, 0), new UniScalar(0.5f, 0), new UniScalar(0.9f, 0), new UniScalar(0.4f, 0))
            };

            _currentMessage = new CommandInputControl()
            {
                Text = "",
                Bounds = new UniRectangle(new UniScalar(0.05f, 0), new UniScalar(0.925f, 0), new UniScalar(0.8f, 0), new UniScalar(0.1f, 0))
            };
            _currentMessage.OnCommandHandler += SendChatMessage_Pressed;

            _playerList = new ListControl()
            {
                SelectionMode = ListSelectionMode.Single,
                Bounds = new UniRectangle(new UniScalar(0.05f, 0), new UniScalar(0.05f, 0), new UniScalar(0.6f, 0), new UniScalar(0.4f, 0))
            };

            screen.Desktop.Children.AddRange(
                new Control[] 
                {
                    btnLeaveGame, btnSendChatMessage, _messageList, _currentMessage, _playerList
                });

            if (showHostButtons)
            {
                screen.Desktop.Children.AddRange(new ButtonControl[] {btnBeginGame, btnKickPlayer});
            }
        }

        #endregion        

        #region Event handlers

        private void BeginGame_Pressed(object sender, EventArgs e)
        {
			if (BeginGamePressed != null)
			{
				BeginGamePressed(this, EventArgs.Empty);
			}
        }
        private void LeaveGame_Pressed(object sender, EventArgs e)
        {
			if (GameLeavePressed != null)
			{
				GameLeavePressed(this, EventArgs.Empty);
			}
        }
        private void KickPlayer_Pressed(object sender, EventArgs e)
        {
            if (_playerList.SelectedItems.Count == 1)
            {
                string username = _playerList.Items[_playerList.SelectedItems[0]];

				if (PlayerKicked != null)
				{
					PlayerKicked(this, PlayerKicked.CreateArgs(username));
				}
            }
        }
        private void SendChatMessage_Pressed(object sender, EventArgs e)
        {
            if (_currentMessage.Text.Trim().Length > 0)
            {
				if (ChatMessageSent != null)
				{
					ChatMessageSent(this, ChatMessageSent.CreateArgs(_currentMessage.Text));
				}
                _currentMessage.Text = "";
            }
        }

        #endregion

        #region Update functions

        public void RefreshPlayerList(List<string> newPlayerList, string hostName, string clientName)
        {
            var usernameList = _playerList.Items;

            usernameList.Clear();
            usernameList.AddRange(newPlayerList);

            int hostIndex = newPlayerList.IndexOf(hostName);
            int clientIndex = newPlayerList.IndexOf(clientName);

            if (hostIndex != -1)
            {
                usernameList[hostIndex] = "[Host] " + usernameList[hostIndex];
            }

            usernameList[clientIndex] = "[+] " + usernameList[clientIndex];
        }

        public void ChatMessageReceived(ChatMessage message)
        {
			_messageList.AddItem(string.Format("[{0}] {1}: {2}", message.Time, message.Username, message.Message));
        }

        public void AddHostMessage(string message, string time)
        {
            _messageList.AddItem(string.Format("({0}): {1}", time, message));
        }

		public void KickReceived()
		{
			_leaveByKick = true;
		}

        #endregion

		#region BaseView members

		protected override void OnHide(double time)
		{
			State = ViewState.FadeOut;

			if (_leaveByKick)
			{
				screen.Desktop.Animate(this)
							  .MoveControlTo(new UniVector(new UniScalar(-screen.Desktop.Bounds.Right .Fraction, 0.0f), screen.Desktop.Bounds.Top))
							  .AddCallback(x => State = ViewState.Hidden);
			}
			else
			{
				screen.Desktop.Animate(this)
							  .MoveControlTo(new UniVector(new UniScalar(1.0f, 0.0f), screen.Desktop.Bounds.Top))
							  .AddCallback(x => State = ViewState.Hidden);
			}
		}

		#endregion

		public GameLobbyView(LobbyState state, bool showHostButtons)
            : base(state)
        {
            IsTransparent = true;
            screen.Desktop.Bounds = new UniRectangle(new UniScalar(0.2f, 0), new UniScalar(0.25f, 0), new UniScalar(0.6f, 0), new UniScalar(0.5f, 0));
            InputReceiver = new NuclexScreenInputReceiver(screen, false);

            CreateChildControls(showHostButtons);
			State = ViewState.Loaded;
        }

		private bool _leaveByKick = false;
    }
}
