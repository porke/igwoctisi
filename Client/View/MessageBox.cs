namespace Client.View
{
    using System;
    using System.Linq;
    using Client.Input.Controls;
    using Common;
    using Input;
    using Nuclex.UserInterface;
    using Nuclex.UserInterface.Controls;
    using Nuclex.UserInterface.Controls.Desktop;
	using Client.State;

    enum MessageBoxButtons
    {
        None,
        OK
    }

    class MessageBox : BaseView
    {
        #region Protected members

        protected MessageBoxButtons _buttons;
        protected WindowControl _window;
        protected WrappableLabelControl _lblMessage;

        protected void CreateChildControls()
        {
            _window = new WindowControl
            {
                Bounds = new UniRectangle(new UniScalar(0.25f, 0), new UniScalar(0.35f, 0), new UniScalar(0.5f, 0), new UniScalar(0.25f, 0))
            };

            _lblMessage = new WrappableLabelControl
            {
                Bounds = new UniRectangle(new UniScalar(0.05f, 0), new UniScalar(0.05f, 0), new UniScalar(0.9f, 0), new UniScalar(0.6f, 0))
            };

            _window.Children.AddRange(new[] { _lblMessage });
            screen.Desktop.Children.AddRange(new[] { _window });
        }
        protected void UpdateButtons(MessageBoxButtons buttons)
        {
            _buttons = buttons;

            // remove all buttons first
            var toRemove = _window.Children.OfType<ButtonControl>().ToList();
            toRemove.ForEach(x => _window.Children.Remove(x));

            // add only necessary buttons
            switch (buttons)
            {
                case MessageBoxButtons.None:
                    break;

                case MessageBoxButtons.OK:
                    var okButton = new ButtonControl
                    {
                        Text = "OK",
                        Bounds = new UniRectangle(new UniScalar(0.55f, 0), new UniScalar(0.70f, 0), new UniScalar(0.4f, 0), new UniScalar(0.2f, 0))
                    };
                    okButton.Pressed += RaiseOkPressed;

                    _window.Children.AddRange(new[] { okButton });
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

		#region BaseView members

		protected override void OnShow(double time)
		{
			State = ViewState.Visible;
		}
		protected override void OnHide(double time)
		{
			State = ViewState.Hidden;
		}

		#endregion

		public string Title
        {
            get { return _window.Title; }
            set { _window.Title = value; }
        }
        public string Message
        {
            get { return _lblMessage.Text; }
            set { _lblMessage.Text = value; }
        }
        public MessageBoxButtons Buttons
        {
            get { return _buttons; }
            set { UpdateButtons(value); }
        }

        public event EventHandler OkPressed;

        public MessageBox(GameState gameState, MessageBoxButtons buttons) : base(gameState)
        {
            screen = new Screen(800, 600);
            InputReceiver = new NuclexScreenInputReceiver(screen, true);

            CreateChildControls();
            Buttons = buttons;
            IsTransparent = true;
			State = ViewState.Loaded;
        }
        public void RaiseOkPressed(object sender, EventArgs e)
        {
            if (OkPressed != null)
                OkPressed(sender, e);
        }
    }
}
