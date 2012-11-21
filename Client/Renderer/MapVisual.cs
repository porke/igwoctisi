namespace Client.Renderer
{
	using System.Linq;
	using Microsoft.Xna.Framework.Graphics;
	using Client.Model;
	using Microsoft.Xna.Framework.Content;
	using Microsoft.Xna.Framework;
	using System;
	
	public class MapVisual
	{
		#region Protected members

		protected SpriteBatch _spriteBatch;
		protected Tuple<BackgroundLayer, Texture2D>[] _layers;

		#endregion

		public Map Map { get; protected set; }
        public VertexBuffer LinksVB { get; protected set; }

		public MapVisual(GameClient client, Map map)
		{
			Map = map;

			var vertices = new VertexPositionColor[map.Links.Count * 2];
			var color = Color.LightGreen;

			for (var i = 0; i < map.Links.Count; ++i)
			{
				var link = Map.Links[i];
				var sourcePlanet = Map.Planets.First(x => x.Id == link.SourcePlanet);
				var targetPlanet = Map.Planets.First(x => x.Id == link.TargetPlanet);

				vertices[2 * i + 0] = new VertexPositionColor(new Vector3(sourcePlanet.X, sourcePlanet.Y, sourcePlanet.Z), color);
				vertices[2 * i + 1] = new VertexPositionColor(new Vector3(targetPlanet.X, targetPlanet.Y, targetPlanet.Z), color);
			}

			LinksVB = new VertexBuffer(client.GraphicsDevice, VertexPositionColor.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
			LinksVB.SetData(vertices);

			_spriteBatch = new SpriteBatch(client.GraphicsDevice);
			_layers = Map.Background.Select(x => Tuple.Create(x, client.Content.Load<Texture2D>(x.Texture))).ToArray();

			foreach (var planet in Map.Planets)
			{
				planet.Visual = new PlanetVisual(client, planet);
			}

			foreach (var planetarySystem in map.PlanetarySystems)
			{
				planetarySystem.Visual = new PlanetarySystemVisual(client, planetarySystem);
				planetarySystem.Visual.Color = Color.LightGray;
				client.Components.Add(planetarySystem.Visual.ParticleSystem);
			}
		}
		internal void Initialize()
		{
			foreach (var planet in Map.Planets)
			{
				planet.Visual.Initialize();
			}
		}
		public void DrawBackground(Viewport viewport, double delta, double time)
		{
			_spriteBatch.Begin();

			foreach (var pair in _layers)
			{
				var layer = pair.Item1;
				var texture = pair.Item2;

				var worldCamera = new Vector2(Map.Camera.X, Map.Camera.Y);
				var halfView = new Vector2(viewport.Width, viewport.Height) / 2.0f;
				var worldOrigin = layer.Origin + worldCamera*(layer.Speed - 1.0f);
				var screenOrigin = (worldOrigin + worldCamera) * new Vector2(1, -1) + halfView;
				var scale = layer.Size / new Vector2(texture.Width, texture.Height);
				_spriteBatch.Draw(texture, screenOrigin, null, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 1);
			}

			_spriteBatch.End();
		}
	}
}
