namespace Client.Renderer
{
    using Model;

    public interface IRenderer
    {
        void Initialize(GameClient client);
        void Release();
        void Draw(Scene scene, double delta, double time);
    }
}
