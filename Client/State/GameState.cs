namespace Client.State
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using View;

    public abstract class GameState
    {
        public GameClient Client { get; protected set; }
        public IGWOCTISI Game { get; protected set; }
        public ViewManager ViewMgr { get; protected set; }

        protected delegate void EventHandler(EventArgs args);
        protected Dictionary<string, EventHandler> eventHandlers = new Dictionary<string, EventHandler>();

        public GameState(GameClient client)
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

            graphicsDevice.Clear(Color.Black);
            ViewMgr.Draw(delta, time);
        }

        public void HandleViewEvent(string eventId, EventArgs args)
        {
            if (eventHandlers.ContainsKey(eventId))
            {
                eventHandlers[eventId](args);
            }
        }
    }
}
