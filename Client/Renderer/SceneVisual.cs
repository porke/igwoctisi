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
        public SimpleCamera Camera { get; set; }

        private Scene Scene { get; set; }
        private readonly AnimationManager AnimationManager;
        private readonly List<Spaceship> _spaceships = new List<Spaceship>();
        private readonly List<Spaceship> _spaceshipsToAdd = new List<Spaceship>();
        private readonly List<Spaceship> _spaceshipsToRemove = new List<Spaceship>();

        public SceneVisual(Scene scene, ICollection<PlayerColor> colors, ContentManager Content, AnimationManager AnimationManager)
        {
            Scene = scene;
            Spaceship.SetupColorPools(colors, Content, AnimationManager);
            this.AnimationManager = AnimationManager;

            // Install handlers
            scene.AnimDeploys += new Action<IList<Tuple<Planet, int, Action>>>(Animation_Deploys);
            scene.AnimMoves += new Action<IList<Tuple<Planet, Planet, int, Action>>>(Animation_Moves);
            scene.AnimAttacks+=new Action<List<Tuple<SimulationResult,Action>>>(Animation_Attacks);
        }

        void Animation_Deploys(IList<Tuple<Planet, int, Action>> deploys)
        {
            this.AnimateDeploys(AnimationManager, Camera, deploys);
        }

        void Animation_Moves(IList<Tuple<Planet, Planet, int, Action>> moves)
        {
            this.AnimateMoves(moves, AnimationManager, Camera);
        }

        void Animation_Attacks(IList<Tuple<SimulationResult, Action>> attacks)
        {
            this.AnimateAttacks(attacks, AnimationManager, Camera);
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
                    _spaceships.Add(_spaceshipsToAdd[i]);

                _spaceshipsToAdd.Clear();
            }

            // TODO Draw planets and links (and particles?)

            // Draw spaceships
            lock (_spaceshipsToRemove)
            {
                foreach (var ship in _spaceships)
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
