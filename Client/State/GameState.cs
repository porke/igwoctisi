namespace Client.State
{
    using View;
    using Microsoft.Xna.Framework;

    public abstract class GameState
    {
        public Client Client { get; protected set; }
        public IGWOCTISI Game { get; protected set; }
        public ViewManager ViewMgr { get; protected set; }

        public GameState(Client client)
        {
            Game = client as IGWOCTISI;
            Client = client;
            ViewMgr = new ViewManager(Client);
        }

        public virtual void OnEnter()
        {
            
        }

        public virtual void OnExit()
        {
            
        }

        public virtual void OnUpdate(double delta, double time)
        {
            ViewMgr.Update(delta, time);
        }

        public virtual void OnDraw(double delta, double time)
        {
            var graphicsDevice = Client.GraphicsDevice;

            graphicsDevice.Clear(Color.White);
            ViewMgr.Draw(delta, time);
        }
    }
}
