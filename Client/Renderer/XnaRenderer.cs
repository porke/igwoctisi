namespace Client.Renderer
{
    using System;
    using System.Linq;
    using Client.Common;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Model;

    public class XnaRenderer : IRenderer
    {
        #region Protected members

        protected Effect _fxLinks, _fxPlanet;
        protected VertexBuffer _sphereVB;

        private SimpleCamera _camera = new SimpleCamera();

        protected void InitializeMapVisual(Map map)
        {
            var vertices = new VertexPositionColor[map.Links.Count * 2];
            var color = Color.LightGreen;

            for (var i = 0; i < map.Links.Count; ++i)
            {
                var link = map.Links[i];
                var sourcePlanet = map.Planets.First(x => x.Id == link.SourcePlanet);
                var targetPlanet = map.Planets.First(x => x.Id == link.TargetPlanet);

                vertices[2 * i + 0] = new VertexPositionColor(new Vector3(sourcePlanet.X, sourcePlanet.Y, sourcePlanet.Z), color);
                vertices[2 * i + 1] = new VertexPositionColor(new Vector3(targetPlanet.X, targetPlanet.Y, targetPlanet.Z), color);
            }

            var visual = new MapVisual();
            visual.LinksVB = new VertexBuffer(GraphicsDevice, VertexPositionColor.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
            visual.LinksVB.SetData(vertices);

            map.Visual = visual;
        }
        protected void InitializePlanetVisual(Planet planet)
        {
            var contentMgr = Client.Content;
            var visual = new PlanetVisual();
            var random = new Random(Guid.NewGuid().GetHashCode());

            visual.Period = (float)(random.NextDouble() * 10.0 + 5.0);
            visual.Yaw = (float)(random.NextDouble() * MathHelper.TwoPi);
            visual.Pitch = (float)(random.NextDouble() * MathHelper.TwoPi);
            visual.Roll = (float)(random.NextDouble() * MathHelper.TwoPi);
            
            if (!string.IsNullOrEmpty(planet.Diffuse))
            {
                visual.DiffuseTexture = contentMgr.Load<Texture2D>(planet.Diffuse);
            }
            if (!string.IsNullOrEmpty(planet.Clouds))
            {
                visual.CloudsTexture = contentMgr.Load<Texture2D>(planet.Clouds);
            }
            if (!string.IsNullOrEmpty(planet.CloudsAlpha))
            {
                visual.CloudsAlphaTexture = contentMgr.Load<Texture2D>(planet.CloudsAlpha);
            }

            planet.Visual = visual;
        }

        #endregion

        #region IRenderer members

        public bool RaySphereIntersection(Vector2 screenPosition, Vector3 position, float radius)
        {
            var ray = _camera.GetRay(GraphicsDevice.Viewport, new Vector3(screenPosition, 0));
            return ray.Intersects(new BoundingSphere(position, radius)) != null;
        }

        public void Initialize(GameClient client)
        {
            Client = client;
            GraphicsDevice = Client.GraphicsDevice;

            var contentMgr = Client.Content;
            _fxLinks = contentMgr.Load<Effect>("Effects\\Links");
            _fxPlanet = contentMgr.Load<Effect>("Effects\\Planet");

            var vertices = Utils.SphereVertices(3);
            _sphereVB = new VertexBuffer(GraphicsDevice, VertexPositionNormalTexture.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
            _sphereVB.SetData(vertices);
        }
        public void Release()
        {
            Client = null;
        }
        public void Draw(Scene scene, double delta, double time)
        {
            _camera.Update(delta);

            var map = scene.Map;
            
            #region Links

            _camera.Apply(_fxLinks, Matrix.Identity);

            if (map.Visual == null)
            {
                InitializeMapVisual(map);
            }

            foreach (var pass in _fxLinks.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.SetVertexBuffer(map.Visual.LinksVB);
                GraphicsDevice.DrawPrimitives(PrimitiveType.LineList, 0, map.Visual.LinksVB.VertexCount / 2);
            }

            #endregion

            #region Planets

            foreach (var planet in map.Planets)
            {
                if (planet.Visual == null)
                {
                    InitializePlanetVisual(planet);
                }
                var visual = planet.Visual;

                _fxPlanet.Parameters["Diffuse"].SetValue(visual.DiffuseTexture);
                _fxPlanet.Parameters["Clouds"].SetValue(visual.CloudsTexture);
                _fxPlanet.Parameters["CloudsAlpha"].SetValue(visual.CloudsAlphaTexture);

                var localWorld = Matrix.CreateScale(planet.Radius) * 
                            Matrix.CreateRotationY((float)time / visual.Period * MathHelper.TwoPi) *
                            Matrix.CreateFromYawPitchRoll(visual.Yaw, visual.Pitch, visual.Roll) *
                            Matrix.CreateTranslation(planet.X, planet.Y, planet.Z);

                _camera.Apply(_fxPlanet, localWorld);                
                foreach (var pass in _fxPlanet.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    GraphicsDevice.SetVertexBuffer(_sphereVB);
                    GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, _sphereVB.VertexCount / 3);
                }
            }

            #endregion
        }

        #endregion

        public SimpleCamera GetCamera() { return _camera; }

        public GameClient Client { get; protected set; }
        public GraphicsDevice GraphicsDevice { get; protected set; }
    }
}
