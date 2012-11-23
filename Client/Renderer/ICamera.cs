
using Client.Common.AnimationSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Client.Renderer
{
	public interface ICamera : ITransformable
	{
		Vector3 Forward { get; }
		Vector3 Up { get; }
		float AspectRatio { get; set; }
		Matrix Projection { get; }
		float FieldOfView { get; set; }

		Ray GetRay(Viewport viewport, Vector3 pointOnScreen);
	}

	public static class ICameraExtensions
	{
		public static Matrix GetView(this ICamera camera)
		{
			return Matrix.Invert(Matrix.CreateWorld(camera.GetPosition(), camera.Forward, camera.Up));//camera.GetUpVector()));
		}
		public static Vector3 Project(this ICamera camera, Viewport viewport, Vector3 worldVector)
		{
			return viewport.Project(worldVector, camera.Projection, camera.GetView(), Matrix.Identity);
		}
	}
}
