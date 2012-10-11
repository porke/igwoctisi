namespace Client.View.Play
{
    using System;
    using Common;
    using Input;
    using Nuclex.UserInterface;
    using Nuclex.UserInterface.Controls;
    using Nuclex.UserInterface.Controls.Desktop;
    using State;
    using System.Collections.Generic;

    class GameHud : BaseView
    {
        #region Protected members

        private ListControl _orderList;
        private LabelControl _fleetCountValue;
        private LabelControl _fleetIncomeValue;

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
            _orderList.SelectedItems.Add(0);

            var fleetCountDesc = new LabelControl("Deployable fleets:")
            {
                Bounds = new UniRectangle(new UniScalar(0.01f, 0), new UniScalar(0.0f, 0), new UniScalar(0.3f, 0), new UniScalar(0.1f, 0))
            };

            _fleetCountValue = new LabelControl("4")
            {
                Bounds = new UniRectangle(new UniScalar(0.175f, 0), new UniScalar(0.0f, 0), new UniScalar(0.3f, 0), new UniScalar(0.1f, 0))
            };

            var fleetIncomeDesc = new LabelControl("Fleets per turn:")
            {
                Bounds = new UniRectangle(new UniScalar(0.01f, 0), new UniScalar(0.05f, 0), new UniScalar(0.3f, 0), new UniScalar(0.1f, 0))
            };

            _fleetIncomeValue = new LabelControl("5")
            {                
                Bounds = new UniRectangle(new UniScalar(0.175f, 0), new UniScalar(0.05f, 0), new UniScalar(0.3f, 0), new UniScalar(0.1f, 0))
            };

            var btnLeaveGame = new ButtonControl()
            {
                Text = "Leave",
                Bounds = new UniRectangle(new UniScalar(0.02f, 0), new UniScalar(0.93f, 0), new UniScalar(0.1f, 0), new UniScalar(0.05f, 0))
            };
            btnLeaveGame.Pressed += LeaveGame_Pressed;

            var btnSendOrders = new ButtonControl()
            {
                Text = "End turn",
                Bounds = new UniRectangle(new UniScalar(0.14f, 0), new UniScalar(0.93f, 0), new UniScalar(0.1f, 0), new UniScalar(0.05f, 0))
            };
            btnSendOrders.Pressed += SendOrders_Pressed;

            var btnMoveOrderUp = new ButtonControl()
            {
                Text = "Up",
                Bounds = new UniRectangle(new UniScalar(0.65f, 0), new UniScalar(0.77f, 0), new UniScalar(0.1f, 0), new UniScalar(0.05f, 0))
            };
            btnMoveOrderUp.Pressed += MoveOrderUp_Pressed;

            var btnMoveOrderDown = new ButtonControl()
            {
                Text = "Down",
                Bounds = new UniRectangle(new UniScalar(0.77f, 0), new UniScalar(0.77f, 0), new UniScalar(0.1f, 0), new UniScalar(0.05f, 0))
            };
            btnMoveOrderDown.Pressed += MoveOrderDown_Pressed;

            var btnDeleteOrder = new ButtonControl()
            {
                Text = "Delete",
                Bounds = new UniRectangle(new UniScalar(0.89f, 0), new UniScalar(0.77f, 0), new UniScalar(0.1f, 0), new UniScalar(0.05f, 0))
            };
            btnDeleteOrder.Pressed += DeleteOrder_Pressed;

            screen.Desktop.Children.AddRange(
                new Control[] 
                {
                    ordersHeader, 
                    _orderList, 
                    _fleetIncomeValue, 
                    _fleetCountValue, 
                    fleetCountDesc,
                    fleetIncomeDesc,
                    btnDeleteOrder, 
                    btnLeaveGame, 
                    btnSendOrders,
                    btnMoveOrderDown,
                    btnMoveOrderUp
                });
        }

        private void LeaveGame_Pressed(object sender, EventArgs e)
        {
            state.HandleViewEvent("LeaveGame", e);
        }

        private void SendOrders_Pressed(object sender, EventArgs e)
        {
            state.HandleViewEvent("SendOrders", e);
        }

        private void MoveOrderUp_Pressed(object sender, EventArgs e)
        {

        }

        private void MoveOrderDown_Pressed(object sender, EventArgs e)
        {

        }

        private void DeleteOrder_Pressed(object sender, EventArgs e)
        {
            if (_orderList.SelectedItems.Count > 0)
            {
                var firstItem = _orderList.SelectedItems[0];
                _orderList.Items.RemoveAt(firstItem);
                _orderList.SelectedItems.RemoveAt(0);

                if (_orderList.Items.Count > 0)
                {                    
                    _orderList.SelectedItems.Add(firstItem - 1);
                }

                // TODO update the logic (fleet count, etc.)
            }
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
