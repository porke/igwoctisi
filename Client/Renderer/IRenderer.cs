namespace Client.Renderer
{
    using Model;
    using Microsoft.Xna.Framework;

    public interface IRenderer
    {
        void Initialize(GameClient client);
        void Release();
		void Draw(ICamera camera, Scene scene, double delta, double time);
        bool RaySphereIntersection(ICamera camera, Vector2 screenPosition, Vector3 position, float radius);
		bool RayLinkIntersection(ICamera camera, Vector2 screenPosition, Vector3 linkSource, Vector3 linkTarget);
    }
}
