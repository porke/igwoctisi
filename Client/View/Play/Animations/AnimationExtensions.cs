using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Client.Common.AnimationSystem;
using Client.Renderer;

namespace Client.View.Play.Animations
{
    public static class AnimationExtensions
    {
        public static Animation<Spaceship> Animate(this Spaceship ship, AnimationManager animationManager)
        {
            return Animation<Spaceship>.Dummy(ship, animationManager);
        }
    }
}
