namespace Client.Renderer
{
    using Model;
    using Microsoft.Xna.Framework;

    public interface IRenderer
    {
        SimpleCamera GetCamera();

        void Initialize(GameClient client);
        void Release();
        void Draw(Scene scene, double delta, double time);
        bool RaySphereIntersection(Vector2 screenPosition, Vector3 position, float radius);
    }
}
