namespace Client.Network
{
    using System;
    using System.Collections.Generic;
    using Model;

    public interface INetwork
    {
        void Initialize(GameClient client);
        void Release();
        void Update(double delta, double time);

        IAsyncResult BeginConnect(string hostname, int port, AsyncCallback asyncCallback, object asyncState);
        bool EndConnect(IAsyncResult asyncResult);
        IAsyncResult BeginLogin(string login, string password, AsyncCallback asyncCallback, object asyncState);
        Player EndLogin(IAsyncResult asyncResult);
        IAsyncResult BeginLogout(AsyncCallback asyncCallback, object asyncState);
        void EndLogout(IAsyncResult asyncResult);
        IAsyncResult BeginJoinGameLobby(int lobbyId, AsyncCallback asyncCallback, object asyncState);
        SpecificGameLobbyInfo EndJoinGameLobby(IAsyncResult asyncResult);
        IAsyncResult BeginLeaveGame(AsyncCallback asyncCallback, object asyncState);
        void EndLeaveGame(IAsyncResult asyncResult);
        IAsyncResult BeginDisconnect(AsyncCallback asyncCallback, object asyncState);
        void EndDisconnect(IAsyncResult asyncResult);
        IAsyncResult BeginGetGameList(AsyncCallback asyncCallback, object asyncState);
        List<LobbyListInfo> EndGetGameList(IAsyncResult asyncResult);
        IAsyncResult BeginSendChatMessage(string message, AsyncCallback asyncCallback, object asyncState);
        void EndSendChatMessage(IAsyncResult asyncResult);
        IAsyncResult BeginCreateGame(string gameName, Map map, AsyncCallback asyncCallback, object asyncState);
        void EndCreateGame(IAsyncResult asyncResult);
        IAsyncResult BeginKickPlayer(string username, AsyncCallback asyncCallback, object asyncState);
        void EndKickPlayer(IAsyncResult asyncResult);
        IAsyncResult BeginStartGame(AsyncCallback asyncCallback, object asyncState);
        void EndStartGame(IAsyncResult asyncResult);

        IAsyncResult BeginReceiveGameState(AsyncCallback asyncCallback, object asyncState);
        Map EndReceiveGameState(IAsyncResult asyncResult);
        IAsyncResult BeginSendCommands(List<UserCommand> commands, AsyncCallback asyncCallback, object asyncState);
        void EndSendCommands(IAsyncResult asyncResult);

        #region Events

        /// <summary>
        /// Arguments: username, chat message, time
        /// </summary>
        event Action<ChatMessage> OnChatMessageReceived;

        /// <summary>
        /// Arguments: username, time.
        /// </summary>
        event Action<string, DateTime> OnOtherPlayerJoined;

        /// <summary>
        /// Arguments: username, time.
        /// </summary>
        event Action<string, DateTime> OnOtherPlayerLeft;

        /// <summary>
        /// Arguments: username, time.
        /// </summary>
        event Action<string, DateTime> OnOtherPlayerKicked;

        /// <summary>
        /// Arguments: currently none.
        /// </summary>
        event Action OnPlayerKicked;

        /// <summary>
        /// Event for non-hosting players.
        /// Argument: map.
        /// </summary>
        event Action<Map> OnGameStarted;

        /// <summary>
        /// Arguments: roundSeconds.
        /// Return true if message was consumed.
        /// </summary>
        event Func<NewRoundInfo, bool> OnRoundStarted;

        /// <summary>
        /// Arguments: currently none.
        /// Return true if message was consumed.
        /// </summary>
        event Func<List<SimulationResult>, bool> OnRoundEnded;

        /// <summary>
        /// Arguments: currently none.
        /// </summary>
        event Action OnGameEnded;

        /// <summary>
        /// Argument: disconnection reason (may be empty)
        /// </summary>
        event Action<string> OnDisconnected;

        #endregion
    }
}
