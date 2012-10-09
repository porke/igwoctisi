namespace Client.View
{
    using System;
    using System.Linq;
    using Common;
    using Input;
    using Nuclex.UserInterface;
    using Nuclex.UserInterface.Controls;
    using Nuclex.UserInterface.Controls.Desktop;
    using IInputReceiver = Input.IInputReceiver;

    enum MessageBoxButtons
    {
        None,
        OK
    }

    class MessageBox : IView
    {
        #region Protected members

        protected Screen _screen;
        protected MessageBoxButtons _buttons;
        protected WindowControl _window;
        protected LabelControl _lblMessage;

        protected void CreateChildControls()
        {
            _window = new WindowControl
            {
                Title = "",
                Bounds = new UniRectangle(new UniScalar(0.25f, 0), new UniScalar(0.35f, 0), new UniScalar(0.5f, 0), new UniScalar(0.25f, 0))
            };

            _lblMessage = new LabelControl
            {
                Text = "",
                Bounds = new UniRectangle(new UniScalar(0.05f, 0), new UniScalar(0.05f, 0), new UniScalar(0.9f, 0), new UniScalar(0.6f, 0))
            };

            _window.Children.AddRange(new[] { _lblMessage });
            _screen.Desktop.Children.AddRange(new[] { _window });
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

        #region IView members

        public bool IsLoaded
        {
            get { return true; }
        }
        public bool IsTransparent
        {
            get { return true; }
        }
        public IInputReceiver InputReceiver { get; protected set; }

        public void OnShow(ViewManager viewMgr, double time)
        {
            ViewMgr = viewMgr;
        }
        public void OnHide(double time)
        {
        }
        public void Update(double delta, double time)
        {
        }
        public void Draw(double delta, double time)
        {
            ViewMgr.Client.Visualizer.Draw(_screen);
        }

        #endregion

        public ViewManager ViewMgr { get; protected set; }
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

        public MessageBox(MessageBoxButtons buttons)
        {
            _screen = new Screen(800, 600);
            InputReceiver = new NuclexScreenInputReceiver(_screen, true);

            CreateChildControls();
            Buttons = buttons;
        }
        public void RaiseOkPressed(object sender, EventArgs e)
        {
            if (OkPressed != null)
                OkPressed(sender, e);
        }
    }
}
