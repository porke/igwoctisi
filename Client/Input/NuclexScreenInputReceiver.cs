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
            return UseModalBehavior;
        }

        // keyboard
        public virtual bool OnKeyPressed(Keys key)
        {
            Screen.InjectKeyPress(key);
            return UseModalBehavior;
        }
        public virtual bool OnKeyReleased(Keys key)
        {
            Screen.InjectKeyRelease(key);
            return UseModalBehavior;
        }

        // mouse
        public virtual bool OnMouseMoved(Vector2 position)
        {
            Screen.InjectMouseMove(position.X, position.Y);
            return UseModalBehavior;
        }
        public virtual bool OnMousePressed(MouseButtons button)
        {
            Screen.InjectMousePress(button);
            return UseModalBehavior;
        }
        public virtual bool OnMouseReleased(MouseButtons button)
        {
            Screen.InjectMouseRelease(button);
            return UseModalBehavior;
        }
        public virtual bool OnMouseWheel(float ticks)
        {
            Screen.InjectMouseWheel(ticks);
            return UseModalBehavior;
        }

        // gamepad
        public virtual bool OnButtonPressed(Buttons button)
        {
            Screen.InjectButtonPress(button);
            return UseModalBehavior;
        }
        public virtual bool OnButtonReleased(Buttons button)
        {
            Screen.InjectButtonRelease(button);
            return UseModalBehavior;
        }

        #endregion

        public Screen Screen { get; protected set; }
        public bool UseModalBehavior { get; protected set; }

        public NuclexScreenInputReceiver(Screen screen, bool useModalBehavior)
        {
            Screen = screen;
            UseModalBehavior = useModalBehavior;
        }
    }
}
