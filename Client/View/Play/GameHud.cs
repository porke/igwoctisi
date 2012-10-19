namespace Client.View.Play
{
    using System;
    using Client.Model;
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
        private LabelControl _playerNameValue;
        private LabelControl _fleetCountValue;
        private LabelControl _fleetIncomeValue;
        
        private LabelControl _selectedPlanetName;
        private LabelControl _selectedPlanetBaseIncome;
        private LabelControl _selectedPlanetFleetCount;

        private LabelControl _timer;

        protected void CreateChildControls()
        {
            #region Orders section

            var ordersHeader = new LabelControl("Orders")
            {                
                Bounds = new UniRectangle(new UniScalar(0.78f, 0), new UniScalar(0.22f, 0), new UniScalar(0.34f, 0), new UniScalar(0.1f, 0))
            };

            _orderList = new ListControl()
            {
                SelectionMode = ListSelectionMode.Single,
                Bounds = new UniRectangle(new UniScalar(0.65f, 0), new UniScalar(0.3f, 0), new UniScalar(0.34f, 0), new UniScalar(0.45f, 0))
            };

            #endregion

            #region Fleet values

            var playerNameDesc = new LabelControl("Player:")
            {
                Bounds = new UniRectangle(new UniScalar(0.01f, 0), new UniScalar(0.0f, 0), new UniScalar(0.3f, 0), new UniScalar(0.1f, 0))
            };

            _playerNameValue = new LabelControl("")
            {
                Bounds = new UniRectangle(new UniScalar(0.175f, 0), new UniScalar(0.0f, 0), new UniScalar(0.3f, 0), new UniScalar(0.1f, 0))
            };

            var fleetCountDesc = new LabelControl("Deployable fleets:")
            {
                Bounds = new UniRectangle(new UniScalar(0.01f, 0), new UniScalar(0.05f, 0), new UniScalar(0.3f, 0), new UniScalar(0.1f, 0))
            };

            _fleetCountValue = new LabelControl("0")
            {
                Bounds = new UniRectangle(new UniScalar(0.175f, 0), new UniScalar(0.05f, 0), new UniScalar(0.3f, 0), new UniScalar(0.1f, 0))
            };

            var fleetIncomeDesc = new LabelControl("Fleets per turn:")
            {
                Bounds = new UniRectangle(new UniScalar(0.01f, 0), new UniScalar(0.1f, 0), new UniScalar(0.3f, 0), new UniScalar(0.1f, 0))
            };

            _fleetIncomeValue = new LabelControl("0")
            {                
                Bounds = new UniRectangle(new UniScalar(0.175f, 0), new UniScalar(0.1f, 0), new UniScalar(0.3f, 0), new UniScalar(0.1f, 0))
            };

            #endregion

            #region Game buttons

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

            #endregion

            #region Order buttons

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

            #endregion

            #region Selected planet data

            var selectedPlanetDesc = new LabelControl("Selected planet:")
            {
                Bounds = new UniRectangle(new UniScalar(0.01f, 0), new UniScalar(0.2f, 0), new UniScalar(0.3f, 0), new UniScalar(0.1f, 0))
            };

            _selectedPlanetName = new LabelControl("None")
            {
                Bounds = new UniRectangle(new UniScalar(0.175f, 0), new UniScalar(0.2f, 0), new UniScalar(0.3f, 0), new UniScalar(0.1f, 0))
            };

            var planetFleetIncomeDesc = new LabelControl("Fleet income:")
            {
                Bounds = new UniRectangle(new UniScalar(0.01f, 0), new UniScalar(0.25f, 0), new UniScalar(0.3f, 0), new UniScalar(0.1f, 0))
            };

            _selectedPlanetBaseIncome = new LabelControl("-")
            {
                Bounds = new UniRectangle(new UniScalar(0.175f, 0), new UniScalar(0.25f, 0), new UniScalar(0.3f, 0), new UniScalar(0.1f, 0))
            };

            var planetStationedFleets = new LabelControl("Fleet count:")
            {
                Bounds = new UniRectangle(new UniScalar(0.01f, 0), new UniScalar(0.3f, 0), new UniScalar(0.3f, 0), new UniScalar(0.1f, 0))
            };

            _selectedPlanetFleetCount = new LabelControl("-")
            {
                Bounds = new UniRectangle(new UniScalar(0.175f, 0), new UniScalar(0.3f, 0), new UniScalar(0.3f, 0), new UniScalar(0.1f, 0))
            };

            #endregion

            #region Timer

            _timer = new LabelControl("0:00")
            {
                Bounds = new UniRectangle(new UniScalar(0.89f, 0), new UniScalar(0.0f, 0), new UniScalar(0.1f, 0), new UniScalar(0.07f, 0))               
            };

            #endregion

            screen.Desktop.Children.AddRange(
                new Control[] 
                {
                    ordersHeader, 
                    _orderList, 
                    
                    _playerNameValue,
                    playerNameDesc,
                    _fleetIncomeValue, 
                    _fleetCountValue, 
                    fleetCountDesc,
                    fleetIncomeDesc,
                    
                    btnDeleteOrder, 
                    btnLeaveGame, 
                    btnSendOrders,
                    btnMoveOrderDown,
                    btnMoveOrderUp,
                    
                    _selectedPlanetName,
                    _selectedPlanetBaseIncome,
                    _selectedPlanetFleetCount,
                    planetFleetIncomeDesc,
                    selectedPlanetDesc,
                    planetStationedFleets,

                    _timer
                });
        }        

        #endregion

        #region Event handlers

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
            // TODO: move order up the list
        }

        private void MoveOrderDown_Pressed(object sender, EventArgs e)
        {
            // TODO: move order down the list
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
                    if (firstItem - 1 > 0) 
                    {
                        firstItem--;
                    }

                    _orderList.SelectedItems.Add(firstItem);
                }

                // TODO update the logic (fleet count, etc.)
            }
        }

        #endregion

        #region Update requests

        public void UpdateSelectedPlanet(Planet planet)
        {
            _selectedPlanetName.Text = planet.Name;
            _selectedPlanetBaseIncome.Text = Convert.ToString(planet.BaseUnitsPerTurn);
            _selectedPlanetFleetCount.Text = Convert.ToString(planet.NumFleetsPresent);
        }

        public void UpdateClientPlayerFleetData(Player player)
        {
            _playerNameValue.Text = player.Username;
            _fleetCountValue.Text = Convert.ToString(player.DeployableFleets);
            _fleetIncomeValue.Text = Convert.ToString(player.FleetIncomePerTurn);
        }

        public void UpdateTimer(int secondsLeft)
        {
            int mins = secondsLeft / 60;
            int secs = secondsLeft % 60;

            // Display Timer in format 0:00
            _timer.Text = mins.ToString() + (secs < 10 ? ":0" : ":") + secs.ToString();
        }

        public void UpdateOrders(List<UserCommand> commands)
        {
            _orderList.Items.Clear();
            foreach (var cmd in commands)
            {
                _orderList.Items.Add(string.Format("Deploy {0} fleets to planet {1}", cmd.UnitCount, cmd.TargetPlanet.Name));   
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
