using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client.View.Lobby
{
    public class KickPlayerArgs : EventArgs
    {
        public string Username { get; private set; }

        public KickPlayerArgs(string username)
        {
            Username = username;
        }
    }
}
