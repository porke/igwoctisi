namespace Client.Input.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Client.Common;
    using Nuclex.UserInterface.Controls.Desktop;

    class WrappableListControl : ListControl
    {
        public new IList<string> Items { private get; set; }

        public int CharacterWidthInPx { get; set; }

        public WrappableListControl()
        {
            CharacterWidthInPx = DefaultCharacterWidthInPx;
        }

        public void AddItem(string item)
        {
            if (Bounds == null)
            {
                throw new InvalidOperationException("The ListControl Bounds property must be set.");
            }

            var finalText = new StringBuilder();
            var labelAbsoluteWidth = GetAbsoluteBounds().Width - Slider.GetAbsoluteBounds().Width;
            var words = item.Split(' ');
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

            base.Items.AddRange(finalText.ToString().Split('\n'));
        }

        public void RemoveItem(int itemIndex)
        {
            base.Items.RemoveAt(itemIndex);
        }

        public string GetItem(int itemIndex)
        {
            return base.Items[itemIndex];
        }

        public int GetItemCount()
        {
            return base.Items.Count;
        }

        public void Clear()
        {
            base.Items.Clear();
        }
        
        private const int DefaultCharacterWidthInPx = 8;
    }
}
