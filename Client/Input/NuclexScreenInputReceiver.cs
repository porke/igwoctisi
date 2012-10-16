namespace Client.Input
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;
    using Nuclex.Input;
    using Nuclex.UserInterface;
    using Nuclex.UserInterface.Input;

    public class NuclexScreenInputReceiver : IInputReceiver
    {
        #region IInputReceiver members

        // commands
        public virtual bool OnCommand(Command command)
        {
            Screen.InjectCommand(command);
            return IsInputHandled;
        }

        // keyboard
        public virtual bool OnKeyPressed(Keys key)
        {
            Screen.InjectKeyPress(key);
            return IsInputHandled;
        }
        public virtual bool OnKeyReleased(Keys key)
        {
            Screen.InjectKeyRelease(key);
            return IsInputHandled;
        }
        public virtual bool OnCharacter(char character)
        {
            Screen.InjectCharacter(character);
            return IsInputHandled;
        }

        // mouse
        public virtual bool OnMouseMoved(Vector2 position)
        {
            Screen.InjectMouseMove(position.X, position.Y);
            return IsInputHandled;
        }
        public virtual bool OnMousePressed(MouseButtons button)
        {
            Screen.InjectMousePress(button);
            return IsInputHandled;
        }
        public virtual bool OnMouseReleased(MouseButtons button)
        {
            Screen.InjectMouseRelease(button);
            return IsInputHandled;
        }
        public virtual bool OnMouseWheel(float ticks)
        {
            Screen.InjectMouseWheel(ticks);
            return IsInputHandled;
        }

        // gamepad
        public virtual bool OnButtonPressed(Buttons button)
        {
            Screen.InjectButtonPress(button);
            return IsInputHandled;
        }
        public virtual bool OnButtonReleased(Buttons button)
        {
            Screen.InjectButtonRelease(button);
            return IsInputHandled;
        }

        #endregion

        public Screen Screen { get; protected set; }
        public bool UseModalBehavior { get; protected set; }
        private bool IsInputHandled
        {
            get
            {
                return Screen.IsInputCaptured || UseModalBehavior;
            }
        }

        public NuclexScreenInputReceiver(Screen screen, bool useModalBehavior)
        {
            Screen = screen;
            UseModalBehavior = useModalBehavior;
        }
    }
}
