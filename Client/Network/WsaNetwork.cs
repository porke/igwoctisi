namespace Client.Network
{
    using System;
    using System.Net;
    using System.Threading;
    using Common;
    using Model;

    public class WsaNetwork : INetwork
    {
        #region INetwork members

        public void Initialize(Client client)
        {
            Client = client;
        }
        public void Release()
        {
            Client = null;
        }
        public void Update(double delta, double time)
        {
        }

        public IAsyncResult BeginConnect(EndPoint remoteEP, AsyncCallback asyncCallback, object asyncState)
        {
            var ar = new AsyncResult<object>(asyncCallback, asyncState);
            // simulate time consuming task
            ar.BeginInvoke(() => { Thread.Sleep(500); return null; });
            return ar;
        }
        public void EndConnect(IAsyncResult asyncResult)
        {
            var ar = (AsyncResult<object>)asyncResult;
            ar.EndInvoke();
        }
        public IAsyncResult BeginLogin(string login, string password, AsyncCallback asyncCallback, object asyncState)
        {
            var ar = new AsyncResult<object>(asyncCallback, asyncState);
            ar.BeginInvoke(() => { Thread.Sleep(500); return null; });
            return ar;
        }
        public void EndLogin(IAsyncResult asyncResult)
        {
            var ar = (AsyncResult<object>)asyncResult;
        }
        public IAsyncResult BeginDisconnect(AsyncCallback asyncCallback, object asyncState)
        {
            var ar = new AsyncResult<object>(asyncCallback, asyncState);
            // simulate time consuming task
            ar.BeginInvoke(() => { Thread.Sleep(500); return null; });
            return ar;
        }
        public void EndDisconnect(IAsyncResult asyncResult)
        {
            var ar = (AsyncResult<object>)asyncResult;
            ar.EndInvoke();
        }

        public IAsyncResult BeginReceiveGameState(AsyncCallback asyncCallback, object asyncState)
        {
            var ar = new AsyncResult<GameState>(asyncCallback, asyncState);
            // simulate time consuming task
            ar.BeginInvoke(() => { Thread.Sleep(500); return null; });
            return ar;
        }
        public GameState EndReceiveGameState(IAsyncResult asyncResult)
        {
            var ar = (AsyncResult<GameState>)asyncResult;
            return ar.EndInvoke();
        }
        public IAsyncResult BeginSendCommands(UserCommands commands, AsyncCallback asyncCallback, object asyncState)
        {
            var ar = new AsyncResult<object>(asyncCallback, asyncState);
            // simulate time consuming task
            ar.BeginInvoke(() => { Thread.Sleep(500); return null; });
            return ar;
        }
        public void EndSendCommands(IAsyncResult asyncResult)
        {
            var ar = (AsyncResult<object>)asyncResult;
            ar.EndInvoke();
        }

        #endregion

        public Client Client { get; protected set; }
    }
}
