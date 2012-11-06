namespace Client.Renderer
{
    using Client.Common.AnimationSystem;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public class SimpleCamera : IMovable
    {
        private const float Velocity = 250.0f;

        public Vector3 TranslationDirection { get; set; }
        public Vector3 Position
        {
            get { return _world.Translation; }
            set { _world.Translation = value; }
        }

        private Matrix _world;
        private Matrix _view;
        private Matrix _projection;
        
        public SimpleCamera()
        {
            _world = Matrix.Identity;
            _view = Matrix.CreateLookAt(Vector3.Backward * 1000, Vector3.Zero, Vector3.Up);
            //_projection = Matrix.CreateOrthographic(1000.0f, 1000.0f, 1.0f, 1000.0f);
            _projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 1.3333f, 1, 10000);
        }

        public void Update(double delta)
        {
            _world *= Matrix.CreateTranslation(Velocity * (float)delta * TranslationDirection);
        }

        public void ApplyToEffect(Effect effect, Matrix localWorld)
        {
            effect.Parameters["World"].SetValue(localWorld * _world);

            if (effect is BasicEffect)
            {
                var basicEffect = effect as BasicEffect;
                basicEffect.View = _view;
                basicEffect.Projection = _projection;
            }
            else
            {
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
