using Microsoft.Xna.Framework;
using System;
namespace Client.Common.AnimationSystem
{
	/// <summary>
	/// If you inherit from this interface, then remember to set Scale to 1!
	/// </summary>
	public interface ITransformable
	{
		float X { get; set; }
		float Y { get; set; }
		float Z { get; set; }

		float RotationX { get; set; }
		float RotationY { get; set; }
		float RotationZ { get; set; }

		float ScaleX { get; set; }
		float ScaleY { get; set; }
		float ScaleZ { get; set; }
	}

	public static class ITransformableExtensions
	{
		public static Matrix CalculateWorldTransform(this ITransformable t)
		{
			return Matrix.CreateRotationX(t.RotationX)
						* Matrix.CreateRotationY(t.RotationY)
						* Matrix.CreateRotationZ(t.RotationZ)
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

		public static void SetRotation(this ITransformable t, Vector3 rot)
		{
			t.RotationX = rot.X;
			t.RotationY = rot.Y;
			t.RotationZ = rot.Z;
		}

		public static void LookAt(this ITransformable t, Vector3 lookAt)
		{
			var dir = (lookAt - t.GetPosition());
			dir.GetRotationMatrix(lookAt);

			if (dir.Z != 0)
			{
				t.RotationX = (float)Math.Atan2(dir.Y, dir.Z);
			}

			if (dir.X != 0)
			{
				t.RotationY = (float)Math.Atan2(dir.Z, dir.X);
			}

			if (dir.X != 0)
			{
				t.RotationZ = (float)Math.Atan2(dir.Y, dir.X);
			}

			return;
			
			// Rotation around axis X
			var up = Vector3.Up;//0,1,0
			float dot = Vector3.Dot(new Vector3(0, dir.Y, dir.Z), up);

			if (!float.IsNaN(dot))
			{
				float angle = (float)Math.Acos(dot);
				if (!float.IsNaN(angle))
				{
					t.RotationX = angle;
				}
			}

			// Rotation around axis Y
			var right = Vector3.Right;//0,0,1
			dot = Vector3.Dot(dir, right);

			if (!float.IsNaN(dot))
			{
				float angle = (float)Math.Acos(dot);
				if (!float.IsNaN(angle))
				{
					t.RotationY = angle;
				}
			}

			// Rotation around axis Z
			var forward = Vector3.Forward;//0,0,-1
			dot = Vector3.Dot(dir, forward);

			if (!float.IsNaN(dot))
			{
				float angle = (float)Math.Acos(dot);
				if (!float.IsNaN(angle))
				{
					t.RotationZ = angle;
				}
			}
		}

		private static Matrix GetRotationMatrix(this Vector3 source, Vector3 target)
		{
			float dot = Vector3.Dot(source, target);
			if (!float.IsNaN(dot))
			{
				float angle = (float)Math.Acos(dot);
				if (!float.IsNaN(angle))
				{
					Vector3 cross = Vector3.Cross(source, target);
					cross.Normalize();
					Matrix rotation = Matrix.CreateFromAxisAngle(cross, angle);
					return rotation;
				}
			}
			return Matrix.Identity;
		}
	}
}
