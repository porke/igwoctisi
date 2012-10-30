namespace Client.Renderer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Client.Common;
    using Client.View;
    using Client.Model;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;

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
            public ContentManager Content { get; set; }

            private PlayerColor _color;
            private Texture2D _texture;
            private static Model _model;
                       
            public SpaceshipFactory(PlayerColor color)
            {
                _color = color;
                _texture = Content.Load<Texture2D>(@"Textures\Spaceships\" + color.ToString());
                _model = Content.Load<Model>(@"Models\LittleSpaceship");
            }

            public Spaceship Fetch()
            {
                var ship = new Spaceship(_color, _texture, _model);
                return ship;
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
