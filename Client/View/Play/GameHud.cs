namespace Client.View.Play
{
    using Common;
    using Input;
    using Nuclex.UserInterface;
    using Nuclex.UserInterface.Controls;
    using Nuclex.UserInterface.Controls.Desktop;
    using State;

    class GameHud : IView
    {
        #region Protected members

        protected Screen _screen;

        private ListControl _ordersList;
        private LabelControl _fleetCount;
        private LabelControl _fleetIncomeCount;

        protected void CreateChildControls()
        {
            var ordersHeader = new LabelControl("Orders")
            {
                Bounds = new UniRectangle()
            };

            _ordersList = new ListControl()
            {
                Bounds = new UniRectangle()
            };

            _fleetCount = new LabelControl()
            {
                Bounds = new UniRectangle()
            };

            _fleetIncomeCount = new LabelControl()
            {
                Bounds = new UniRectangle()
            };

            _screen.Desktop.Children.AddRange(new Control[] {ordersHeader, _ordersList, _fleetIncomeCount, _fleetCount});
        }

        #endregion

        #region IView members

        public bool IsLoaded
        {
            get { return true; }
        }
        public bool IsTransparent
        {
            get { return true; }
        }
        public IInputReceiver InputReceiver { get; protected set; }

        public void OnShow(ViewManager viewMgr, double time)
        {
            ViewMgr = viewMgr;
        }
        public void OnHide(double time)
        {
        }
        public void Update(double delta, double time)
        {
        }
        public void Draw(double delta, double time)
        {
        }

        #endregion

        public PlayState State { get; protected set; }
        public ViewManager ViewMgr { get; protected set; }

        public GameHud(PlayState state)
        {
            State = state;
            _screen = new Screen(800, 600);
            InputReceiver = new NuclexScreenInputReceiver(_screen, false);

            CreateChildControls();
        }
    }
}
