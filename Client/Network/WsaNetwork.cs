namespace Client.Network
{
    using System;
    using System.Net;
    using System.Threading;
    using Common;
    using Model;
    using System.Net.Sockets;
    using System.IO;

    public class WsaNetwork : INetwork
    {
        #region Public members

        public event Action<object> OnMessageReceived;
        public event Action<string> OnDisconnect;

        #endregion

        #region Internal connection handling

        bool IsConnectionSuccessful = false;
        Exception socketexception = null;
        ManualResetEvent TimeoutObject = new ManualResetEvent(false);

        private const int TIMEOUT_MILLISECONDS = 3000;

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

        private void BeginReceive(NetworkStream stream)
        {
            var thread = new Thread(new ParameterizedThreadStart(ReceiveThread));
            thread.Start(stream);
        }

        private void ReceiveThread(object obj)
        {
            var networkStream = obj as NetworkStream;
            var sr = new StreamReader(networkStream, System.Text.Encoding.UTF8);
            string line = null;
            try
            {
                while ((line = sr.ReadLine()) != null)
                {
                    // TODO Interpret message from Json format and identify message type: header or content.
                    //      React specifically due to the message type.

                    if (OnMessageReceived != null)
                    {
                        // TODO there should be full object
                        OnMessageReceived.Invoke(line);
                    }
                }
            }
            catch (IOException ex)
            {
                // Connection may be forcibly closed while waiting for message.
                // We will just catch it.
            }
            finally
            {
                if (OnDisconnect != null)
                {
                    OnDisconnect.Invoke("Connection ended.");
                }
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
                if (TimeoutObject.WaitOne(TIMEOUT_MILLISECONDS, false))
                {
                    if (IsConnectionSuccessful)
                    {
                        BeginReceive(tcpClient.GetStream());
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

        public IAsyncResult BeginGetGameList(AsyncCallback asyncCallback, object asyncState)
        {
            var ar = new AsyncResult<object>(asyncCallback, asyncState);            
            ar.BeginInvoke(() => 
            {
                var rand = new Random();
                int numGames = rand.Next(5, 15);
                var gameNames = new string[numGames];

                for (int i = 0; i < numGames; ++i)
                {
                    gameNames[i] = string.Format("Game {0}", i + 1);
                }

                return gameNames;
            });
            return ar;
        }
        public void EndGetGameList(IAsyncResult asyncResult)
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
