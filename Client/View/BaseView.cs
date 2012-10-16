namespace Client.View
{
    using System;
    using System.Collections.Concurrent;
    using Input;
    using Nuclex.UserInterface;
    using State;

    public abstract class BaseView
    {
        public bool IsLoaded { get; protected set; }
        public bool IsTransparent { get; protected set; }
        public IInputReceiver InputReceiver { get; protected set; }

        protected internal ViewManager ViewMgr { get; protected set; }
        protected Screen screen;
        protected GameState state;

        public delegate void MessageFunc(object args);
        private ConcurrentQueue<Tuple<MessageFunc, object>> _messageQueue = new ConcurrentQueue<Tuple<MessageFunc, object>>();

        protected BaseView(GameState controller)
        {
            state = controller;
            screen = new Screen(800, 600);
        }

        /// <summary>
        /// The function is to be called from the async callback thread. It will invoke the given delegate in the main update thread.
        /// </summary>        
        public void Invoke(MessageFunc functionToInvoke, object arg)
        {
            _messageQueue.Enqueue(new Tuple<MessageFunc, object>(functionToInvoke, arg));
        }

        public virtual void OnShow(ViewManager viewMgr, double time)
        {
            this.ViewMgr = viewMgr;
        }

        public virtual void OnHide(double time)
        {
            // No implementation required
        }

        public virtual void Update(double delta, double time)
        {
            if (!_messageQueue.IsEmpty)
            {
                Tuple<MessageFunc, object> front;
                if (_messageQueue.TryDequeue(out front))
                {
                    front.Item1(front.Item2);
                }
            }
        }

        public virtual void Draw(double delta, double time)
        {
            ViewMgr.Client.Visualizer.Draw(screen);
        }
    }
}
