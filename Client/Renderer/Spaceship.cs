namespace Client.Renderer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Client.Model;
    using Client.View;
    using Common;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using System.Diagnostics;
    
    public class Spaceship : IMovable
    {
        #region Pooling

        private static Dictionary<PlayerColor, ObjectPool<Spaceship>> pools;

        static Spaceship()
        {
            pools = new Dictionary<PlayerColor, ObjectPool<Spaceship>>();
            foreach (var color in Enum.GetValues(typeof(PlayerColor)).Cast<PlayerColor>())
            {
                var colorPool = new ObjectPool<Spaceship>(100, new SpaceshipFactory(color));
                pools.Add(color, colorPool);
            }
        }

        public static void InstallContentManager(ContentManager Content)
        {
            foreach (var factory in pools.Values.Select(pool => pool.Factory))
            {
                (factory as SpaceshipFactory).Content = Content;
            }
        }
        
        public static Spaceship Acquire(PlayerColor playerColor)
        {
            return pools[playerColor].Get(spaceship => { spaceship.Visible = true; });
        }

        public static void Put(Spaceship obj)
        {
            pools[obj.PlayerColor].Put(obj);
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

            private ContentManager _contentManager;
            private static Model _model;
            private Texture2D _texture;
            private PlayerColor _color;
                       
            public SpaceshipFactory(PlayerColor color)
            {
                _color = color;
            }

            public Spaceship Fetch()
            {
                Debug.Assert(_contentManager != null, "ContentManager can't be null!", "SpaceshipFactory should have ContentManager already installed on Fetching new Spaceship.");
                return new Spaceship(_color, _texture, _model);
            }

            private void OnInstallContentManager()
            {
                Content.BeginLoad<Model>(@"Models\LittleSpaceship", OnModelLoad, null);
                Content.BeginLoad<Texture2D>(@"Textures\Spaceships\" + _color.ToString(), OnTextureLoad, null);
            }

            public void OnModelLoad(IAsyncResult ar)
            {
                if (_model == null)
                {
                    _model = _contentManager.EndLoad<Model>(ar);
                }
            }

            public void OnTextureLoad(IAsyncResult ar)
            {
                _texture = _contentManager.EndLoad<Texture2D>(ar);
            }
        }

        #endregion
        
        #region Public Fields

        public Vector3 Position
        {
            get { return WorldTransform.Translation; }
            set
            {
                Matrix tmpTransform = this.WorldTransform;
                tmpTransform.Translation = value;
                this.WorldTransform = tmpTransform;
            }
        }
        public Matrix WorldTransform { get; set; }
        public bool Visible { get; set; }
        public PlayerColor PlayerColor { get; private set; }

        #endregion

        #region Private Fields

        private Texture2D Texture { get; set; }
        private Model Model { get; set; }

        #endregion


        private Spaceship(PlayerColor playerColor, Texture2D texture, Model model)
        {
            PlayerColor = playerColor;
            Texture = texture;
            Model = model;
        }

        public void Draw(SimpleCamera camera, double delta, double time)
        {
            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = WorldTransform;
                    effect.Texture = this.Texture;
                }
                mesh.Draw();
            }
        }
    }
}
