namespace Client.Input.Controls
{
    using System;
    using Nuclex.UserInterface.Controls.Desktop;
    using Nuclex.UserInterface.Input;

    public class CommandInputControl : InputControl
    {
        public event EventHandler OnCommandHandler;

        protected override bool OnCommand(Command command)
        {
            if (OnCommandHandler != null 
                && command.HasFlag(Command.Accept))
            {
                OnCommandHandler(this, null);
            }

            return base.OnCommand(command);
        }
    }
}
