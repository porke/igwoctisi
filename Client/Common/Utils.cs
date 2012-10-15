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
