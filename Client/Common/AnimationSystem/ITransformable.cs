using Microsoft.Xna.Framework;
using System;
namespace Client.Common.AnimationSystem
{
	/// <summary>
	/// If you inherit from this interface, then remember that Scale equals zero! You should mminimally change it to 1!
	/// </summary>
	public interface ITransformable
	{
		float X { get; set; }
		float Y { get; set; }
		float Z { get; set; }

		Matrix Rotation { get; set; }

		float ScaleX { get; set; }
		float ScaleY { get; set; }
		float ScaleZ { get; set; }
	}

	public static class ITransformableExtensions
	{
		public static Matrix CalculateWorldTransform(this ITransformable t)
		{
			return t.Rotation
						* Matrix.CreateScale(t.ScaleX, t.ScaleY, t.ScaleZ)
						* Matrix.CreateTranslation(t.X, t.Y, t.Z);
		}

		public static void SetPosition(this ITransformable t, Vector3 position)
		{
			t.X = position.X;
			t.Y = position.Y;
			t.Z = position.Z;
		}

		public static Vector3 GetPosition(this ITransformable t)
		{
			return new Vector3(t.X, t.Y, t.Z);
		}

		public static void LookAt(this ITransformable t, Vector3 lookAt, Vector3 up)
		{
			var dir = lookAt - t.GetPosition();
			dir.Normalize();

			Vector3 right = Vector3.Cross(up, dir);
			right.Normalize();

			t.Rotation = new Matrix(
				right.X, right.Y, right.Z, 0,
				up.X, up.Y, up.Z, 0,
				dir.X, dir.Y, dir.Z, 0,
				0, 0, 0, 1
			);
		}
	}
}
