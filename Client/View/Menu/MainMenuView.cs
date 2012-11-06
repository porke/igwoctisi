namespace Client.View.Menu
{
    using System;
    using Common;
    using Input;
    using Input.Controls;
    using Nuclex.UserInterface;
    using Nuclex.UserInterface.Controls;
    using Nuclex.UserInterface.Controls.Desktop;
    using State;

	class MainMenuView : BaseView
    {
        #region Protected members

        protected CommandInputControl tbLogin;
        protected PasswordInputControl tbPassword;
		protected ButtonControl btnLogin, btnQuit;

        protected void CreateChildControls()
        {
            tbLogin = new CommandInputControl
            {
                Text = "infinite",
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

            screen.Desktop.Children.AddRange(new Control[] { tbLogin, tbPassword, btnLogin, btnQuit});
        }

        #endregion

        #region Event handlers

        protected void Login_Pressed(object sender, EventArgs e)
        {
			MenuState.RequestLogin(tbLogin.Text, tbPassword.GetPassword());
        }
        protected void Quit_Pressed(object sender, EventArgs e)
        {
			MenuState.QuitGame();
        }

        #endregion

		public MenuState MenuState { get; protected set; }

		public MainMenuView(MenuState state)
			: base(state)
        {
			MenuState = state;
            IsTransparent = true;
            InputReceiver = new NuclexScreenInputReceiver(screen, false);

            CreateChildControls();
			State = ViewState.Loaded;
        }
    }
}
