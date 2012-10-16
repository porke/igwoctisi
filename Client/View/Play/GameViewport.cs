namespace Client.View.Play
{
    using Input;
    using Model;
    using Client.State;

    class GameViewport : BaseView
    {
        #region IView members

        public override void Draw(double delta, double time)
        {
            var renderer = state.Client.Renderer;

            renderer.Draw(State.Scene, delta, time);
        }

        #endregion

        public PlayState State { get; protected set; }

        public GameViewport(PlayState state) : base(state)
        {
            IsLoaded = true;
            IsTransparent = false;
            InputReceiver = new InputReceiver(false);

            State = state;
        }
    }
}
