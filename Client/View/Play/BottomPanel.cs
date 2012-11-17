namespace Client.View.Play
{
	using System;
	using Client.Common;
	using Client.View.Controls;
	using Nuclex.UserInterface;
	using Nuclex.UserInterface.Controls;
	using Nuclex.UserInterface.Controls.Desktop;

	public class BottomPanel : TabbedPaneControl
	{
		public event EventHandler ChatMessageSent;

		public BottomPanel() 
			: base(new UniRectangle(new UniScalar(0.0f, 32), new UniScalar(0.7f, 0), new UniScalar(0.3f, 0), new UniScalar(0.3f, 0)))
		{
			var panel = new LabelControl
			{
				Bounds = new UniRectangle(new UniScalar(), new UniScalar(), new UniScalar(1.0f, 0), new UniScalar(1.0f, 0))
			};

			_messageList = new WrappableListControl
			{
				SelectionMode = ListSelectionMode.None,
				Bounds = new UniRectangle(new UniScalar(0.03f, 0), new UniScalar(0.05f, 0), new UniScalar(0.94f, 0), new UniScalar(0.775f, 0))
			};

			_chatMessage = new CommandInputControl
			{
				Bounds = new UniRectangle(new UniScalar(0.03f, 0), new UniScalar(0.825f, 0), new UniScalar(0.87f, 0), new UniScalar(0.125f, 0))
			};
			_chatMessage.OnCommandHandler += new EventHandler(ChatMessage_Execute);

			var btnClearMessage = new ButtonControl
			{
				Text = "C",
				Bounds = new UniRectangle(new UniScalar(0.9f, 0), new UniScalar(0.825f, 0), new UniScalar(0.075f, 0), new UniScalar(0.125f, 0))
			};
			btnClearMessage.Pressed += ClearMessageList;
			panel.Children.AddRange(new Control[] { _chatMessage, _messageList, btnClearMessage });

			AddTab("Chat", panel);
		}

		#region Update functions

		public void AddMessage(string message)
		{
			_messageList.AddItem(message);
		}

		#endregion

		#region Event handlers

		private void ClearMessageList(object sender, EventArgs e)
		{
			_messageList.Clear();
		}

		private void ChatMessage_Execute(object sender, EventArgs e)
		{
			if (ChatMessageSent != null)
			{
				ChatMessageSent(_chatMessage, e);
			}
		}

		#endregion

		private WrappableListControl _messageList;
		private CommandInputControl _chatMessage;
	}
}
