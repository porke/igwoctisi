namespace Client.Input.Controls
{
    using System;
    using Microsoft.Xna.Framework.Input;
    using Nuclex.UserInterface.Controls.Desktop;
    using Nuclex.UserInterface.Input;

    public class PasswordInputControl : CommandInputControl
    {
        private String _value;
        public event EventHandler Activated;

        public PasswordInputControl()
        {
        }

        public string GetPassword()
        {
            return _value;
        }

        public void SetPassword(String password)
        {
            _value = password;
            base.Text = new String('*', _value.Length);
        }

        protected override void OnCharacterEntered(char character)
        {
            if (character == '\n' || character == '\r')
            {
                OnActivate();
                return;
            }
            if (this.HasFocus &&
               (Char.IsLetter(character) || Char.IsNumber(character) || Char.IsPunctuation(character) || Char.IsSeparator(character)))
            {
                _value += character;
                this.Text = String.Empty;
                for (int i = 0; i < _value.Length; i++)
                    this.Text += "*";
                this.CaretPosition = this.Text.Length;
            }

        }

        protected override void OnKeyReleased(Microsoft.Xna.Framework.Input.Keys key)
        {
            if (!HasFocus) return;
            if (!String.IsNullOrEmpty(_value)
                && (Keys.Back == key || Keys.Delete == key))
            {
                int oldIndex = this.CaretPosition;
                _value = RemoveIndex(_value.ToCharArray(), this.CaretPosition);
                this.Text = string.Empty;
                for (int i = 0; i < _value.Length; i++)
                    this.Text += "*";
                this.CaretPosition = oldIndex;
                return;
            }
        }

        private String RemoveIndex(Char[] IndicesArray, int RemoveAt)
        {
            Char[] newIndicesArray = new char[IndicesArray.Length - 1];

            int i = 0;
            int j = 0;
            while (i < IndicesArray.Length)
            {
                if (i != RemoveAt)
                {
                    newIndicesArray[j] = IndicesArray[i];
                    j++;
                }

                i++;
            }

            return new string(newIndicesArray);
        }

        /// <summary>
        /// Call our delegate and clear text
        /// </summary>
        protected void OnActivate()
        {
            if (Activated != null)
            {
                Activated(this, EventArgs.Empty);
                Text = string.Empty;
            }
        }        
    }
}
