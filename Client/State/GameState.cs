namespace Client.State
{
    using System;
    using System.Collections.Concurrent;
    using Microsoft.Xna.Framework;
    using View;

    public abstract class GameState
    {
        public GameClient Client { get; protected set; }
        public IGWOCTISI Game { get; protected set; }

        public delegate void MessageQueueFunc(object args);
        private ConcurrentQueue<Tuple<MessageQueueFunc, object>> _messageQueue = new ConcurrentQueue<Tuple<MessageQueueFunc, object>>();

        public GameState(GameClient client)
        {
            Game = client as IGWOCTISI;
            Client = client;
        }

        public virtual void OnEnter()
        {
            // Implementation not required
        }

        public virtual void OnExit()
        {
            // Implementation not required
        }

        /// <summary>
        /// The function is to be called from the async callback thread. 
        /// It will invoke the given delegate in the main update thread.
        /// </summary>        
        public void InvokeOnMainThread(MessageQueueFunc functionToInvoke, object arg = null)
        {
            _messageQueue.Enqueue(new Tuple<MessageQueueFunc, object>(functionToInvoke, arg));
        }

        public virtual void OnUpdate(double delta, double time)
        {
			Client.ViewMgr.Update(delta, time);

            if (!_messageQueue.IsEmpty)
            {
                Tuple<MessageQueueFunc, object> front;
                if (_messageQueue.TryDequeue(out front))
                {
                    front.Item1(front.Item2);
                }
            }
        }

        public virtual void BeforeDraw(double delta, double time)
        {
            var graphicsDevice = Client.GraphicsDevice;

            graphicsDevice.Clear(Color.Black);
			Client.ViewMgr.Draw(delta, time);
        }
    }
}
