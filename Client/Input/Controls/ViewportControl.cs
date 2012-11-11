
using Nuclex.UserInterface.Controls;
using Client.Model;
using Client.Renderer;
using Microsoft.Xna.Framework;
using Client.Common.AnimationSystem;
using System;
using Microsoft.Xna.Framework.Graphics;


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
			_currentMousePosition = new Vector2(x, y);
		}
		protected override void OnMouseWheel(float ticks)
		{
			_wheelTicks += ticks;
		}

		#endregion

		public const float ZoomVelocityFactor = 1.0f / 1200.0f;

		public IRenderer Renderer { get; set; }
		public Scene Scene { get; set; }

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
				var viewport = new Viewport((int)bounds.X, (int)bounds.Y, (int)bounds.Width, (int)bounds.Height);
				var cameraPosition = camera.GetPosition();
				var zeroPlane = new Plane(Vector3.Backward, 0);

				var currentUnit = Vector3.Normalize(viewport.Project(
					new Vector3(-_currentMousePosition.X, _currentMousePosition.Y, 1),
					camera.Projection, camera.GetView(), Matrix.Identity));
				var currentDist = new Ray(cameraPosition, currentUnit).Intersects(zeroPlane).Value;
				var currentWorld = cameraPosition + currentUnit * currentDist;

				var lastUnit = Vector3.Normalize(viewport.Project(
					new Vector3(-_lastMousePosition.X, _lastMousePosition.Y, 1),
					camera.Projection, camera.GetView(), Matrix.Identity));
				var lastDist = new Ray(cameraPosition, lastUnit).Intersects(zeroPlane).Value;
				var lastWorld = cameraPosition + lastUnit * lastDist;

				var dragForce = (currentWorld - lastWorld) / new Vector3(bounds.Width, bounds.Height, 1) / (float)delta * new Vector3(1, 1, 0);
				camera.Force = new Vector3(dragForce.X, dragForce.Y, camera.Force.Z);
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
