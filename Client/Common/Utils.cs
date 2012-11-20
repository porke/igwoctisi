namespace Client.Common
{
	using System;
	using System.Linq;
	using System.Reflection;
	using Microsoft.Xna.Framework;
	using Microsoft.Xna.Framework.Graphics;
	using System.Diagnostics;

	public static class Utils
    {
        #region Text methods

        public static string LowerFirstLetter(string str)
        {
            return char.ToLower(str[0]) + str.Substring(1);
        }
        
        public static string UpperFirstLetter(string str)
        {
            return char.ToUpper(str[0]) + str.Substring(1);
        }

        public static string SplitWordsByCapitals(string str)
        {
            return string.Concat(str.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
        }

        #endregion

        private static Random rand = new Random();
        public static T RandomEnum<T>()
        {
            FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Static | BindingFlags.Public);
            int index = rand.Next(fields.Length);

            return (T)fields[index].GetValue(null);
        }

        public static T RandomEnum<T>(T withoutVal)
        {
            FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Static | BindingFlags.Public);
            int index = rand.Next(fields.Length);

            if (fields[index].GetValue(null).Equals(withoutVal))
                index++;
            if (index >= fields.Length)
                index = 0;

            return (T)fields[index].GetValue(null);
        }

		public static void SwapRef<T>(ref T l, ref T r)
		{
			T tmp = l;
			l = r;
			r = tmp;
		}

        public static VertexPositionNormalTexture[] SphereVertices(int subdivisions)
        {
			var vertices = new[]
            {
                // 0
                new VertexPositionNormalTexture(new Vector3( 0.0f, 1.0f, 0.0f), Vector3.Zero, new Vector2(0.125f, 0.0f)),
                new VertexPositionNormalTexture(new Vector3( 0.0f, 0.0f,-1.0f), Vector3.Zero, new Vector2(0.000f, 0.5f)),
                new VertexPositionNormalTexture(new Vector3( 1.0f, 0.0f, 0.0f), Vector3.Zero, new Vector2(0.250f, 0.5f)),
                new VertexPositionNormalTexture(new Vector3( 0.0f,-1.0f, 0.0f), Vector3.Zero, new Vector2(0.125f, 1.0f)),
                new VertexPositionNormalTexture(new Vector3( 1.0f, 0.0f, 0.0f), Vector3.Zero, new Vector2(0.250f, 0.5f)),
                new VertexPositionNormalTexture(new Vector3( 0.0f, 0.0f,-1.0f), Vector3.Zero, new Vector2(0.001f, 0.5f)),

                // 1
                new VertexPositionNormalTexture(new Vector3( 0.0f, 1.0f, 0.0f), Vector3.Zero, new Vector2(0.375f, 0.0f)),
                new VertexPositionNormalTexture(new Vector3( 1.0f, 0.0f, 0.0f), Vector3.Zero, new Vector2(0.250f, 0.5f)),
                new VertexPositionNormalTexture(new Vector3( 0.0f, 0.0f, 1.0f), Vector3.Zero, new Vector2(0.500f, 0.5f)),
                new VertexPositionNormalTexture(new Vector3( 0.0f,-1.0f, 0.0f), Vector3.Zero, new Vector2(0.375f, 1.0f)),
                new VertexPositionNormalTexture(new Vector3( 0.0f, 0.0f, 1.0f), Vector3.Zero, new Vector2(0.500f, 0.5f)),
                new VertexPositionNormalTexture(new Vector3( 1.0f, 0.0f, 0.0f), Vector3.Zero, new Vector2(0.250f, 0.5f)),

                // 2
                new VertexPositionNormalTexture(new Vector3( 0.0f, 1.0f, 0.0f), Vector3.Zero, new Vector2(0.625f, 0.0f)),
                new VertexPositionNormalTexture(new Vector3( 0.0f, 0.0f, 1.0f), Vector3.Zero, new Vector2(0.500f, 0.5f)),
                new VertexPositionNormalTexture(new Vector3(-1.0f, 0.0f, 0.0f), Vector3.Zero, new Vector2(0.750f, 0.5f)),
                new VertexPositionNormalTexture(new Vector3( 0.0f,-1.0f, 0.0f), Vector3.Zero, new Vector2(0.625f, 1.0f)),
                new VertexPositionNormalTexture(new Vector3(-1.0f, 0.0f, 0.0f), Vector3.Zero, new Vector2(0.750f, 0.5f)),
                new VertexPositionNormalTexture(new Vector3( 0.0f, 0.0f, 1.0f), Vector3.Zero, new Vector2(0.500f, 0.5f)),

                // 3
                new VertexPositionNormalTexture(new Vector3( 0.0f, 1.0f, 0.0f), Vector3.Zero, new Vector2(0.875f, 0.0f)),
                new VertexPositionNormalTexture(new Vector3(-1.0f, 0.0f, 0.0f), Vector3.Zero, new Vector2(0.750f, 0.5f)),
                new VertexPositionNormalTexture(new Vector3( 0.0f, 0.0f,-1.0f), Vector3.Zero, new Vector2(1.000f, 0.5f)),
                new VertexPositionNormalTexture(new Vector3( 0.0f,-1.0f, 0.0f), Vector3.Zero, new Vector2(0.875f, 1.0f)),
                new VertexPositionNormalTexture(new Vector3( 0.0f, 0.0f,-1.0f), Vector3.Zero, new Vector2(0.999f, 0.5f)),
                new VertexPositionNormalTexture(new Vector3(-1.0f, 0.0f, 0.0f), Vector3.Zero, new Vector2(0.750f, 0.5f)),
            };
            var result = vertices;

            for (var i = 0; i < subdivisions; ++i)
            {
				result = new VertexPositionNormalTexture[vertices.Length * 4];

                for (var j = 0; j < vertices.Length; j += 3)
                {
                    var v0 = vertices[j + 0];
                    var v1 = vertices[j + 1];
                    var v2 = vertices[j + 2];

					var a = new VertexPositionNormalTexture();
                    a.Position = (v0.Position + v1.Position) / 2.0f;
                    a.TextureCoordinate = (v0.TextureCoordinate + v1.TextureCoordinate) / 2.0f;

					var b = new VertexPositionNormalTexture();
                    b.Position = (v1.Position + v2.Position) / 2.0f;
                    b.TextureCoordinate = (v1.TextureCoordinate + v2.TextureCoordinate) / 2.0f;

					var c = new VertexPositionNormalTexture();
                    c.Position = (v2.Position + v0.Position) / 2.0f;
                    c.TextureCoordinate = (v2.TextureCoordinate + v0.TextureCoordinate) / 2.0f;

                    result[4*j + 3*0 + 0] = v0;
                    result[4*j + 3*0 + 1] = a;
                    result[4*j + 3*0 + 2] = c;

                    result[4*j + 3*1 + 0] = a;
                    result[4*j + 3*1 + 1] = v1;
                    result[4*j + 3*1 + 2] = b;

                    result[4*j + 3*2 + 0] = c;
                    result[4*j + 3*2 + 1] = b;
                    result[4*j + 3*2 + 2] = v2;

                    result[4*j + 3*3 + 0] = b;
                    result[4*j + 3*3 + 1] = c;
                    result[4*j + 3*3 + 2] = a;
                }

                vertices = result;
            }

            for (var i = 0; i < result.Length; ++i)
            {
                var position = result[i].Position = Vector3.Normalize(result[i].Position);

                result[i].TextureCoordinate.X = (float)(0.5 - Math.Atan2(position.Z, position.X)/MathHelper.TwoPi);
                result[i].TextureCoordinate.Y = (float)(0.5 - 2.0*Math.Asin(position.Y)/MathHelper.TwoPi);

                result[i].Normal = position;
            }

            for (var i = result.Length/4*3; i < result.Length; ++i)
            {
                if (result[i].TextureCoordinate.X <= 0.01f)
                {
                    result[i].TextureCoordinate.X += 1.0f;
                }
            }

            return result;
        }
    }

	public static class MathExtensions
	{
		/// <summary>
		/// Gets up vector for given look vector and roll rotation angle.
		/// </summary>
		/// <param name="lookAt">normalized look vector</param>
		/// <param name="roll">radians</param>
		/// <returns>normalized up vector</returns>
		public static Vector3 GetUpVector(this Vector3 look, float roll = 0)
		{
			var right = Vector3.Cross(look, Vector3.Up);
			var up = Vector3.Cross(right, look);
			up.Normalize();
			
			if (roll != 0)
			{
				var rotation = Matrix.CreateFromAxisAngle(look, roll);
				up = Vector3.TransformNormal(up, rotation);
			}

			return Vector3.Normalize(up);
		}
	}
}
