namespace Client.Renderer
{
    using System;
    using System.Collections.Generic;
    using Client.Common.AnimationSystem;
    using Client.Model;
    using Microsoft.Xna.Framework.Content;
    using Client.View.Play.Animations;

    public sealed class SceneVisual
    {
        public List<Spaceship> Spaceships { get; private set; }
        public SimpleCamera Camera { get; set; }

        private Scene Scene { get; set; }
        private AnimationManager AnimationManager;
        private List<Spaceship> _spaceshipsToAdd = new List<Spaceship>();
        private List<Spaceship> _spaceshipsToRemove = new List<Spaceship>();

        public SceneVisual(Scene scene, ICollection<PlayerColor> colors, ContentManager Content, AnimationManager AnimationManager)
        {
            Scene = scene;
            Spaceships = new List<Spaceship>();
            Spaceship.SetupColorPools(colors, Content, AnimationManager);
            this.AnimationManager = AnimationManager;

            // Install handlers
            scene.AnimDeploy += new Action<Planet, int, Action>(Animation_Deploy);
        }

        void Animation_Deploy(Planet targetPlanet, int newFleetsCount, Action onEndCallback)
        {
            for (int i = 0; i < newFleetsCount; ++i)
            {
                var ship = Spaceship.Acquire(targetPlanet.Owner.Color);
                AddSpaceship(ship);
                ship.AnimateDeploy(AnimationManager, Camera, targetPlanet, newFleetsCount,
                    () => {
                        onEndCallback();
                        Spaceship.Recycle(ship);
                    });

                break;
            }
        }

        internal void AddSpaceship(Spaceship ship)
        {
            lock (_spaceshipsToAdd)
            {
                _spaceshipsToAdd.Add(ship);
            }
        }

        internal void Draw(double delta, double time)
        {
            // Update spaceship list
            lock (_spaceshipsToAdd)
            {
                // It is more efficient to use that type of loop than foreach/ForEach.
                for (int i = 0, n = _spaceshipsToAdd.Count; i < n; ++i)
                    Spaceships.Add(_spaceshipsToAdd[i]);

                _spaceshipsToAdd.Clear();
            }

            // TODO Draw planets and links (and particles?)

            // Draw spaceships
            lock (_spaceshipsToRemove)
            {
                foreach (var ship in Spaceships)
                {
                    if (ship.Visible)
                        ship.Draw(Camera, delta, time);
                    else
                        _spaceshipsToRemove.Add(ship);
                }
                _spaceshipsToRemove.Clear();
            }
        }
    }
}
