
namespace Client.Renderer
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using Client.Common.AnimationSystem;
	using Client.Model;
	using Common;
	using Microsoft.Xna.Framework;
	using Microsoft.Xna.Framework.Content;
	using Microsoft.Xna.Framework.Graphics;

	public enum SpaceshipModelType
	{
		LittleSpaceship,	//Level1
		//ArmedCruiser,		//Level2
		OmegaDestroyer,		//Level2
		Dreadnought			//Level3
	}

	public class Spaceship : ITransformable
	{
		#region Pooling

		/// <summary>
		/// Dictionary key is the model type.
		/// </summary>
		private static Dictionary<int, ObjectPool<Spaceship>> pools = new Dictionary<int, ObjectPool<Spaceship>>();
		
		public static void SetupModelPools(ContentManager Content, AnimationManager AnimationManager)
		{
			var modelTypes = Enum.GetValues(typeof(SpaceshipModelType)).Cast<SpaceshipModelType>();

			pools = new Dictionary<int, ObjectPool<Spaceship>>();
			foreach (var type in modelTypes)
			{
				var factory = new SpaceshipFactory(type);
				var pool = new ObjectPool<Spaceship>(100, factory);

				factory.Content = Content;
				factory.AnimationManager = AnimationManager;

				pools.Add((int)type, pool);
			}
		}
		
		public static Spaceship Acquire(SpaceshipModelType modelType, PlayerColor playerColor)
		{
			return pools[(int)modelType].Get(spaceship =>
			{
				spaceship.Visible = true;
				spaceship.ScaleX = spaceship.ScaleY = spaceship.ScaleZ = 1;
				spaceship.Opacity = 1f;
				spaceship.PlayerColor = playerColor;
			});
		}

		public static void Recycle(Spaceship obj)
		{
			obj.Visible = false;
			pools[(int)obj.ModelType].Put(obj);
		}

		#endregion

		#region Object creation

		private class SpaceshipFactory : ObjectPool<Spaceship>.IObjectFactory
		{
			public ContentManager Content
			{
				get { return _contentManager; }
				set { _contentManager = value; OnInstallContentManager(); }
			}

			public AnimationManager AnimationManager { get; set; }

			private ContentManager _contentManager;
			private Model _model;
			private Texture2D _texture;
			private SpaceshipModelType _modelType;
					   
			public SpaceshipFactory(SpaceshipModelType modelType)
			{
				_modelType = modelType;
			}

			public Spaceship Fetch()
			{
				Debug.Assert(_contentManager != null, "ContentManager can't be null!", "SpaceshipFactory should have ContentManager already installed on Fetching new Spaceship.");
				return new Spaceship(_modelType, _model, _texture, AnimationManager);
			}

			private void OnInstallContentManager()
			{
				_model = Content.Load<Model>(@"Models\" + _modelType.ToString());
			}

			public void OnTextureLoad(IAsyncResult ar)
			{
				_texture = _contentManager.EndLoad<Texture2D>(ar);
			}
		}

		#endregion
		
		#region Public Fields

		public float X { get; set; }
		public float Y { get; set; }
		public float Z { get; set; }

		public Matrix Rotation { get; set; }
		
		public float ScaleX { get; set; }
		public float ScaleY { get; set; }
		public float ScaleZ { get; set; }

		public bool Visible { get; set; }
		public PlayerColor PlayerColor { get; private set; }
		public float Opacity { get; set; }
		public SpaceshipModelType ModelType { get; set; }

		#endregion

		#region Private Fields

		private Model Model { get; set; }
		private Texture2D Texture { get; set; }
		private AnimationManager AnimationManager;

		#endregion

		private Spaceship(SpaceshipModelType modelType, Model model, Texture2D texture, AnimationManager animationManager)
		{
			ModelType = modelType;
			Model = model;
			Texture = texture;
			AnimationManager = animationManager;
		}
		public void Draw(ICamera camera, double delta, double time)
		{
			foreach (ModelMesh mesh in Model.Meshes)
			{
				foreach (BasicEffect effect in mesh.Effects)
				{
					effect.EnableDefaultLighting();
					effect.World = this.CalculateWorldTransform();
					effect.View = camera.GetView();
					effect.Projection = camera.Projection;
					effect.PreferPerPixelLighting = true;

					// Transparency
					effect.GraphicsDevice.BlendState = BlendState.AlphaBlend;
					effect.Alpha = Opacity;
				}
				mesh.Draw();
			}
		}
	}
}
