namespace Client.View
{
    using System;
    using System.Collections.Generic;
    using Client.Common.AnimationSystem;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;
    using Nuclex.Input;
    using Nuclex.UserInterface.Input;
    using IInputReceiver = Input.IInputReceiver;

    public class ViewManager
    {
        #region Protected members

        protected double currentTime;
        protected List<BaseView> _viewStack;

        #endregion

        #region Nested class: ViewManagerInputReceiver

        protected class ViewManagerInputReceiver : IInputReceiver
        {
            #region IInputReceiver members

            // commands
            public bool OnCommand(Command command)
            {
                return ProcessEvent((receiver) => receiver.OnCommand(command));
            }

            // keyboard
            public bool OnKeyPressed(Keys key)
            {
                return ProcessEvent((receiver) => receiver.OnKeyPressed(key));
            }
            public bool OnKeyReleased(Keys key)
            {
                return ProcessEvent((receiver) => receiver.OnKeyReleased(key));
            }
            public bool OnCharacter(char character)
            {
                return ProcessEvent((receiver) => receiver.OnCharacter(character));
            }

            // mouse
            public bool OnMouseMoved(Vector2 position)
            {
                return ProcessEvent((receiver) => receiver.OnMouseMoved(position));
            }
            public bool OnMousePressed(MouseButtons button)
            {
                return ProcessEvent((receiver) => receiver.OnMousePressed(button));
            }
            public bool OnMouseReleased(MouseButtons button)
            {
                return ProcessEvent((receiver) => receiver.OnMouseReleased(button));
            }
            public bool OnMouseWheel(float ticks)
            {
                return ProcessEvent((receiver) => receiver.OnMouseWheel(ticks));
            }

            // gamepad
            public bool OnButtonPressed(Buttons button)
            {
                return ProcessEvent((receiver) => receiver.OnButtonPressed(button));
            }
            public bool OnButtonReleased(Buttons button)
            {
                return ProcessEvent((receiver) => receiver.OnButtonReleased(button));
            }

            #endregion

            public ViewManager ViewMgr { get; protected set; }
            public ViewManagerInputReceiver(ViewManager viewMgr)
            {
                ViewMgr = viewMgr;
            }
            protected bool ProcessEvent(Func<IInputReceiver, bool> eventFunc)
            {
                var viewStack = ViewMgr._viewStack;
                lock (viewStack)
                {
                    for (var i = viewStack.Count - 1; i >= 0; --i)
                    {
                        var layer = viewStack[i];
                        if (layer.State == ViewState.Visible && eventFunc(layer.InputReceiver))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        #endregion

        public IInputReceiver InputReceiver { get; protected set; }
		public AnimationManager AnimationManager { get; protected set; }

        public ViewManager()
        {
            _viewStack = new List<BaseView>();

            InputReceiver = new ViewManagerInputReceiver(this);
			AnimationManager = new AnimationManager();
        }
        public void Update(double delta, double time)
        {
            currentTime += delta;
			
            lock (_viewStack)
            {
				for (var i = 0; i < _viewStack.Count; )
				{
					var view = _viewStack[i];

					if (view.State == ViewState.Loaded)
					{
						view.Show(this, currentTime);
					}

					if (view.State == ViewState.Hidden)
					{
						_viewStack.RemoveAt(i);
					}
					else
					{
						++i;
					}
				}

				_viewStack.ForEach(view =>
				{
					if (view.State == ViewState.FadeIn ||
						view.State == ViewState.Visible ||
						view.State == ViewState.FadeOut)
					{
						view.Update(delta, currentTime);
					}
				});
            }
			AnimationManager.Update(delta);
        }
        public void Draw(double delta, double time)
        {
            lock (_viewStack)
            {
                // start from topmost non transparent layer
                var first = _viewStack.FindLastIndex(view => !view.IsTransparent && 
					(view.State == ViewState.FadeIn ||
					view.State == ViewState.Visible ||
					view.State == ViewState.FadeOut));
                if (first == -1) first = 0;

                // and draw
                for (var i = first; i < _viewStack.Count; ++i)
                {
					var view = _viewStack[i];
					if (view.State == ViewState.FadeIn ||
						view.State == ViewState.Visible ||
						view.State == ViewState.FadeOut)
					{
						view.Draw(delta, currentTime);
					}
                }
            }
        }
        public void PushLayer(BaseView view)
        {
            lock (_viewStack)
            {
                _viewStack.Add(view);
            }
        }
        public BaseView PopLayer()
        {
            lock (_viewStack)
            {
				for (var i = _viewStack.Count - 1; i >= 0; --i)
				{
					var view = _viewStack[i];
					if (view.State == ViewState.FadeOut || view.State == ViewState.Hidden)
						continue;

					view.Hide(currentTime);
					return view;
				}
            }

            return null;
        }
        public BaseView PeekLayer()
        {
            lock (_viewStack)
			{
				for (var i = _viewStack.Count - 1; i >= 0; --i)
				{
					var view = _viewStack[i];
					if (view.State == ViewState.FadeOut || view.State == ViewState.Hidden)
						continue;

					return view;
				}
            }
			return null;
        }
    }
}
