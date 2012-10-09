namespace Client.Renderer
{
    using Model;

    public interface IRenderer
    {
        void Initialize(Client client);
        void Release();
        void Draw(Scene scene, double delta, double time);
    }
}
