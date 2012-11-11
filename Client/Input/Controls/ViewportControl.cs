
using Nuclex.UserInterface.Controls;
using Client.Model;
using Client.Renderer;
using Microsoft.Xna.Framework;


namespace Client.Input.Controls
{
	public class ViewportControl : Control
	{
		#region Protected members

		protected const double ClickInterval = 0.5;
		protected double _currentTime;
		protected double _lastTime;
		protected Vector2 _currentMousePosition;
		protected Vector2 _lastMousePosition;
		protected double _pressTime;
		protected bool _pressed;

		#endregion

		#region Control members

		protected override void OnMousePressed(Nuclex.Input.MouseButtons button)
		{
			_currentTime = _pressTime;
			_pressed = true;
		}
		protected override void OnMouseReleased(Nuclex.Input.MouseButtons button)
		{
			_pressed = false;
			if (_currentTime - _pressTime < ClickInterval)	// clic
			{
			}
		}
		protected override void OnMouseMoved(float x, float y)
		{
			_lastMousePosition = _currentMousePosition;
			_currentMousePosition = new Vector2(x, y);

			if (_pressed)	// drag
			{
			}
		}

		#endregion

		public IRenderer Renderer { get; set; }
		public Scene Scene { get; set; }

		public ViewportControl(IRenderer renderer)
		{
			_pressed = false;
			Renderer = renderer;
		}
		public void Update(double delta, double time)
		{
			_lastTime = _currentTime;
			_currentTime = time;
		}
		public void Draw(double delta, double time)
		{
			Renderer.Draw(Scene.Map.Camera, Scene, delta, time);
		}
	}
}
