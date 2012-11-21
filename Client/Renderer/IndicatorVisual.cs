namespace Client.Renderer
{
	using Client.Common.AnimationSystem;
	using Client.Model;
	using Microsoft.Xna.Framework;
	using Microsoft.Xna.Framework.Graphics;

	public class IndicatorVisual : ITransformable
	{
		#region Public members

		#region ITransformable members

		public float X { get; set; }
		public float Y { get; set; }
		public float Z { get; set; }

		public Matrix Rotation { get; set; }

		public float ScaleX { get; set; }
		public float ScaleY { get; set; }
		public float ScaleZ { get; set; }

		#endregion


		#endregion
		
		private Model _model;
		private Planet _sourcePlanet;
		private Planet _targetPlanet;
		private Matrix _worldMatrix;

		public IndicatorVisual(Model arrowModel, Planet sourcePlanet, Planet targetPlanet)
		{
			_model = arrowModel;
			_sourcePlanet = sourcePlanet;
			_targetPlanet = targetPlanet;
		}

		public void Initialize()
		{
			// TODO calculate rotation, position and so on.

			//Cache those in _worldMatrix
			_worldMatrix = Rotation * this.GetScaleMatrix() * this.GetTranslationMatrix();

			var pos = Vector3.Lerp(_targetPlanet.Visual.GetPosition(), _sourcePlanet.Visual.GetPosition(), 0.5f);
			_worldMatrix = Matrix.CreateTranslation(pos);
		}

		public void Draw(GraphicsDevice device, ICamera camera, double delta, double time)
		{
			//if (Visible)
			{
				// TODO set red or green color (depends on attack/move)

				foreach (ModelMesh mesh in _model.Meshes)
				{
					foreach (BasicEffect effect in mesh.Effects)
					{
						effect.EnableDefaultLighting();
						effect.World = _worldMatrix;
						effect.View = camera.GetView();
						effect.Projection = camera.Projection;
					}
					mesh.Draw();
				}
			}
		}
	}
}
