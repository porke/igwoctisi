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
	using System.Collections.Generic;

    public class MenuBackground : BaseView
    {
        #region Protected members

		protected SpriteBatch _spriteBatch;
		protected Texture2D _texBackground;
		protected ICamera _camera;
		protected PlanetVisual _planetVisual;
		protected List<Tuple<Spaceship, float, float, float>> _spaceships;

        #endregion

        #region IView members

		protected override void OnShow(double time)
		{
			base.OnShow(time);
			
			Spaceship.SetupModelPools(GameState.Client.Content, ViewMgr.AnimationManager);
			var random = new Random();

			var spaceshipsCount = random.Next() % 4 + 6;
			_spaceships = new List<Tuple<Spaceship, float, float, float>>(spaceshipsCount);
			var color = new PlayerColor(0, (int)Color.Blue.PackedValue);
			for (var i = 0; i < spaceshipsCount; ++i)
			{
				var ship = Spaceship.Acquire(SpaceshipModelType.LittleSpaceship, color);
				var shift = (float)random.NextDouble() * MathHelper.TwoPi;
				var zRot = (float)(random.NextDouble() - 0.5f)*2.0f;
				var velocity = (float)(random.NextDouble() - 0.5f) * 3.0f;
				if (Math.Abs(velocity) < 0.5f)
				{
					velocity = velocity >= 0 ? 0.5f : -0.5f;
				}

				ship.SetScale(0.01f);

				var tuple = new Tuple<Spaceship, float, float, float>(ship, shift, zRot, velocity);
				_spaceships.Add(tuple);
			}
		}
		public override void Update(double delta, double time)
		{
			base.Update(delta, time);

			foreach (var tuple in _spaceships)
			{
				var ship = tuple.Item1;
				var shift = tuple.Item2;
				var zRot = tuple.Item3;
				var velocity = tuple.Item4;

				var rotation =
					Matrix.CreateRotationY((float)-time*velocity + shift) *
					Matrix.CreateRotationZ(zRot);
				var transform =
					Matrix.CreateTranslation(3.5f, 0, 0) *
					rotation *
					Matrix.CreateTranslation(_planetVisual.GetPosition());

				var position = transform.Translation;
				var orientation =
					((velocity > 0) ? Matrix.Identity : Matrix.CreateRotationY(MathHelper.Pi)) *
					Matrix.CreateRotationZ(-MathHelper.PiOver2) * 
					rotation;

				ship.SetPosition(position);
				ship.Rotation = orientation;
			}
		}
        public override void Draw(double delta, double time)
        {
			var device = GameState.Client.GraphicsDevice;

			_spriteBatch.Begin();
			_spriteBatch.Draw(_texBackground, new Rectangle(0, 0, _texBackground.Width, _texBackground.Height), Color.White);
			_spriteBatch.End();

			device.DepthStencilState = DepthStencilState.Default;
			_planetVisual.Draw(device, _camera, delta, time, 0, Color.White, false);

			foreach (var tuple in _spaceships)
			{
				var ship = tuple.Item1;
				ship.Draw(_camera, delta, time);
			}
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
