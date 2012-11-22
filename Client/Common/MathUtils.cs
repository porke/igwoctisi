namespace Client.Common
{
	using Microsoft.Xna.Framework;
	using Microsoft.Xna.Framework.Graphics;


	public static class MathUtils
	{

		/// <summary>
		/// Calculate the rotation for one point to face another point.
		/// </summary>
		public static Matrix LookAt(Vector3 position, Vector3 lookAt, Vector3 up)
		{
			var dir = lookAt - position;
			dir.Normalize();

			Vector3 right = Vector3.Cross(up, dir);
			right.Normalize();

			return new Matrix(
				right.X, right.Y, right.Z, 0,
				up.X, up.Y, up.Z, 0,
				dir.X, dir.Y, dir.Z, 0,
				0, 0, 0, 1
			);
		}

		public static Matrix LookAt(Vector3 position, Vector3 lookAt)
		{
			var look = lookAt - position;
			var up = look.GetUpVector();

			return LookAt(position, lookAt, up);
		}

		public static BoundingBox GetModelBoundingBox(Model model, Matrix worldTransform)
		{
			// Initialize minimum and maximum corners of the bounding box to max and min values
			Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

			// For each mesh of the model
			foreach (ModelMesh mesh in model.Meshes)
			{
				foreach (ModelMeshPart meshPart in mesh.MeshParts)
				{
					// Vertex buffer parameters
					int vertexStride = meshPart.VertexBuffer.VertexDeclaration.VertexStride;
					int vertexBufferSize = meshPart.NumVertices * vertexStride;

					// Get vertex data as float
					float[] vertexData = new float[vertexBufferSize / sizeof(float)];
					meshPart.VertexBuffer.GetData<float>(vertexData);

					// Iterate through vertices (possibly) growing bounding box, all calculations are done in world space
					for (int i = 0; i < vertexBufferSize / sizeof(float); i += vertexStride / sizeof(float))
					{
						Vector3 transformedPosition = Vector3.Transform(new Vector3(vertexData[i], vertexData[i + 1], vertexData[i + 2]), worldTransform);

						min = Vector3.Min(min, transformedPosition);
						max = Vector3.Max(max, transformedPosition);
					}
				}
			}

			// Create and return bounding box
			return new BoundingBox(min, max);
		}

		public static BoundingBox GetMeshBoundingBox(ModelMesh mesh, Matrix worldTransform)
		{
			// Initialize minimum and maximum corners of the bounding box to max and min values
			Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

			foreach (ModelMeshPart meshPart in mesh.MeshParts)
			{
				// Vertex buffer parameters
				int vertexStride = meshPart.VertexBuffer.VertexDeclaration.VertexStride;
				int vertexBufferSize = meshPart.NumVertices * vertexStride;

				// Get vertex data as float
				float[] vertexData = new float[vertexBufferSize / sizeof(float)];
				meshPart.VertexBuffer.GetData<float>(vertexData);

				// Iterate through vertices (possibly) growing bounding box, all calculations are done in world space
				for (int i = 0; i < vertexBufferSize / sizeof(float); i += vertexStride / sizeof(float))
				{
					Vector3 transformedPosition = Vector3.Transform(new Vector3(vertexData[i], vertexData[i + 1], vertexData[i + 2]), worldTransform);

					min = Vector3.Min(min, transformedPosition);
					max = Vector3.Max(max, transformedPosition);
				}
			}

			// Create and return bounding box
			return new BoundingBox(min, max);
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
