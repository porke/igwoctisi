namespace Client.View.Play
{
    using Input;
    using State;

    class GameViewport : IView
    {
        #region IView members

        public bool IsLoaded { get; protected set; }
        public bool IsTransparent
        {
            get { return false; }
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
            var renderer = State.Client.Renderer;
            var scene = State.Scene;

            renderer.Draw(scene, delta, time);
        }

        #endregion

        public PlayState State { get; protected set; }
        public ViewManager ViewMgr { get; protected set; }

        public GameViewport(PlayState state)
        {
            IsLoaded = true;
            State = state;
            InputReceiver = new InputReceiver(false);
        }
    }
}
