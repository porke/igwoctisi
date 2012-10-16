namespace Client.Input
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;
    using Nuclex.Input;
    using Nuclex.UserInterface.Input;

    public class XnaInput : IInput
    {
        #region Protected members

        protected KeyboardState _oldKeyboardState;
        protected MouseState _oldMouseState;

        #endregion

        #region IInput members

        public void Initialize(GameClient client)
        {
            Client = client;

            _oldKeyboardState = new KeyboardState();
        }
        public void Release()
        {
        }
        public void Update(double delta, double time)
        {
            var receiver = Client.State.ViewMgr.InputReceiver;
            if (receiver == null) return;

            var newKeyboardState = Keyboard.GetState();
            var newMouseState = Mouse.GetState();

            #region Commands

            if (newKeyboardState.IsKeyDown(Keys.Enter) && _oldKeyboardState.IsKeyUp(Keys.Enter))
                receiver.OnCommand(Command.Accept);
            if (newKeyboardState.IsKeyDown(Keys.Escape) && _oldKeyboardState.IsKeyUp(Keys.Escape))
                receiver.OnCommand(Command.Cancel);
            if (newKeyboardState.IsKeyDown(Keys.Up) && _oldKeyboardState.IsKeyUp(Keys.Up))
                receiver.OnCommand(Command.Up);
            if (newKeyboardState.IsKeyDown(Keys.Down) && _oldKeyboardState.IsKeyUp(Keys.Down))
                receiver.OnCommand(Command.Down);
            if (newKeyboardState.IsKeyDown(Keys.Left) && _oldKeyboardState.IsKeyUp(Keys.Left))
                receiver.OnCommand(Command.Left);
            if (newKeyboardState.IsKeyDown(Keys.Right) && _oldKeyboardState.IsKeyUp(Keys.Right))
                receiver.OnCommand(Command.Right);

            #endregion

            #region Keyboard

            foreach (var keyObj in Enum.GetValues(typeof(Keys)))
            {
                var key = (Keys) keyObj;
                if (newKeyboardState.IsKeyDown(key) != _oldKeyboardState.IsKeyDown(key))
                {
                    if (newKeyboardState.IsKeyDown(key))
                    {
                        receiver.OnKeyPressed(key);

                        char character = (char)key;
                        if (character >= 65 && character <= 90)
                        {
                            if (!newKeyboardState.IsKeyDown(Keys.LeftShift) &&
                            !newKeyboardState.IsKeyDown(Keys.RightShift))
                            {
                                character += (char)32;
                            }

                            receiver.OnCharacter(character);
                        }
                    }
                    else
                    {
                        receiver.OnKeyReleased(key);
                    }
                }
            }
            _oldKeyboardState = newKeyboardState;

            #endregion

            #region Mouse

            if (newMouseState.X != _oldMouseState.X || newMouseState.Y != _oldMouseState.Y)
                receiver.OnMouseMoved(new Vector2(newMouseState.X, newMouseState.Y));
            if (newMouseState.ScrollWheelValue != _oldMouseState.ScrollWheelValue)
                receiver.OnMouseWheel(newMouseState.ScrollWheelValue - _oldMouseState.ScrollWheelValue);

            if (newMouseState.LeftButton != _oldMouseState.LeftButton)
            {
                if (newMouseState.LeftButton == ButtonState.Pressed)
                {
                    receiver.OnMousePressed(MouseButtons.Left);
                }
                else
                {
                    receiver.OnMouseReleased(MouseButtons.Left);
                }
            }
            if (newMouseState.RightButton != _oldMouseState.RightButton)
            {
                if (newMouseState.RightButton == ButtonState.Pressed)
                {
                    receiver.OnMousePressed(MouseButtons.Right);
                }
                else
                {
                    receiver.OnMouseReleased(MouseButtons.Right);
                }
            }
            if (newMouseState.MiddleButton != _oldMouseState.MiddleButton)
            {
                if (newMouseState.MiddleButton == ButtonState.Pressed)
                {
                    receiver.OnMousePressed(MouseButtons.Middle);
                }
                else
                {
                    receiver.OnMouseReleased(MouseButtons.Middle);
                }
            }
            if (newMouseState.XButton1 != _oldMouseState.XButton1)
            {
                if (newMouseState.XButton1 == ButtonState.Pressed)
                {
                    receiver.OnMousePressed(MouseButtons.X1);
                }
                else
                {
                    receiver.OnMouseReleased(MouseButtons.X1);
                }
            }
            if (newMouseState.XButton2 != _oldMouseState.XButton2)
            {
                if (newMouseState.XButton2 == ButtonState.Pressed)
                {
                    receiver.OnMousePressed(MouseButtons.X2);
                }
                else
                {
                    receiver.OnMouseReleased(MouseButtons.X2);
                }
            }
            _oldMouseState = newMouseState;

            #endregion
        }

        #endregion

        public GameClient Client { get; protected set; }
    }
}
