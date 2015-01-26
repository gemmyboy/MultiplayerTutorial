/*
 * Copyright (c) 2013 LowLevelTech
 * Alex Dunn
 * llt@dunnalex.com
*/

using UnityEngine;

namespace Fluidity.DemoUtilities
{
    /// <summary>
    /// Links an volumes transform to a simulation.
    /// </summary>
    [RequireComponent(typeof(Volume.FluidVolume))]
    public class LinkMotionToSimulation : MonoBehaviour
    {
        private Volume.FluidVolume fluid;
        private Vector3 oldPosition;
        private Vector3 direction;
        private float startingForce;

        protected virtual void Start()
        {
            fluid = GetComponent<Volume.FluidVolume>();
            oldPosition = transform.position;

            Physics.Simulation mySimulation = fluid.Simulation;

            startingForce = (mySimulation == null ? 20 : mySimulation.emitterForce);
        }

        protected virtual void Update()
        {
            Physics.Simulation mySimulation = fluid.Simulation;

            if (mySimulation == null) return;

            direction = oldPosition - transform.position;
            mySimulation.sourceDirection = (direction).normalized;
            mySimulation.emitterForce = (direction.magnitude) * startingForce;
            mySimulation.sourcePosition = -mySimulation.sourceDirection * 0.8f;

            oldPosition = transform.position;
        }
    }
}