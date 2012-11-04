namespace Client.Input.Controls
{
    using System;
    using System.ComponentModel;
    using System.Text;
    using Nuclex.UserInterface.Controls;

    public class WrappableLabelControl : LabelControl
    {
        public bool EnableWrapping { get; set; }
        public int CharacterWidthInPx { get; set; }

        public WrappableLabelControl()
        {
            EnableWrapping = true;
            CharacterWidthInPx = DefaultCharacterWidthInPx;
        }

        public new string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                if (EnableWrapping)
                {
                    if (Bounds == null)
                    {
                        throw new InvalidOperationException("The LabelControl Bounds property must be set.");
                    }

                    var finalText = new StringBuilder();
                    var labelAbsoluteWidth = base.GetAbsoluteBounds().Width;
                    var words = value.Split(' ');
                    int currWidth = 0;
                    foreach (var word in words)
                    {
                        int currentWordLen = word.Length * CharacterWidthInPx;
                        if (currWidth + currentWordLen > labelAbsoluteWidth)
                        {
                            currWidth = 0;
                            finalText.Append("\n");
                        }

                        currWidth += currentWordLen;
                        finalText.Append(" ");
                        finalText.Append(word);
                    }

                    base.Text = finalText.ToString();
                }
                else
                {
                    base.Text = value;
                }
            }
        }

        private const int DefaultCharacterWidthInPx = 8;
    }    
}
