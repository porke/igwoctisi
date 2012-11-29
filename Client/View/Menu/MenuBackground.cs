namespace Client.View.Menu
{
	using System.Linq;
	using Client.Common.AnimationSystem;
	using Client.Model;
	using Client.Renderer;
	using Common;
	using Input;
	using Microsoft.Xna.Framework;
	using Microsoft.Xna.Framework.Graphics;
	using State;
	using System;

    public class MenuBackground : BaseView
    {
        #region Protected members

		protected SpriteBatch _spriteBatch;
		protected Texture2D _texBackground;
		protected ICamera _camera;
		protected PlanetVisual _planetVisual;

        #endregion

        #region IView members

        public override void Draw(double delta, double time)
        {
			var device = GameState.Client.GraphicsDevice;

			_spriteBatch.Begin();
			_spriteBatch.Draw(_texBackground, new Rectangle(0, 0, _texBackground.Width, _texBackground.Height), Color.White);
			_spriteBatch.End();

			device.DepthStencilState = DepthStencilState.Default;
			_planetVisual.Draw(device, _camera, delta, time, 0, Color.White, false);
        }

        #endregion

        public MenuBackground(GameState state) : base(state)
        {
            IsTransparent = false;
            var device = state.Client.GraphicsDevice;
			var contentMgr = state.Client.Content;
			State = ViewState.Loaded;
            InputReceiver = new NullInputReceiver(false);

			_spriteBatch = new SpriteBatch(device);
			_texBackground = contentMgr.Load<Texture2D>("Textures\\MenuBackground");

			_camera = new SimpleCamera();			
			_camera.SetPosition(Vector3.Backward * 10);			

			var planet = new Planet
			{
				X = -2.5f,
				Y = 0,
				Z = 0,
				Radius = 3,
				Diffuse = "Textures\\Planets\\EarthDiffuse",
				Clouds = "Textures\\Planets\\EarthClouds",
				CloudsAlpha = "Textures\\Planets\\EarthCloudsAlpha"
			};
			_planetVisual = new PlanetVisual(state.Client, planet);

			var vertices = Utils.SphereVertices(5).Select(x => new Vertex(x.Position, x.Normal, Color.LightGreen, x.TextureCoordinate)).ToArray();
			_planetVisual.VB = new VertexBuffer(device, Vertex.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
			_planetVisual.VB.SetData(vertices);
        }
    }
}
