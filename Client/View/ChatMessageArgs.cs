using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client.View
{
    public class ChatMessageArgs : EventArgs
    {
        public string Message { get; set; }

        public ChatMessageArgs(string message)
        {
            Message = message;
        }
    }
}
