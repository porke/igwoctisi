namespace Client.View.Play
{
	using Client.View.Controls;
	using Nuclex.UserInterface;

	class NotificationPanel : IconControl
	{
		/// <summary>
		/// In seconds.
		/// </summary>
		public double Timeout { get; set; }
		public readonly UniVector DefaultPosition = new UniVector(new UniScalar(0.6f, 0), new UniScalar(1.0f, -32));
		public readonly UniVector TogglePosition = new UniVector(new UniScalar(0.6f, 0), new UniScalar(1.0f, 0));

		public NotificationPanel() : base("hud_background")
		{
			Timeout = DefaultTimeout;
			Bounds = new UniRectangle(TogglePosition, Size);
			_notification = new WrappableLabelControl()
			{
				Bounds = new UniRectangle(new UniScalar(), new UniScalar(), new UniScalar(1.0f, 0), new UniScalar(1.0f, 0))
			};

			Children.Add(_notification);
		}

		public void SetNotification(string text)
		{
			_notification.Text = text;
		}
		
		private WrappableLabelControl _notification;

		private const double DefaultTimeout = 3.0;
		private static readonly UniVector Size = new UniVector(new UniScalar(0.4f, 0), new UniScalar(32));
	}
}
