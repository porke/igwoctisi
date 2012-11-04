namespace Client.Common
{
    using System;
    using System.Threading;

    public sealed class AsyncResult<T> : IAsyncResult, IDisposable
    {
        #region IAsyncResult members

        public bool IsCompleted { get; private set; }
        public WaitHandle AsyncWaitHandle
        {
            get { return _waitHandle; }
        }
        public object AsyncState { get; private set; }
        public bool CompletedSynchronously { get; private set; }

        #endregion

        #region Private members

        private readonly AsyncCallback _asyncCallback;
        private readonly ManualResetEvent _waitHandle;

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (this)
                {
                    if (_waitHandle != null)
                    {
                        _waitHandle.Dispose();
                    }
                }
            }
        }
        private void SignalCompletion()
        {
            _waitHandle.Set();
            ThreadPool.QueueUserWorkItem(InvokeCallback);
        }
        private void InvokeCallback(object state)
        {
            if (_asyncCallback != null)
                _asyncCallback(this);
        }

        #endregion

        public Exception Exception { get; private set; }
        public T Result { get; private set; }

        public AsyncResult(AsyncCallback asyncCallback, object asyncState)
        {
            _asyncCallback = asyncCallback;
            AsyncState = asyncState;
            IsCompleted = false;
            CompletedSynchronously = false;
            _waitHandle = new ManualResetEvent(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Complete(T result, bool completedSynchronously)
        {
            lock (this)
            {
                IsCompleted = true;
                CompletedSynchronously = completedSynchronously;
                Result = result;
            }

            SignalCompletion();
        }

        public void HandleException(Exception exc, bool completedSynchronously)
        {
            lock (this)
            {
                IsCompleted = true;
                CompletedSynchronously = completedSynchronously;
                Exception = exc;
            }

            SignalCompletion();
        }

        public void BeginInvoke(Func<T> method)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    var result = method();
                    Complete(result, false);
                }
                catch (Exception exc)
                {
                    HandleException(exc, false);
                }
            });
        }

        public T EndInvoke()
        {
            AsyncWaitHandle.WaitOne();
            if (Exception != null)
                throw Exception;

            return Result;
        }
    }
}
