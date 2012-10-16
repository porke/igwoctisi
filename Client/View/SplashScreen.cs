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

    public class SplashScreen : BaseView
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

        public override void OnShow(ViewManager viewMgr, double time)
        {
            ViewMgr = viewMgr;
            _elapsedTime = 0;
        }

        public override void Update(double delta, double time)
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
        public override void Draw(double delta, double time)
        {
            if (!IsLoaded)
                return;

            var graphicsDevice = state.Client.GraphicsDevice;
            var viewport = graphicsDevice.Viewport.Bounds;

            var destPos = new Vector2((viewport.Width - _texture.Width)/2.0f, (viewport.Height - _texture.Height)/2.0f);
            _spriteBatch.Begin();
            _spriteBatch.Draw(_texture, destPos, Color.White);
            _spriteBatch.End();
        }

        #endregion

        public double SwitchTime { get; protected set; }
        public IList<BaseView> NextLayers { get; set; }

        public SplashScreen(GameState state, string textureName, double switchTime) : base(state)
        {
            IsTransparent = false;
            IsLoaded = false;
            var graphicsDevice = state.Client.GraphicsDevice;
            var contentMgr = state.Client.Content;
            InputReceiver = new InputReceiver(false);
            SwitchTime = switchTime;
            Thread.MemoryBarrier();

            contentMgr.BeginLoad<Texture2D>(textureName, OnTextureLoad, contentMgr);
            _spriteBatch = new SpriteBatch(graphicsDevice);
        }
    }
}
