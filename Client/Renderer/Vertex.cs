
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.InteropServices;


namespace Client.Renderer
{
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct Vertex : IVertexType
	{
		public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
			36,
			new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
			new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
			new VertexElement(24, VertexElementFormat.Color, VertexElementUsage.Color, 0),
			new VertexElement(28, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
		);

		public Vector3 Position;
		public Vector3 Normal;
		public Color Color;
		public Vector2 TextureCoordinate;

		public Vertex(Vector3 position, Vector3 normal, Color color, Vector2 textureCoordinate)
		{
			Position = position;
			Normal = normal;
			Color = color;
			TextureCoordinate = textureCoordinate;
		}

		VertexDeclaration IVertexType.VertexDeclaration
		{
			get { return VertexDeclaration; }
		}
	}
}
