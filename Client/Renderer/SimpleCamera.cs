namespace Client.Renderer
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public class SimpleCamera
    {
        private const float Velocity = 250.0f;

        public Vector3 TranslationDirection { get; set; }

        private Matrix _world;
        private Matrix _view;
        private Matrix _projection;
        
        public SimpleCamera()
        {
            _world = Matrix.Identity;
            _view = Matrix.CreateLookAt(Vector3.Backward * -1000, Vector3.Zero, Vector3.Up);
            _projection = Matrix.CreateOrthographic(1000.0f, 1000.0f, 1.0f, 1000.0f);
        }

        public void Update(double delta)
        {
            _world *= Matrix.CreateTranslation(Velocity * (float)delta * TranslationDirection);
        }

        public void Apply(Effect effect, Matrix localWorld)
        {
            effect.Parameters["World"].SetValue(localWorld * _world);
            effect.Parameters["View"].SetValue(_view);
            effect.Parameters["Projection"].SetValue(_projection);
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
    }
}
