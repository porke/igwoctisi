namespace Client.View
{
    using System;

    class SenderEventArgs : EventArgs
    {
        public BaseView Sender { get; private set; }

        public SenderEventArgs(BaseView sender)
        {
            Sender = sender;
        }
    }
}
