namespace Client.View
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Common;
    using Input;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using State;

    public class SplashScreen : IView
    {
        #region Protected members

        protected Texture2D _texture;
        protected SpriteBatch _spriteBatch;
        protected double _elapsedTime;

        protected void OnTextureLoad(IAsyncResult ar)
        {
            var contentMgr = (ContentManager) ar.AsyncState;
            _texture = contentMgr.EndLoad<Texture2D>(ar);

            IsLoaded = true;
        }

        #endregion

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
            _elapsedTime = 0;
        }

        public void OnHide(double time)
        {

        }

        public void Update(double delta, double time)
        {
            _elapsedTime += delta;

            if (_elapsedTime > SwitchTime && NextLayers != null && NextLayers.Count > 0)
            {
                // wait until each layer are loaded
                foreach (var layer in NextLayers)
                {
                    if (!layer.IsLoaded) break;
                }

                ViewMgr.PopLayer();

                foreach (var layer in NextLayers)
                {
                    ViewMgr.PushLayer(layer);
                }
            }
        }
        public void Draw(double delta, double time)
        {
            if (!IsLoaded)
                return;

            var graphicsDevice = State.Client.GraphicsDevice;
            var viewport = graphicsDevice.Viewport.Bounds;

            var destPos = new Vector2((viewport.Width - _texture.Width)/2.0f, (viewport.Height - _texture.Height)/2.0f);
            _spriteBatch.Begin();
            _spriteBatch.Draw(_texture, destPos, Color.White);
            _spriteBatch.End();
        }

        #endregion

        public GameState State { get; protected set; }
        public ViewManager ViewMgr { get; protected set; }
        public double SwitchTime { get; protected set; }
        public IList<IView> NextLayers { get; set; }

        public SplashScreen(GameState state, string textureName, double switchTime)
        {
            IsLoaded = false;
            State = state;
            var graphicsDevice = State.Client.GraphicsDevice;
            var contentMgr = State.Client.Content;
            InputReceiver = new InputReceiver(false);
            SwitchTime = switchTime;
            Thread.MemoryBarrier();

            contentMgr.BeginLoad<Texture2D>(textureName, OnTextureLoad, contentMgr);
            _spriteBatch = new SpriteBatch(graphicsDevice);
        }
    }
}
