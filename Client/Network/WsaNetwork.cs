namespace Client.Network
{
    using System;
    using System.Net;
    using System.Threading;
    using Common;
    using Model;
    using System.Net.Sockets;

    public class WsaNetwork : INetwork
    {
        #region Public members

        public event Action<object> OnMessageReceived;

        #endregion

        #region Internal connection handling

        bool IsConnectionSuccessful = false;
        Exception socketexception = null;
        ManualResetEvent TimeoutObject = new ManualResetEvent(false);

        private const int TIMEOUT_MILISECONDS = 3000;

        private void TcpConnectCallback(IAsyncResult tcpAsyncResult)
        {
            try
            {
                IsConnectionSuccessful = false;
                TcpClient tcpclient = tcpAsyncResult.AsyncState as TcpClient;
             
                if (tcpclient.Client != null)
                {
                    tcpclient.EndConnect(tcpAsyncResult);
                    IsConnectionSuccessful = true;
                }
            }
            catch (Exception ex)
            {
                IsConnectionSuccessful = false;
                socketexception = ex;
            }
            finally
            {
                TimeoutObject.Set();
            }    
        }
    
        #endregion

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

        /// <summary>
        /// Connects to the server and starts listening for messages from it.
        /// To receive messages register OnMessageReceived event.
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        /// <param name="asyncCallback"></param>
        /// <param name="asyncState"></param>
        /// <returns></returns>
        public IAsyncResult BeginConnect(string hostname, int port, AsyncCallback asyncCallback, object asyncState)
        {
            var tcpClient = new TcpClient();
            tcpClient.BeginConnect(hostname, port, new AsyncCallback(TcpConnectCallback), tcpClient);

            var ar = new AsyncResult<object>(asyncCallback, asyncState);
            ar.BeginInvoke(() =>
            {
                if (TimeoutObject.WaitOne(TIMEOUT_MILISECONDS, false))
                {
                    if (IsConnectionSuccessful)
                    {
                        return true;
                    }
                    else
                    {
                        throw socketexception;
                    }
                }
                else
                {
                    tcpClient.Close();
                    throw new TimeoutException("TimeOut Exception");
                }
            });

            return ar;
        }
        public void EndConnect(IAsyncResult asyncResult)
        {
            var ar = (AsyncResult<object>)asyncResult;
            var res = ar.Result;
            ar.EndInvoke();
        }
        public IAsyncResult BeginLogin(string login, string password, AsyncCallback asyncCallback, object asyncState)
        {
            var ar = new AsyncResult<GameState>(asyncCallback, asyncState);

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
