
using Nuclex.UserInterface.Controls;
using Client.Model;
using Client.Renderer;
using Microsoft.Xna.Framework;
using Client.Common.AnimationSystem;
using System;
using Nuclex.Input;


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
		protected float _wheelTicks;
		protected double _pressTime;
		protected bool _pressed;

		#endregion

		#region Control members

		protected override void OnMousePressed(MouseButtons button)
		{
			if (button == MouseButtons.Left)
			{
				_pressTime = _currentTime;
				_pressed = true;
			}
		}
		protected override void OnMouseReleased(MouseButtons button)
		{
			if (button == MouseButtons.Left)
			{
				_pressed = false;
			}

			if (!_pressed || _currentTime - _pressTime < ClickInterval)	// click
			{
				if (MouseClick != null) MouseClick(this, button);
			}
		}
		protected override void OnMouseMoved(float x, float y)
		{
			_currentMousePosition = new Vector2(x, y);

			if (_pressed)
			{
				HoveredPlanet = null;
			}
			else
			{
				HoveredPlanet = Scene.PickPlanet(_currentMousePosition, Renderer);
				HoveredLink = Scene.PickLink(_currentMousePosition, Renderer);
			}
		}
		protected override void OnMouseWheel(float ticks)
		{
			_wheelTicks += ticks;
		}

		#endregion

		public const float DragSpeedFactor = 1.0f / 10.0f;
		public const float ZoomVelocityFactor = 1.0f / 1200.0f;

		public IRenderer Renderer { get; set; }
		public Scene Scene { get; set; }
		public Planet HoveredPlanet { get; protected set; }
		public PlanetLink HoveredLink { get; protected set; }

		public event Action<ViewportControl, MouseButtons> MouseClick;

		public ViewportControl(IRenderer renderer)
		{
			_pressed = false;
			Renderer = renderer;
		}
		public void Update(double delta, double time)
		{
			if (Scene == null)
				return;

			var camera = Scene.Map.Camera;
			if (_pressed) // drag
			{
				var bounds = this.GetAbsoluteBounds();
				var dragForce = (_currentMousePosition - _lastMousePosition);
				dragForce *= (float)Math.Tan(camera.FieldOfView) * Math.Abs(Vector3.Distance(camera.GetPosition(), camera.LookAt));
				dragForce *= DragSpeedFactor;
				camera.Force = new Vector3(dragForce.X, -dragForce.Y, camera.Force.Z);
			}

			if (_wheelTicks != 0)
			{
				var zoomForce = _wheelTicks * camera.Z / (float)delta * ZoomVelocityFactor;
				camera.Force = new Vector3(camera.Force.X, camera.Force.Y, zoomForce);
			}

			_wheelTicks = 0;
			_lastMousePosition = _currentMousePosition;
			_lastTime = _currentTime;
			_currentTime = time;


		}
		public void Draw(double delta, double time)
		{
			Renderer.Draw(Scene.Map.Camera, Scene, delta, time);
		}
	}
}
