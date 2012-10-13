using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client.Common
{
    public static class Utils
    {
        public static string lowerFirstLetter(string str)
        {
            return char.ToLower(str[0]) + str.Substring(1);
        }
    }
}
