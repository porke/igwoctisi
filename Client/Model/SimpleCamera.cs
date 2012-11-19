namespace Client.Model
{
	using System.Runtime.Serialization;
	using Client.Common;
	using Client.Common.AnimationSystem;
	using Client.Renderer;
	using Microsoft.Xna.Framework;
	using Microsoft.Xna.Framework.Graphics;

	[DataContract]
    public class SimpleCamera : ICamera
	{
		#region ITransformable members

		public float X { get; set; }
		public float Y { get; set; }
		public float Z { get; set; }

		public Matrix Rotation { get; set; }

		public float ScaleX { get; set; }
		public float ScaleY { get; set; }
		public float ScaleZ { get; set; }

		#endregion

		#region ICamera members

		public Vector3 Forward
		{
			get { return Vector3.Normalize(LookAt - this.GetPosition()); }
		}
		public Vector3 Up
		{
			get { return Vector3.Up; }
		}
		public float AspectRatio { get; set; }
		public Matrix Projection
		{
			get { return Matrix.CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, NearPlane, FarPlane); }
			//get { return Matrix.CreateOrthographicOffCenter(-640, 640, -360, 360, NearPlane, FarPlane); }
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

		public const float DecelerationFactor = 5;
		public const float BoundsExceedFactor = 3;
		public const float ZoomExceedFactor = 5;
		public static readonly Vector3 MaxForce = new Vector3(10000, 10000, 3000);

		public Vector3 LookAt { get; protected set; }
		public float FieldOfView { get; set; }
		public float NearPlane { get; set; }
		public float FarPlane { get; set; }
		
		[DataMember]
		public Vector3 Min { get; set; }
		[DataMember]
		public Vector3 Max { get; set; }
		public Vector3 Force { get; set; }
        
        public SimpleCamera()
        {
			this.SetPosition(Vector3.Forward * 1000);
			LookAt = Vector3.Zero;
			FieldOfView = MathHelper.ToRadians(45);
			AspectRatio = 1280.0f/720.0f;
			NearPlane = 1;
			FarPlane = 10000;
        }
        public void Update(double delta, double time)
        {
			var oldPosition = this.GetPosition();

			var destForce = Vector3.Zero;
			destForce -= Vector3.Min(oldPosition - Min, Vector3.Zero) * BoundsExceedFactor;
			destForce -= Vector3.Max(oldPosition - Max, Vector3.Zero) * BoundsExceedFactor;
			destForce *= new Vector3(1, 1, ZoomExceedFactor);
			Force = Vector3.Lerp(Force, destForce, (float)delta * DecelerationFactor);
			Force = Vector3.Min(Force, MaxForce);
			Force = Vector3.Max(Force, -MaxForce);

			var newPosition = oldPosition + Force * (float)delta;
			this.SetPosition(newPosition);
			this.LookAt += Force * (float)delta * new Vector3(1, 1, 0);

			// Update camera rotation
			this.LookAt(LookAt);
        }
    }
}
