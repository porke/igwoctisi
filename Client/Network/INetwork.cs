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
        bool EndLogin(IAsyncResult asyncResult);
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
        IAsyncResult BeginCreateGame(string gameName, string mapJsonContent, AsyncCallback asyncCallback, object asyncState);
        void EndCreateGame(IAsyncResult asyncResult);

        IAsyncResult BeginReceiveGameState(AsyncCallback asyncCallback, object asyncState);
        Map EndReceiveGameState(IAsyncResult asyncResult);
        IAsyncResult BeginSendCommands(UserCommands commands, AsyncCallback asyncCallback, object asyncState);
        void EndSendCommands(IAsyncResult asyncResult);

        /// <summary>
        /// Arguments: username, chat message, time
        /// </summary>
        event Action<ChatMessage> OnChatMessageReceived;

        /// <summary>
        /// Argument: disconnection reason (may be empty)
        /// </summary>
        event Action<string> OnDisconnected;
    }
}
