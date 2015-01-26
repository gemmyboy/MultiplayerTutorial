/*
 * Copyright (c) 2013 LowLevelTech
 * Alex Dunn
 * llt@dunnalex.com
*/

using Fluidity.Volume;
using UnityEngine;

namespace Fluidity.DemoUtilities
{
    /// <summary>
    /// Links an objects animation to its fluid volume.
    /// Pauses the animation of the global run simulation flag is unset on the controller.
    /// </summary>
    [RequireComponent(typeof(FluidVolume))]
    public class LinkAnimationToSimulation : LinkMotionToSimulation
    {
        public string nameOfAnimation;

        protected override void Start()
        {
            base.Start();

            if (!animation || !animation[nameOfAnimation]) return;

            animation.Play(nameOfAnimation);
        }

        protected override void Update()
        {
            base.Update();

            if (!animation || !animation[nameOfAnimation]) return;

            animation[nameOfAnimation].speed = Controller.GlobalRunSimulations ? 1 : 0;
        }
    }
}