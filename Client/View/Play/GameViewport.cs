namespace Client.View.Play
{
    using Input;
    using Model;

    class GameViewport : BaseView
    {
        #region IView members

        public override void Draw(double delta, double time)
        {
            var renderer = state.Client.Renderer;

            renderer.Draw(scene, delta, time);
        }

        #endregion

        private Scene scene;

        public GameViewport(State.GameState state, Scene scene) : base(state)
        {
            IsLoaded = true;
            IsTransparent = false;
            InputReceiver = new InputReceiver(false);
            this.scene = scene;
        }
    }
}
