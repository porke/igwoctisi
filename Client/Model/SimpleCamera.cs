
using Client.Common.AnimationSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Client.Renderer;


namespace Client.Model
{
    public class SimpleCamera : ICamera
	{
		#region ITransformable members

		public float X { get; set; }
		public float Y { get; set; }
		public float Z { get; set; }

		public float RotationX { get; set; }
		public float RotationY { get; set; }
		public float RotationZ { get; set; }

		public float ScaleX { get; set; }
		public float ScaleY { get; set; }
		public float ScaleZ { get; set; }

		#endregion

		#region ICamera members

		/// in this case it is also LookAt vector
		public Vector3 Forward { get; protected set; }
		public Vector3 Up
		{
			get { return Vector3.Up; }
		}
		public float AspectRatio { get; set; }
		public Matrix Projection
		{
			get { return Matrix.CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, NearPlane, FarPlane); }
		}

		public Ray GetRay(Viewport viewport, Vector3 pointOnScreen)
		{
			var pointNear = new Vector3(pointOnScreen.X, pointOnScreen.Y, 0);
			var pointFar = new Vector3(pointOnScreen.X, pointOnScreen.Y, 1);
			var pointNearUnproj = viewport.Unproject(pointNear, Projection, this.GetView(), Matrix.Identity);
			var pointFarUnproj = viewport.Unproject(pointFar, Projection, this.GetView(), Matrix.Identity);
			var dir = Vector3.Normalize(pointFarUnproj - pointNearUnproj);
			return new Ray(pointNearUnproj, dir);
		}

		#endregion

		public float FieldOfView { get; set; }
		public float NearPlane { get; set; }
		public float FarPlane { get; set; }
		public Vector3 Min { get; set; }
		public Vector3 Max { get; set; }
        
        public SimpleCamera()
        {
			this.SetPosition(Vector3.Backward * -1000);
			Forward = Vector3.Zero;
			FieldOfView = MathHelper.ToRadians(45);
			AspectRatio = 4.0f / 3.0f;
			NearPlane = 1;
			FarPlane = 10000;
        }
        public void Update(double delta, double time)
        {
        }
    }
}
