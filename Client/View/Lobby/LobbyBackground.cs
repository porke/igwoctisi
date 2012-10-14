namespace Client.View.Lobby
{
    using System;
    using System.Linq;
    using Common;
    using Input;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using State;

    public class LobbyBackground : BaseView
    {
        #region Protected members

        protected Effect _effect;
        protected VertexBuffer _vb;

        protected void OnEffectLoad(IAsyncResult ar)
        {
            var contentMgr = (ContentManager)ar.AsyncState;
            _effect = contentMgr.EndLoad<Effect>(ar);

            IsLoaded = true;
        }

        #endregion

        #region IView members

        public override void Draw(double delta, double time)
        {
            if (!IsLoaded)
                return;

            var graphicsDevice = state.Client.GraphicsDevice;
            var world = Matrix.CreateRotationZ((float)(time + MathHelper.PiOver4));
            var view = Matrix.CreateLookAt(Vector3.Backward * -2, Vector3.Zero, Vector3.Up);
            var projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(60.0f),
                (float)graphicsDevice.Viewport.Width / graphicsDevice.Viewport.Height,
                0.1f, 1000.0f);

            _effect.Parameters["World"].SetValue(world);
            _effect.Parameters["View"].SetValue(view);
            _effect.Parameters["Projection"].SetValue(projection);
            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.SetVertexBuffer(_vb);
                graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, _vb.VertexCount / 3);
            }
        }

        #endregion

        public LobbyBackground(GameState state): base(state)
        {
            IsLoaded = false;
            IsTransparent = false;
            var graphicsDevice = state.Client.GraphicsDevice;
            var contentMgr = state.Client.Content;
            InputReceiver = new NullInputReceiver(false);

            contentMgr.BeginLoad<Effect>("Effects\\Menu", OnEffectLoad, contentMgr);

            var vertices = new[] 
            {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(+0.5f, -0.5f, -0.5f),
                new Vector3(+0.5f, +0.5f, -0.5f),
            };

            var faces = new[] 
            {
                0, 1, 2
            };

            var colors = new[] 
            {
                Color.Magenta,
            };

            var data = faces.Select(index => new VertexPositionColor(vertices[index], colors[0])).ToArray();
            for (var i = 0; i < data.Length; ++i)
            {
                data[i].Color = colors[i / 6];
            }

            _vb = new VertexBuffer(graphicsDevice, typeof(VertexPositionColor), 3, BufferUsage.WriteOnly);
            _vb.SetData(data);
        }
    }
}
