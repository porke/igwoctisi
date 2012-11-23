namespace Client.Renderer
{
	using System.Collections.Generic;
	using System.Linq;
	using Client.Common;
	using Microsoft.Xna.Framework;
	using Microsoft.Xna.Framework.Graphics;
	using Model;
	using Microsoft.Xna.Framework.Input;

	public class XnaRenderer : IRenderer
	{
		#region Protected members

		protected DepthStencilState _dssDefault, _dssGlow;
		protected SpriteBatch _spriteBatch;
		protected Effect _fxBlur;
		protected RenderTarget2D _rtGlow, _rtBlur;
		protected VertexBuffer _vbQuad;
		private Dictionary<int, bool> _planetsDetailsShowing = new Dictionary<int,bool>();

		protected void RenderGlow(ICamera camera, Scene scene, double delta, double time)
		{
			Viewport viewport;
			var world = Matrix.Identity;
			var view = Matrix.CreateLookAt(Vector3.Backward, Vector3.Zero, Vector3.Up);
			var projection = Matrix.CreateOrthographicOffCenter(0, 1, 1, 0, 0.1f, 100.0f);

			// render indicators
			GraphicsDevice.SetRenderTarget(_rtGlow);
			viewport = GraphicsDevice.Viewport;
			GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1, 0);
			scene.Visual.DrawGlow(GraphicsDevice, camera, delta, time);

			// render horizontal blur
			GraphicsDevice.SetRenderTarget(_rtBlur);
			viewport = GraphicsDevice.Viewport;
			//GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Transparent, 1, 0);
			_fxBlur.Parameters["World"].SetValue(world);
			_fxBlur.Parameters["View"].SetValue(view);
			_fxBlur.Parameters["Projection"].SetValue(projection);
			_fxBlur.Parameters["Diffuse"].SetValue(_rtGlow);
			_fxBlur.Parameters["BlurRange"].SetValue(new Vector2(0.002f, 0.00f));
			_fxBlur.Parameters["Resolution"].SetValue(new Vector2(viewport.Width, viewport.Height));
			foreach (var pass in _fxBlur.CurrentTechnique.Passes)
			{
				pass.Apply();
				GraphicsDevice.SetVertexBuffer(_vbQuad);
				GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, _vbQuad.VertexCount - 2);
			}

			// render vertical blur
			GraphicsDevice.SetRenderTarget(_rtGlow);
			viewport = GraphicsDevice.Viewport;
			//GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Transparent, 1, 0);
			_fxBlur.Parameters["World"].SetValue(world);
			_fxBlur.Parameters["View"].SetValue(view);
			_fxBlur.Parameters["Projection"].SetValue(projection);
			_fxBlur.Parameters["Diffuse"].SetValue(_rtBlur);
			_fxBlur.Parameters["BlurRange"].SetValue(new Vector2(0.00f, 0.002f));
			_fxBlur.Parameters["Resolution"].SetValue(new Vector2(viewport.Width, viewport.Height));
			foreach (var pass in _fxBlur.CurrentTechnique.Passes)
			{
				pass.Apply();
				GraphicsDevice.SetVertexBuffer(_vbQuad);
				GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, _vbQuad.VertexCount - 2);
			}

			// restore render target to back buffer
			GraphicsDevice.SetRenderTarget(null);
		}

		#endregion

		#region IRenderer members

		public bool RaySphereIntersection(ICamera camera, Vector2 screenPosition, Vector3 position, float radius)
		{
			var ray = camera.GetRay(GraphicsDevice.Viewport, new Vector3(screenPosition, 0));
			return ray.Intersects(new BoundingSphere(position, radius)) != null;
		}
		public bool RayLinkIntersection(ICamera camera, Vector2 screenPosition, Planet sourcePlanet, Planet targetPlanet)
		{
			var ray = camera.GetRay(GraphicsDevice.Viewport, new Vector3(screenPosition, 0));
			var indicator = sourcePlanet.Visual.GetIndicator(targetPlanet.Id);
			return indicator.IsIntersected(ray);
		}

		public void Initialize(GameClient client)
		{
			Client = client;
			GraphicsDeviceService = (IGraphicsDeviceService)Client.Services.GetService(typeof(IGraphicsDeviceService));
			GraphicsDevice = GraphicsDeviceService.GraphicsDevice;

			_dssDefault = new DepthStencilState
			{
				DepthBufferEnable = true,
				DepthBufferWriteEnable = true,
				StencilEnable = true,
				StencilMask = 0xFF,
				StencilWriteMask = 0xFF,
				StencilFail = StencilOperation.Keep,
				StencilDepthBufferFail = StencilOperation.Keep,
				StencilPass = StencilOperation.IncrementSaturation,
				StencilFunction = CompareFunction.Always
			};
			_dssGlow = new DepthStencilState
			{
				DepthBufferEnable = false,
				DepthBufferWriteEnable = false,
				StencilEnable = true,
				StencilMask = 0xFF,
				StencilWriteMask = 0xFF,
				StencilFail = StencilOperation.IncrementSaturation,
				StencilDepthBufferFail = StencilOperation.Keep,
				StencilPass = StencilOperation.Keep,
				StencilFunction = CompareFunction.Equal
			};

			var contentMgr = Client.Content;
			_spriteBatch = new SpriteBatch(GraphicsDevice);

			_fxBlur = contentMgr.Load<Effect>("Effects\\Blur");

			_rtGlow = new RenderTarget2D(GraphicsDevice, 512, 512, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
			_rtBlur = new RenderTarget2D(GraphicsDevice, 512, 512, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

			var quadVertices = new[] {
				new VertexPositionTexture(new Vector3(1, 0, 0), new Vector2(1, 0)),
				new VertexPositionTexture(new Vector3(1, 1, 0), new Vector2(1, 1)),
				new VertexPositionTexture(new Vector3(0, 0, 0), new Vector2(0, 0)),
				new VertexPositionTexture(new Vector3(0, 1, 0), new Vector2(0, 1)),
			};
			_vbQuad = new VertexBuffer(GraphicsDevice, VertexPositionTexture.VertexDeclaration, quadVertices.Length, BufferUsage.WriteOnly);
			_vbQuad.SetData(quadVertices);
		}
		public void Release()
		{
			Client = null;
		}
		public void Update(Scene scene, double delta, double time)
		{
			scene.Visual.Update(delta, time);
		}
		public void Draw(ICamera camera, Scene scene, double delta, double time)
		{
			GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil, Color.Black, 1, 0);
			var viewport = GraphicsDevice.Viewport;

			RenderGlow(camera, scene, delta, time);

			if (Keyboard.GetState().IsKeyDown(Keys.LeftAlt))
			{
				var tmp = GraphicsDevice.Viewport;
				_spriteBatch.Begin();
				_spriteBatch.Draw(Keyboard.GetState().IsKeyDown(Keys.LeftControl) ? _rtBlur : _rtGlow, tmp.Bounds, Color.White);
				_spriteBatch.End();
				return;
			}

			var map = scene.Map;

            // Turn depth and stencil buffers on (SpriteBatch may turn it off).
			GraphicsDevice.DepthStencilState = _dssDefault;

			scene.Visual.DrawBackground(GraphicsDevice, camera, delta, time);

			// indicators
			scene.Visual.DrawIndicators(GraphicsDevice, camera, delta, time);

			// planets
			GraphicsDevice.DepthStencilState = _dssDefault;
			foreach (var planet in map.Planets)
			{
				var planetarySystem = scene.Map.GetSystemByPlanetid(planet.Id);

				var ambient = scene.SelectedPlanet == planet.Id || scene.HoveredPlanet == planet.Id ? HoverAmbient : 0.0f;
				var glow = planetarySystem != null && planet.Owner != null ? planet.Owner.Color.XnaColor : Color.LightGray;

				bool grayPlanet = planet.Owner != scene.ClientPlayer
					&& planet.NeighbourPlanets.All(p => p.Owner != scene.ClientPlayer);
				planet.Visual.Draw(GraphicsDevice, camera, delta, time, ambient, glow, grayPlanet);

				// cache that information
				_planetsDetailsShowing[planet.Id] = !grayPlanet;
			}

			// spacesheeps
			GraphicsDevice.DepthStencilState = _dssDefault;
			scene.Visual.Draw(GraphicsDevice, camera, delta, time);

			// glow
			_spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Additive, null, _dssGlow, null);
			_spriteBatch.Draw(_rtGlow, viewport.Bounds, Color.White);
			_spriteBatch.End();

			GraphicsDevice.DepthStencilState = _dssDefault;

            // planets info
            _spriteBatch.Begin();
			foreach (var planet in scene.Map.Planets)
			{
				if (_planetsDetailsShowing[planet.Id])
				{
					planet.Visual.DrawInfo(GraphicsDevice, _spriteBatch, camera, scene.HoveredPlanet == planet.Id);
				}
			}
			_spriteBatch.End();
		}

		#endregion

		public const float HoverAmbient = 1.0f;

		public GameClient Client { get; protected set; }
		public GraphicsDevice GraphicsDevice { get; protected set; }
		public IGraphicsDeviceService GraphicsDeviceService { get; protected set; }
	}
}
