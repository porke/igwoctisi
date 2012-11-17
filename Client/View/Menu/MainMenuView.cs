namespace Client.View.Menu
{
	using System;
	using System.Diagnostics;
	using System.Linq;
	using Common;
	using Input;
	using Nuclex.UserInterface;
	using Nuclex.UserInterface.Controls;
	using Nuclex.UserInterface.Controls.Desktop;
	using State;
	using View.Controls;

	class MainMenuView : BaseView
    {
		public event EventHandler<EventArgs<string, string>> LoginPressed;
		public event EventHandler<EventArgs<string, string>> EnterPlayStatePressed;
		public event EventHandler<EventArgs> QuitPressed;

        #region Protected members

        protected CommandInputControl tbLogin;
        protected PasswordInputControl tbPassword;
		protected ButtonControl btnLogin, btnQuit;

		private string GetOSUniqueUsername()
		{
			var currentProcess = Process.GetCurrentProcess();
			int otherProcessesCount = Process.GetProcessesByName(currentProcess.ProcessName).Count() - 1;

			string nickname = Environment.UserName;

			if (otherProcessesCount > 0)
			{
				nickname += (otherProcessesCount + 1).ToString();
			}

			return nickname;
		}

        protected void CreateChildControls()
        {
            tbLogin = new CommandInputControl
            {
				Text = GetOSUniqueUsername(),
                Bounds = new UniRectangle(new UniScalar(0.29f, 0), new UniScalar(0.4f, 0), new UniScalar(0.42f, 0), new UniScalar(0.05f, 0))                
            };
            tbLogin.OnCommandHandler += Login_Pressed;

            tbPassword = new PasswordInputControl
            {
                Bounds = new UniRectangle(new UniScalar(0.29f, 0), new UniScalar(0.5f, 0), new UniScalar(0.42f, 0), new UniScalar(0.05f, 0))
            };
            tbPassword.SetPassword("pswd");
            tbPassword.OnCommandHandler += Login_Pressed;

            btnLogin = new ButtonControl
            {
                Text = "Login",
                Bounds = new UniRectangle(new UniScalar(0.29f, 0), new UniScalar(0.6f, 0), new UniScalar(0.2f, 0), new UniScalar(0.05f, 0))
            };
            btnLogin.Pressed += Login_Pressed;

            btnQuit = new ButtonControl
            {
                Text = "Quit",
                Bounds = new UniRectangle(new UniScalar(0.51f, 0), new UniScalar(0.6f, 0), new UniScalar(0.2f, 0), new UniScalar(0.05f, 0))
            };
            btnQuit.Pressed += Quit_Pressed;


            var btnEnterPlayState = new ButtonControl
            {
                Text = "Join or Create Game",
                Bounds = new UniRectangle(new UniScalar(0.29f, 0), new UniScalar(0.6f, 40), new UniScalar(0.2f, 0), new UniScalar(0.05f, 0))
            };
            btnEnterPlayState.Pressed += EnterPlayState_Pressed;

            screen.Desktop.Children.AddRange(new Control[] { tbLogin, tbPassword, btnLogin, btnQuit});
#if DEBUG
            screen.Desktop.Children.Add(btnEnterPlayState);
#endif
        }

        #endregion

        #region Event handlers

        protected void Login_Pressed(object sender, EventArgs e)
        {
			if (LoginPressed != null)
			{
				LoginPressed(sender, LoginPressed.CreateArgs(tbLogin.Text, tbPassword.GetPassword()));
			}			
        }
        protected void Quit_Pressed(object sender, EventArgs e)
        {
			if (QuitPressed != null)
			{
				QuitPressed(sender, EventArgs.Empty);				
			}			
        }
        protected void EnterPlayState_Pressed(object sender, EventArgs e)
        {
			if (EnterPlayStatePressed != null)
			{
				EnterPlayStatePressed(sender, EnterPlayStatePressed.CreateArgs(tbLogin.Text, tbPassword.GetPassword()));
			}
        }
        #endregion
	

		public MainMenuView(MenuState state)
			: base(state)
        {
            IsTransparent = true;
            InputReceiver = new NuclexScreenInputReceiver(screen, false);

            CreateChildControls();
			State = ViewState.Loaded;
        }
    }
}
