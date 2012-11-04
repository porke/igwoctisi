namespace Client.View
{
    using Input;
    using Nuclex.UserInterface;
    using State;

    public abstract class BaseView
    {
        public bool IsLoaded { get; protected set; }
		public bool IsHidden { get; protected set; }
        public bool IsTransparent { get; protected set; }
        public IInputReceiver InputReceiver { get; protected set; }

        protected internal ViewManager ViewMgr { get; protected set; }
        protected Screen screen;
        protected GameState state;

        protected BaseView(GameState controller)
        {
            state = controller;
            screen = new Screen(800, 600);
        }

        public virtual void OnShow(ViewManager viewMgr, double time)
        {
            this.ViewMgr = viewMgr;
			IsHidden = false;
        }

        public virtual void OnHide(double time)
        {
			IsHidden = true;
        }

        public virtual void Update(double delta, double time)
        {
            // No implementation required
        }

        public virtual void Draw(double delta, double time)
        {
            ViewMgr.Client.Visualizer.Draw(screen);
        }
    }
}
