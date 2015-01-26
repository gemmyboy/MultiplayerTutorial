/*
 * Copyright (c) 2013 LowLevelTech
 * Alex Dunn
 * llt@dunnalex.com
*/

using System;
using Fluidity.Auxilliary;
using Fluidity.Volume;
using UnityEngine;

namespace Fluidity.Tools
{
    /// <summary>
    /// Apply effects to fluid simulations.
    /// 
    /// Currently only spheres are supported.
    /// Currently only colliders and temporary emitters are supported.
    /// </summary>
    [AddComponentMenu("Fluidity/Effector...")]
    public class Effector : MonoBehaviour
    {
        [Flags]
        public enum EffectorModes
        {
            Collision       = 1 << 0,
            TempEmitter     = 1 << 1,
            Receiver        = 1 << 2,
            Turbulence      = 1 << 3
        }

        /// <summary>
        /// World space radius of the sphere effector.
        /// </summary>
        [SerializeField] private float radius = 0.5f;

        /// <summary>
        /// Flags for effector mode.
        /// </summary>
        [SerializeField] private EffectorModes modes;

        /// <summary>
        /// Strength of effector, used for temporary emitter.
        /// </summary>
        [SerializeField] private float power;

        /// <summary>
        /// Optional channel to use for effector, this allowsa temporary emitter to write into a specific
        /// channel of a simulation.  When rendering this will use the gradient assigned to that particular channel if one exists.
        /// </summary>
        [SerializeField] private int channel = 1;

        /// <summary>
        /// Allows for tweaking of heat resistence.
        /// </summary>
        [SerializeField] private float heatResistance = 1;

        /// <summary>
        /// How many seconds it would take to cool down from 100 to 0.
        /// </summary>
        [SerializeField] private float coolDownRate = 1;

        /// <summary>
        /// Temperature at which becomes fatal.
        /// </summary>
        [SerializeField] private float fatalTemperature = 75;

        /// <summary>
        /// Optional send a message when dead.  This could be used to signal other controllers that we have died.
        /// </summary>
        [SerializeField] private bool sendSignalOnFatal;

        /// <summary>
        /// Local space axis of this effector.
        /// </summary>
        [SerializeField] private Vector3 axis = Vector3.up;

        /// <summary>
        /// Accessor for the effector modes.
        /// </summary>
        public EffectorModes ModeMask
        {
            get { return modes; }
            set { modes = value; }
        }

        /// <summary>
        /// World space radius for the effector, takes into account this transforms scale.
        /// </summary>
        public float WorldRadius
        {
            get { return Mathf.Max(transform.lossyScale.x, Mathf.Max(transform.lossyScale.y, transform.lossyScale.z))*radius; }
            set { radius = value; }
        }

        /// <summary>
        /// Accessor for this effectors power parameter.
        /// </summary>
        public float Power
        {
            get { return power; }
            set { power = value; }
        }

        /// <summary>
        /// Accessor for this effectors channel parametr.
        /// </summary>
        public int Channel
        {
            get { return channel; }
            set { channel = Mathf.Clamp(value, 1, 4); }
        }

        /// <summary>
        /// Accessor for this effectors fatal temperature, only useful when 'SendSignalOnFatal' is set.
        /// </summary>
        public float FatalTemperature
        {
            get { return fatalTemperature; }
            set { fatalTemperature = value; }
        }

        /// <summary>
        /// Accessor for the heat resistance of this effector.
        /// </summary>
        public float HeatResistance
        {
            get { return heatResistance; }
            set { heatResistance = value; }
        }

        /// <summary>
        /// Accessor for this effectors rate of cool down.
        /// </summary>
        public float CoolDownRate
        {
            get { return coolDownRate; }
            set { coolDownRate = value; }
        }

        /// <summary>
        /// Sets whether or not to send a message when a fatal temperature has been reached.
        /// </summary>
        public bool SendSignalOnFatal
        {
            get { return sendSignalOnFatal; }
            set { sendSignalOnFatal = value; }
        }

        /// <summary>
        /// Accessor for the local space axis to use for direction of this effector.
        /// </summary>
        public Vector3 Axis 
        { 
            get { return axis; }
            set { axis = value.normalized; }
        }

        /// <summary>
        /// Direction of this effector in world space.  Transform 'Axis' to world space.
        /// </summary>
        public Vector3 WorldSpaceDirection { get { return transform.localToWorldMatrix.MultiplyVector(axis); } }

        /// <summary>
        /// AABB bounds of the effector.
        /// </summary>
        public Bounds Bounds { get { return new Bounds(transform.position, Vector3.one * radius * 2); } }

        /// <summary>
        /// GPU -> CPU readback field for temperature.
        /// </summary>
        public float Temperature { get; private set; }

        #region Unity API
        /// <summary>
        /// Unity callback, draws a sphere representing this effector.
        /// </summary>
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.4f, 0.7f, 1.0f, 1.0f);
            Gizmos.DrawWireSphere(transform.position, radius);
            Gizmos.color = new Color(0.4f, 0.7f, 1.0f, 0.2f);
            Gizmos.DrawSphere(transform.position, radius);

            if ((ModeMask & (EffectorModes.TempEmitter | EffectorModes.Turbulence)) != 0)
            {
                Helpers.DrawArrow(transform.position, WorldSpaceDirection);
            }

            if((ModeMask & EffectorModes.TempEmitter) != 0)
            {
                Gizmos.DrawIcon(transform.position, "FluidityLogo.png", true);
            }
        }

        /// <summary>
        /// Unity callback,
        /// 
        /// Effects simulations.
        /// </summary>
        private void Update()
        {
            EffectSimulations();

            if ((modes & EffectorModes.Receiver) != 0)
            {
                Temperature -= coolDownRate*100.0f*Time.deltaTime;
                Temperature = Mathf.Clamp(Temperature, 0, 100);

                if(Temperature >= fatalTemperature)
                {
                    if(sendSignalOnFatal)
                    {
                        SendMessageUpwards("OnDeath", SendMessageOptions.DontRequireReceiver);
                        Destroy(this);
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Calculates any intersecting fluid volumes and applies the properties of this effector
        /// to that simulation.
        /// </summary>
        private void EffectSimulations()
        {
            var fluidVolumes = Controller.QueryVolumeIntersection(Bounds);

            foreach (Volume.FluidVolume v in fluidVolumes)
            {
                if(v != null && v.Simulation != null)
                {
                    v.Simulation.LinkEffector(this, v);
                }
            }
        }

        public void ApplyHeat(float temperature)
        {
            Temperature += temperature * (1 - heatResistance);
        }
    }
}

