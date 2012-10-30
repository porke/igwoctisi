namespace Client.Input.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Client.Common;
    using Nuclex.UserInterface.Controls.Desktop;
    using System.Linq;

    public class WrappableListControl : ListControl
    {
        public new IList<int> SelectedItems
        {
            get
            {
                var selectedItems = new List<int>();
                base.SelectedItems.OrderBy(elem => elem);
                return selectedItems;
            }
        }
        public new IList<string> Items 
        { 
            get
            {
                string[] itemsCopy = new string[_virtualItems.Count];
                _virtualItems.ToArray().CopyTo(itemsCopy, 0);
                return new List<string>(itemsCopy);
            }
        }
        public int CharacterWidthInPx { get; set; }

        public WrappableListControl()
        {
            CharacterWidthInPx = DefaultCharacterWidthInPx;
            base.SelectionMode = ListSelectionMode.Multi;
            _virtualItems = new List<Tuple<int, int>>();
        }

        public void AddItems(string[] items)
        {
            foreach (var item in items)
            {
                AddItem(item);
            }
        }

        public void AddItem(string item)
        {
            if (Bounds == null) throw new InvalidOperationException("The ListControl Bounds property must be set.");
            if (Parent == null) throw new InvalidOperationException("The ListControl has to belong to a Desktop.");

            var finalText = new StringBuilder();
            var listAbsoluteWidth = GetAbsoluteBounds().Width - Slider.GetAbsoluteBounds().Width;
            var words = item.Split(' ');
            int currWidth = 0;
            foreach (var word in words)
            {
                // Include space character, hence + 1
                int currentWordLen = (word.Length + 1) * CharacterWidthInPx;
                if (currWidth + currentWordLen > listAbsoluteWidth)
                {
                    currWidth = 0;                    
                    finalText.Append("\n");
                }
                
                currWidth += currentWordLen;
                finalText.Append(word);
                finalText.Append(' ');
            }

            var splitItems = finalText.ToString().Trim().Split('\n');

            _virtualItems.Add(Tuple.Create<int, int>(base.Items.Count, splitItems.Length));
            base.Items.AddRange(splitItems);
        }

        public void DeleteItem(int itemIndex)
        {
            var itemToRemove = _virtualItems.Find(item => itemIndex >= item.Item1 && itemIndex < item.Item1 + item.Item2);
            if (itemToRemove != null)
            {
                for (int i = 0; i < itemToRemove.Item2; ++i)
                {
                    base.Items.RemoveAt(itemToRemove.Item1);
                }

                _virtualItems.RemoveAt(itemIndex);

                if (itemIndex < _virtualItems.Count)
                {
                    OnRowClicked(itemIndex);
                }
            }
        }

        public string GetItem(int itemIndex)
        {
            var itemString = new StringBuilder();
            int startIndex = _virtualItems[itemIndex].Item1;
            int endIndex = _virtualItems[itemIndex].Item1 + _virtualItems[itemIndex].Item2;
            for (int i = startIndex; i < endIndex; ++i)
            {
                itemString.Append(base.Items[i]);
                itemString.Append(' ');
            }

            return itemString.ToString().Trim();
        }

        public int GetItemCount()
        {
            return _virtualItems.Count;
        }

        public void Clear()
        {
            _virtualItems.Clear();
            base.Items.Clear();
            SelectedItems.Clear();
        }

        public void SelectItem(int item)
        {
            if (item >= 0 && item < _virtualItems.Count)
            {
                OnRowClicked(item);
            }
        }

        protected override void OnRowClicked(int row)
        {
            SelectedItems.Clear();
            var selectedItem = _virtualItems.Find(item => row >= item.Item1 && row < item.Item1 + item.Item2);
            for (int i = selectedItem.Item1; i < selectedItem.Item1 + selectedItem.Item2; ++i)
            {
                SelectedItems.Add(i);
            }
        }

        private List<Tuple<int, int>> _virtualItems = new List<Tuple<int, int>>();
        private const int DefaultCharacterWidthInPx = 8;
    }
}
