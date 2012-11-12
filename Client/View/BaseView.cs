namespace Client.View
{
	using Common;
	using Common.AnimationSystem.DefaultAnimations;
	using Input;
	using Nuclex.UserInterface;
	using State;

	public enum ViewState
	{
		Loading,
		Loaded,
		FadeIn,
		Visible,
		FadeOut,
		Hidden
	}

    public abstract class BaseView
	{
		#region Protected members

		protected internal ViewManager ViewMgr { get; protected set; }
		protected Screen screen;
		
		protected BaseView(GameState controller)
		{
			State = ViewState.Loading;
			GameState = controller;
			screen = new Screen(800, 600);
		}
		protected virtual void OnShow(double time)
		{
			screen.Desktop.Animate(this).SlideIn().AddCallback(x => State = ViewState.Visible);
		}
		protected virtual void OnHide(double time)
		{
			screen.Desktop.Animate(this).SlideOut().AddCallback(x => State = ViewState.Hidden);
		}

		#endregion

		public GameState GameState { get; protected set; }
		public ViewState State { get; protected set; }
        public bool IsTransparent { get; protected set; }
        public IInputReceiver InputReceiver { get; protected set; }

		public void Show(ViewManager viewMgr, double time)
		{
			ViewMgr = viewMgr;
			State = ViewState.FadeIn;

			OnShow(time);
		}
		public void Hide(double time)
		{
			State = ViewState.FadeOut;

			OnHide(time);
		}
        public virtual void Update(double delta, double time)
        {
            // No implementation required
        }
        public virtual void Draw(double delta, double time)
        {
            GameState.Client.Visualizer.Draw(screen);
        }
    }
}
