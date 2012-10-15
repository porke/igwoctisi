using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client.Common
{
    public static class Utils
    {
        public static string LowerFirstLetter(string str)
        {
            return char.ToLower(str[0]) + str.Substring(1);
        }

        public static string UpperFirstLetter(string str)
        {
            return char.ToUpper(str[0]) + str.Substring(1);
        }
    }
}
