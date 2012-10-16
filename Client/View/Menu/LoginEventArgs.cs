namespace Client.View.Menu
{
    using System;

    class LoginEventArgs : EventArgs
    {
        public string Login { get; private set; }
        public string Password { get; private set; }

        public LoginEventArgs(string login, string password)
        {
            Login = login;
            Password = password;
        }
    }
}
