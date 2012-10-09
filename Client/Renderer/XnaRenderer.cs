namespace Client.Renderer
{
    using Model;

    public class XnaRenderer : IRenderer
    {
        #region IRenderer members

        public void Initialize(Client client)
        {
            Client = client;
        }
        public void Release()
        {
            Client = null;
        }
        public void Draw(Scene scene, double delta, double time)
        {
        }

        #endregion

        public Client Client { get; protected set; }
    }
}
