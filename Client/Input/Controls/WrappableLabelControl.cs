namespace Client.Input.Controls
{
    using Nuclex.UserInterface.Controls;
    using Nuclex.UserInterface.Visuals.Flat;

    class WrappableLabelControl : LabelControl
    {
        public new string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                // TODO: Exception, the text cannot be set before setting the bounds
                var width = base.GetAbsoluteBounds().Width;
                base.Text = value;
            }
        }
    }
}
