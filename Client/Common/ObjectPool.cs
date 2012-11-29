namespace Client.Common
{
    using System;
    using System.Collections.Generic;    

    /// <summary>
    /// Represents a pool of objects with a frameSize limit.
    /// </summary>
    /// <typeparam name="T">The type of object in the pool.</typeparam>
    public sealed class ObjectPool<T> : IDisposable
    {
        private readonly int size;
        private int count;
        private readonly object locker = new object();
        private readonly Queue<T> queue = new Queue<T>();

        public IObjectFactory Factory { get; set; }


        public interface IObjectFactory
        {
            T Fetch();
        }

        private class StandardObjectFactory : IObjectFactory
        {
            private Func<T> factoryFunc;

            public StandardObjectFactory(Func<T> factoryFunc)
            {
                this.factoryFunc = factoryFunc;
            }

            public T Fetch()
            {
                return factoryFunc.Invoke();
            }
        }

        /// <summary>
        /// Initializes a new instance of the ObjectPool class.
        /// </summary>
        /// <param name="frameSize">The frameSize of the object pool.</param>
        /// <param name="factory">Factory method called when new object is required.</param>
        public ObjectPool(int size, Func<T> factoryFunc)
        {
            if (size <= 0)
            {
                const string message = "The size of the pool must be greater than zero.";
                throw new ArgumentOutOfRangeException("size", size, message);
            }

            this.Factory = new StandardObjectFactory(factoryFunc);
            this.size = size;
        }

        public ObjectPool(int size, IObjectFactory factory)
        {
            this.Factory = factory;
            this.size = size;
        }

        /// <summary>
        /// Retrieves an item from the pool. 
        /// </summary>
        /// <returns>The item retrieved from the pool.</returns>
        public T Get()
        {
            lock (locker)
            {
                if (queue.Count > 0)
                {
                    return queue.Dequeue();
                }

                count++;
                return Factory.Fetch();
            }
        }

        public T Get(Action<T> preGetAction)
        {
            T obj = Get();
            preGetAction.Invoke(obj);
            return obj;
        }

        /// <summary>
        /// Places an item in the pool.
        /// </summary>
        /// <param name="item">The item to place to the pool.</param>
        public void Put(T item)
        {
            lock (locker)
            {
                if (count < size)
                {
                    queue.Enqueue(item);
                }
                else
                {
                    using (item as IDisposable)
                    {
                        count--;
                    }
                }
            }
        }

        /// <summary>
        /// Disposes of items in the pool that implement IDisposable.
        /// </summary>
        public void Dispose()
        {
            lock (locker)
            {
                count = 0;
                while (queue.Count > 0)
                {
                    using (queue.Dequeue() as IDisposable) { }
                }
            }
        }
    }
}
