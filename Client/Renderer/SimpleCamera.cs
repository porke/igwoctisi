namespace Client.Renderer
{
    using Client.Common.AnimationSystem;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public class SimpleCamera : ITransformable
    {
        private const float Velocity = 250.0f;

		public float X { get; set; }
		public float Y { get; set; }
		public float Z { get; set; }

		public float RotationX { get; set; }
		public float RotationY { get; set; }
		public float RotationZ { get; set; }

		public float ScaleX { get; set; }
		public float ScaleY { get; set; }
		public float ScaleZ { get; set; }

        public Vector3 TranslationDirection { get; set; }
		public Vector3 LookAt { get; set; }

        private Matrix _world;
		private Matrix _view
		{
			get { return Matrix.CreateLookAt(this.GetPosition(), LookAt, Vector3.Up); }
		}
        private Matrix _projection;
        
        public SimpleCamera(GraphicsDevice graphicsDevice)
        {
            _world = Matrix.Identity;
			this.SetPosition(Vector3.Backward * -1000);
			LookAt = Vector3.Zero;
            _projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), graphicsDevice.Viewport.AspectRatio, 1, 10000);
        }

        public void Update(double delta)
        {
			this.SetPosition(this.GetPosition() + (float)delta * TranslationDirection * 100);
			LookAt += (float)delta * TranslationDirection * 100;
        }

        public void ApplyToEffect(Effect effect, Matrix localWorld)
        {
            if (effect is BasicEffect)
            {
                var basicEffect = effect as BasicEffect;
				basicEffect.World = localWorld * _world;
                basicEffect.View = _view;
                basicEffect.Projection = _projection;
            }
            else
            {
				effect.Parameters["World"].SetValue(localWorld * _world);
                effect.Parameters["View"].SetValue(_view);
                effect.Parameters["Projection"].SetValue(_projection);
            }
        }

        public Ray GetRay(Viewport viewport, Vector3 pointOnScreen)
        {
            var pointNear = new Vector3(pointOnScreen.X, pointOnScreen.Y, 0);
            var pointFar = new Vector3(pointOnScreen.X, pointOnScreen.Y, 1);
            var pointNearUnproj = viewport.Unproject(pointNear, _projection, _view, _world);
            var pointFarUnproj = viewport.Unproject(pointFar, _projection, _view, _world);
            var dir = Vector3.Normalize(pointFarUnproj - pointNearUnproj);
            return new Ray(pointNearUnproj, dir);
        }
		public Vector3 Project(Viewport viewport, Vector3 worldVector)
		{
			return viewport.Project(worldVector, _projection, _view, _world);
		}
    }
}
