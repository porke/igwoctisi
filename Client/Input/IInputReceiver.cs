namespace Client.Input
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;
    using Nuclex.Input;
    using Nuclex.UserInterface.Input;

    public interface IInputReceiver
    {
        // commands
        bool OnCommand(Command command);

        // keyboard
        bool OnKeyPressed(Keys key);
        bool OnKeyReleased(Keys key);
        bool OnCharacter(char character);

        // mouse
        bool OnMouseMoved(Vector2 position);
        bool OnMousePressed(MouseButtons button);
        bool OnMouseReleased(MouseButtons button);
        bool OnMouseWheel(float ticks);

        // gamepad
        bool OnButtonPressed(Buttons button);
        bool OnButtonReleased(Buttons button);
    }
}
