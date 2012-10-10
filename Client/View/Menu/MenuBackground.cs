namespace Client.View.Menu
{
    using System;
    using System.Linq;
    using Common;
    using Input;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using State;

    public class MenuBackground : BaseView
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
            var world = Matrix.CreateRotationX((float)time) * 
                        Matrix.CreateRotationY((float)(time + MathHelper.PiOver4)) * 
                        Matrix.CreateRotationZ((float)(time + MathHelper.PiOver4*3.0));
            var view = Matrix.CreateLookAt(Vector3.Backward * -2, Vector3.Zero, Vector3.Up);
            var projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(60.0f), 
                (float) graphicsDevice.Viewport.Width / graphicsDevice.Viewport.Height,
                0.1f, 1000.0f);

            _effect.Parameters["World"].SetValue(world);
            _effect.Parameters["View"].SetValue(view);
            _effect.Parameters["Projection"].SetValue(projection);
            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.SetVertexBuffer(_vb);
                graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, _vb.VertexCount/3);
            }
        }

        #endregion

        public MenuBackground(GameState state) : base(state)
        {
            IsTransparent = false;
            IsLoaded = false;
            var graphicsDevice = state.Client.GraphicsDevice;
            var contentMgr = state.Client.Content;
            InputReceiver = new InputReceiver(false);

            contentMgr.BeginLoad<Effect>("Effects\\Menu", OnEffectLoad, contentMgr);

            var vertices = new[] 
            {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(+0.5f, -0.5f, -0.5f),
                new Vector3(+0.5f, +0.5f, -0.5f),
                new Vector3(-0.5f, +0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, +0.5f),
                new Vector3(+0.5f, -0.5f, +0.5f),
                new Vector3(+0.5f, +0.5f, +0.5f),
                new Vector3(-0.5f, +0.5f, +0.5f),
            };

            var faces = new[] 
            {
                // bottom
                0, 1, 2,
                0, 2, 3,
                // up
                4, 6, 5,
                4, 7, 6,
                // left
                0, 7, 4,
                0, 3, 7,
                // right
                2, 5, 6,
                2, 1, 5,
                // front
                0, 5, 1,
                0, 4, 5,
                // back
                2, 7, 3,
                2, 6, 7
            };

            var colors = new[] 
            {
                Color.Red,
                Color.Violet,
                Color.Blue,
                Color.Yellow,
                Color.Magenta,
                Color.LightGreen
            };

            var data = faces.Select(index => new VertexPositionColor(vertices[index], colors[0])).ToArray();
            for (var i = 0; i < data.Length; ++i)
            {
                data[i].Color = colors[i / 6];
            }

            _vb = new VertexBuffer(graphicsDevice, typeof(VertexPositionColor), 3 * 2 * 6, BufferUsage.WriteOnly);
            _vb.SetData(data);
        }
    }
}
