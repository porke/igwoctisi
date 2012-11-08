namespace Client.View.Play.Animations
{
    using System;
    using System.Collections.Generic;
    using Client.Common.AnimationSystem;
    using Client.Model;
    using Client.Renderer;
    using System.Threading;
    
    public static class Attack
    {
        public static void AnimateAttacks(this SceneVisual scene, IList<Tuple<SimulationResult, Action>> attacks,
            AnimationManager animationManager, SimpleCamera camera)
        {
            foreach (var tpl in attacks)
            {
                var attack = tpl.Item1;
                var onAttackEnd = tpl.Item2;

                // TODO attack animation
                onAttackEnd.Invoke();
            }
        }
    }
}
