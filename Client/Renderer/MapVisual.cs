namespace Client.Renderer
{
	using System.Linq;
	using Microsoft.Xna.Framework.Graphics;
	using Client.Model;
	using Microsoft.Xna.Framework.Content;
	using Microsoft.Xna.Framework;
	
	public class MapVisual
	{
		public Map Map { get; protected set; }
        public VertexBuffer LinksVB { get; protected set; }

		public MapVisual(Map map, GraphicsDevice device, ContentManager contentMgr)
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

			LinksVB = new VertexBuffer(device, VertexPositionColor.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
			LinksVB.SetData(vertices);

			foreach (var planet in Map.Planets)
			{
				planet.Visual = new PlanetVisual(planet, device, contentMgr);
			}
		}
	}
}
