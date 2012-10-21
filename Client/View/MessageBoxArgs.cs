namespace Client.View
{
    using System;

    public class MessageBoxArgs : EventArgs
    {
        public string Title { get; private set; }
        public string Message { get; private set; }

        public MessageBoxArgs(string title, string message)
        {
            Title = title;
            Message = message;
        }
    }
}
