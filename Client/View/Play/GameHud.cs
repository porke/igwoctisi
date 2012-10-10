namespace Client.View.Play
{
    using System;
    using Common;
    using Input;
    using Nuclex.UserInterface;
    using Nuclex.UserInterface.Controls;
    using Nuclex.UserInterface.Controls.Desktop;
    using State;

    class GameHud : BaseView
    {
        #region Protected members

        private ListControl _orderList;
        private LabelControl _fleetCount;
        private LabelControl _fleetIncomeCount;

        protected void CreateChildControls()
        {
            var ordersHeader = new LabelControl("Orders")
            {                
                Bounds = new UniRectangle(new UniScalar(0.78f, 0), new UniScalar(0.22f, 0), new UniScalar(0.34f, 0), new UniScalar(0.1f, 0))
            };

            _orderList = new ListControl()
            {
                SelectionMode = ListSelectionMode.Single,
                Bounds = new UniRectangle(new UniScalar(0.65f, 0), new UniScalar(0.3f, 0), new UniScalar(0.34f, 0), new UniScalar(0.45f, 0))
            };
            _orderList.Items.Add("Move 2 fleets from Alpha to Beta");
            _orderList.Items.Add("Deploy 2 fleets on Gamma");

            _fleetCount = new LabelControl("Deployable fleets: 4")
            {
                Bounds = new UniRectangle(new UniScalar(0.05f, 0), new UniScalar(0.0f, 0), new UniScalar(0.3f, 0), new UniScalar(0.1f, 0))
            };

            _fleetIncomeCount = new LabelControl("Fleets per turn: 5")
            {                
                Bounds = new UniRectangle(new UniScalar(0.4f, 0), new UniScalar(0.0f, 0), new UniScalar(0.3f, 0), new UniScalar(0.1f, 0))
            };

            var btnLeaveGame = new ButtonControl()
            {
                Text = "Leave",
                Bounds = new UniRectangle(new UniScalar(0.02f, 0), new UniScalar(0.93f, 0), new UniScalar(0.1f, 0), new UniScalar(0.05f, 0))
            };
            btnLeaveGame.Pressed += LeaveGame_Pressed;

            var btnDeleteOrder = new ButtonControl()
            {
                Text = "Delete",
                Bounds = new UniRectangle(new UniScalar(0.65f, 0), new UniScalar(0.77f, 0), new UniScalar(0.15f, 0), new UniScalar(0.05f, 0))
            };

            screen.Desktop.Children.AddRange(new Control[] {ordersHeader, _orderList, _fleetIncomeCount, _fleetCount, btnDeleteOrder, btnLeaveGame});
        }

        private void LeaveGame_Pressed(object sender, EventArgs e)
        {
            state.HandleViewEvent("LeaveGame", e);
        }

        #endregion

        public GameHud(PlayState state) : base(state)
        {
            IsLoaded = true;
            IsTransparent = true;
            InputReceiver = new NuclexScreenInputReceiver(screen, false);

            CreateChildControls();
        }
    }
}
