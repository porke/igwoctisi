namespace Client.Renderer
{
	using Client.Common;
	using Client.Common.AnimationSystem;
	using Client.Model;
	using Microsoft.Xna.Framework;
	using Microsoft.Xna.Framework.Graphics;

	public class IndicatorVisual
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
		private Matrix _worldMatrix;
		private BoundingBox _transformedModelBBox;

		public Planet SourcePlanet { get; private set; }
		public Planet TargetPlanet { get; private set; }

		public IndicatorVisual(Model arrowModel, Planet sourcePlanet, Planet targetPlanet)
		{
			_model = arrowModel;
			SourcePlanet = sourcePlanet;
			TargetPlanet = targetPlanet;
			_worldMatrix = Matrix.Identity;
		}

		public void Initialize()
		{
			var sourcePlanetPos = SourcePlanet.Visual.GetPosition();
			var targetPlanetPos = TargetPlanet.Visual.GetPosition();
			var direction = Vector3.Normalize(targetPlanetPos - sourcePlanetPos);
			
			// Calculate rotation for whole model
			var rotationMatrix = MathUtils.LookAt(sourcePlanetPos, targetPlanetPos, Vector3.Backward);

			// Calculate positioning
			var bbox = MathUtils.GetModelBoundingBox(_model, Matrix.Identity);
			var arrowVect = bbox.Max - bbox.Min;
			float arrowLength = MathHelper.Max(arrowVect.X, MathHelper.Max(arrowVect.Y, arrowVect.Z));

			var translation = sourcePlanetPos
				+ (SourcePlanet.Radius + arrowLength) * direction
				+ Vector3.Forward * 2; //TODO: remove this line and correct not hiding link line using stencil buffer (look at Links.fx)
			var translationMatrix = Matrix.CreateTranslation(translation);

			// Calculate world matrix
			_worldMatrix = rotationMatrix * translationMatrix;
			_transformedModelBBox = new BoundingBox(Vector3.Transform(bbox.Min, _worldMatrix), Vector3.Transform(bbox.Max, _worldMatrix));
		}

		public void Draw(GraphicsDevice device, ICamera camera, double delta, double time, bool hovered, float opacity)
		{
			Color color = Color.DarkRed;
			if (TargetPlanet.Owner == SourcePlanet.Owner)
			{
				color = Color.DarkGreen;
			}

			var previousDepthState = device.DepthStencilState;
			/*device.DepthStencilState = new DepthStencilState()
			{
				StencilEnable = true,
				StencilMask = 0xFF,
				StencilWriteMask = 0xFF,
				StencilFail = StencilOperation.IncrementSaturation,
				StencilPass = StencilOperation.Keep,
				StencilFunction = CompareFunction.Greater
			};*/

			foreach (var mesh in _model.Meshes)
			{
				foreach (BasicEffect effect in mesh.Effects)
				{
					effect.EnableDefaultLighting();
					effect.World = _worldMatrix;
					effect.View = camera.GetView();
					effect.Projection = camera.Projection;
					effect.DiffuseColor = color.ToVector3();
					effect.Alpha = opacity * (hovered ? 2.0f : 0.6f);
				}
				mesh.Draw();
			}
			device.DepthStencilState = previousDepthState ?? DepthStencilState.Default;
		}

		internal bool IsIntersected(Ray ray)
		{
			return ray.Intersects(_transformedModelBBox) != null;
		}
	}
}
