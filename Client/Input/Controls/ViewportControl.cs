
using Nuclex.UserInterface.Controls;
using Client.Model;
using Client.Renderer;


namespace Client.Input.Controls
{
	public class ViewportControl : Control
	{
		#region Control members

		#endregion

		public IRenderer Renderer { get; set; }
		public Scene Scene { get; set; }
		public SimpleCamera Camera { get; set; }

		public ViewportControl(IRenderer renderer)
		{
			Renderer = renderer;
		}
		public void Draw(double delta, double time)
		{
			Renderer.Draw(Scene, delta, time);
		}
	}
}
