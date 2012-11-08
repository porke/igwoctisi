namespace Client.View.Play.Animations
{
    using Client.Common.AnimationSystem;
    using Client.Model;
    using Client.Renderer;
    using System.Collections.Generic;
    using System;
    using System.Threading;
    
    public static class Move
    {
        public static void AnimateMoves(this SceneVisual scene, IList<Tuple<Planet, Planet, int, Action>> moves,
            AnimationManager animationManager, SimpleCamera camera)
        {
            foreach (var move in moves)
            {
                var sourcePlanet = move.Item1;
                var targetPlanet = move.Item2;
                int numFleetsCount = move.Item3;
                Action onMoveEnd = move.Item4;

                // TODO move animation
                onMoveEnd.Invoke();
            }
        }
    }
}
