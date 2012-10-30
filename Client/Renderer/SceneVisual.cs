namespace Client.Renderer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Client.Model;

    public sealed class SceneVisual
    {
        public List<Spaceship> Spaceships { get; private set; }

        private Scene Scene { get; set; }
        private List<Spaceship> _spaceshipsToAdd = new List<Spaceship>();


        public SceneVisual(Scene scene)
        {
            Scene = scene;
            Spaceships = new List<Spaceship>();
        }

        public void AddSpaceship(Spaceship ship)
        {
            lock (_spaceshipsToAdd)
            {
                _spaceshipsToAdd.Add(ship);
            }
        }

        internal void Draw(SimpleCamera camera, double delta, double time)
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
            foreach (var ship in Spaceships)
            {
                ship.Draw(camera, delta, time);
            }
        }
    }
}
