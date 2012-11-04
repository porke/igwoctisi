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
            var receiver = Client.ViewMgr.InputReceiver;
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
						bool shiftOn = newKeyboardState.IsKeyDown(Keys.LeftShift) || newKeyboardState.IsKeyDown(Keys.RightShift);
                        if (character >= 65 && character <= 90)
                        {
                            if (!shiftOn)
                            {
                                character += (char)32;
                            }

                            receiver.OnCharacter(character);
                        }
						else if (character >= 48 && character <= 57 && !shiftOn)
						{
							receiver.OnCharacter(character);
						}

						if (key == Keys.Space) receiver.OnCharacter(' ');					// 32
						if (key == Keys.D1 && shiftOn) receiver.OnCharacter('!');			// 33
						if (key == Keys.OemQuotes && shiftOn) receiver.OnCharacter('"');	// 34
						if (key == Keys.D3 && shiftOn) receiver.OnCharacter('#');			// 35
						if (key == Keys.D4 && shiftOn) receiver.OnCharacter('$');			// 36
						if (key == Keys.D5 && shiftOn) receiver.OnCharacter('%');			// 37
						if (key == Keys.D7 && shiftOn) receiver.OnCharacter('&');			// 38
                        if (key == Keys.OemQuotes && !shiftOn) receiver.OnCharacter('\'');	// 39
						if (key == Keys.D9 && shiftOn) receiver.OnCharacter('(');			// 40
						if (key == Keys.D0 && shiftOn) receiver.OnCharacter(')');			// 41
						if (key == Keys.D8 && shiftOn) receiver.OnCharacter('*');			// 42
						if (key == Keys.Multiply) receiver.OnCharacter('*');
						if (key == Keys.OemPlus && shiftOn) receiver.OnCharacter('+');		// 43
						if (key == Keys.Add) receiver.OnCharacter('+');
						if (key == Keys.OemComma && !shiftOn) receiver.OnCharacter(',');	// 44
                        if (key == Keys.OemMinus && !shiftOn) receiver.OnCharacter('-');	// 45
						if (key == Keys.Subtract) receiver.OnCharacter('-');
						if (key == Keys.OemPeriod && !shiftOn) receiver.OnCharacter('.');	// 46
						if (key == Keys.Decimal) receiver.OnCharacter('.');
						if (key == Keys.OemQuestion && !shiftOn) receiver.OnCharacter('/');	// 47

						if (key == Keys.OemSemicolon && shiftOn) receiver.OnCharacter(':');	// 58
						if (key == Keys.OemSemicolon && !shiftOn) receiver.OnCharacter(';');// 59
						if (key == Keys.OemComma && shiftOn) receiver.OnCharacter('<');		// 60
						if (key == Keys.OemPlus && !shiftOn) receiver.OnCharacter('=');		// 61
						if (key == Keys.OemPeriod && shiftOn) receiver.OnCharacter('>');	// 62
                        if (key == Keys.OemQuestion && shiftOn) receiver.OnCharacter('?');	// 63
						if (key == Keys.D2 && shiftOn) receiver.OnCharacter('@');			// 64

						if (key == Keys.OemOpenBrackets && !shiftOn) receiver.OnCharacter('[');	// 91
						if (key == Keys.OemPipe && !shiftOn) receiver.OnCharacter('\\');		// 92
						if (key == Keys.OemCloseBrackets && !shiftOn) receiver.OnCharacter(']');// 93
						if (key == Keys.D6 && shiftOn) receiver.OnCharacter('^');				// 94
						if (key == Keys.OemMinus && shiftOn) receiver.OnCharacter('_');			// 95
						if (key == Keys.OemTilde && !shiftOn) receiver.OnCharacter('`');		// 96

						if (key == Keys.OemOpenBrackets && shiftOn) receiver.OnCharacter('{');	// 123
						if (key == Keys.OemPipe && shiftOn) receiver.OnCharacter('|');			// 124
						if (key == Keys.OemCloseBrackets && shiftOn) receiver.OnCharacter('}');	// 125
                        if (key == Keys.OemTilde && shiftOn) receiver.OnCharacter('~');			// 126
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
