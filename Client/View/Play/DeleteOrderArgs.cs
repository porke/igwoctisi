namespace Client.View.Play
{
    using System;

    class DeleteCommandArgs : EventArgs
    {
        public int OrderListIndex { get; private set; }

        public DeleteCommandArgs(int orderListIndex)
        {
            OrderListIndex = orderListIndex;
        }
    }
}
