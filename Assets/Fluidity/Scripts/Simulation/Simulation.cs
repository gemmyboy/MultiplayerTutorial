/*
 * Copyright (c) 2013 LowLevelTech
 * Alex Dunn
 * llt@dunnalex.com
*/

using System;
using System.Collections.Generic;
using Fluidity.Auxilliary;
using Fluidity.Volume;
using UnityEngine;

namespace Fluidity.Physics
{
    /// <summary>
    /// All the relevant data to run a fluid simulation is held here.
    /// </summary>
    [Serializable]
    public class Simulation
    {
        /// <summary>
        /// Which emitter state to use.
        /// </summary>
        public enum EmitterMode
        {
            Sphere,
            Texture
        }

        /// <summary>
        /// Maps the physical simulation kernels to logical names.
        /// </summary>
        private enum SimKernel
        {
            EmitWithSpherePartial = 0,
            EmitWithSphere,
            EmitWithSphereAndTempEmitter,
            EmitWithTexture,
            EmitWithTextureAndTempEmitter,
            Advect,
            AdvectBackward,
            MacCormack,
            SimulateAIOClamped,
            SimulateAIOUnclamped,
            Divergence, 
            JaccobiClamped,
            JaccobiUnclamped,
            ProjectClamped,
            ProjectUnclamped,
            Vorticity,
            Confinement,
            Clear
        }

        /// <summary>
        /// A CPU structure used to hold effector data, maps directly to GPU counterpart.
        /// </summary>
        private struct Effector // Two quad words, 32 byte.
        {
#           pragma warning disable 0414
            public Vector4 position_radius;
            public float temperature;
            public float power;
            public int mask;
            public int pk_direction;
#           pragma warning restore 0414

            public static int ByteStride { get { return 32; } }

            public Effector(Vector3 posTS, float radius, int mask, Vector3 direction, float power, int channel)
            {
                position_radius = new Vector4(posTS.x, posTS.y, posTS.z, radius);
                temperature = 0;
                pk_direction = Helpers.PackFloat4IntoInt(direction);

                this.mask = (channel << 8) | mask;
                this.power = power;
            }
        }

        /// <summary>
        /// Maximum number of effectors to use in simulation.
        /// NOTE: This must be mapped directly to the GPU counterpart in the simulation compute shader.
        /// </summary>
        private const int kMaxEffectors = 3;

        /// <summary>
        /// A useful identifier for this simulation.
        /// </summary>
        public string name;

        /// <summary>
        /// Emit fluid flag.  With this set the simulation will spew fluid.
        /// </summary>
        public bool emitFluid = true;

        /// <summary>
        /// Resolution of the eulerian grid to perform the simulation in.
        /// </summary>
        public int3 maxDimensions = new int3(32, 32, 32);

        /// <summary>
        /// Force at which to emit fluid from the source.
        /// </summary>
        public float emitterForce = 1;

        /// <summary>
        /// Used in sphere emission mode. This is the world space radius of the emitter.
        /// </summary>
        public float emitterRadius = 10;

        /// <summary>
        /// Scale of confinement to use when applying vorticity in simulation.
        /// </summary>
        public float confinementScale = 0.1f;

        /// <summary>
        /// Rate of decay for the fluid.
        /// </summary>
        public float reactionSpeed = 0.01f;

        /// <summary>
        /// How much gravity should affect this simulation.
        /// </summary>
        public float affectedByGravity;

        /// <summary>
        /// Viscosity term of fluid.
        /// </summary>
        public float viscosity = 1.0f;

        /// <summary>
        /// With this flag set the simulation will perform 2nd order correction on the fluid, resulting in a more realistic effect.
        /// </summary>
        public bool useMacCormack = false;

        /// <summary>
        /// With this flag set the simulation will collide with it's own bounds.
        /// </summary>
        public bool useBounds = true;

        /// <summary>
        /// With this flag set vorticity confinement will be performed, providing a more realistic effect.
        /// </summary>
        public bool useVorticityConfinement = false;

        /// <summary>
        /// The amount of iterations to use when solving for pressure.
        /// </summary>
        public int jaccobiIterations = 10;

        /// <summary>
        /// The position on the source emitter in sphere mode.
        /// NOTE: Local space.
        /// </summary>
        public Vector3 sourcePosition;

        /// <summary>
        /// The direction of the emitter in sphere mode.
        /// NOTE: Local space.
        /// </summary>
        public Vector3 sourceDirection = Vector3.up;

        /// <summary>
        /// With this flag set the fluid simulation will be performed in high quality mode, which provides a variable number of iterations.
        /// </summary>
        public bool isHighQuality;

        /// <summary>
        /// With this flag set this simulation will ignore culling, which means it will still be run when no camera is looking at it.
        /// </summary>
        public bool ignoreCulling;

        /// <summary>
        /// The emission mode to use when injecting fluid into the simulation.
        /// </summary>
        public EmitterMode emitterMode;

        /// <summary>
        /// When using texture emission mode, this is the texture to inject fluid from.
        /// </summary>
        public Texture emitTexture;

        // Public accessors, used for the rendering.
        public int3 Dimensions { get { return maxDimensions; } }

        /// <summary>
        /// Turns this simulation into a translating fixed grid.
        /// </summary>
        public bool isTranslatingFixedGrid;

        /// <summary>
        /// Amount this fluid simulation has moved in a frame.
        /// </summary>
        public Vector3 translation;

        /// <summary>
        /// Is this simulation visible and used by any renderers?
        /// </summary>
        private bool isVisible;

        /// <summary>
        /// Checks to see if we need to perform a GPU to CPU readback on effector data.
        /// </summary>
        private bool doWeNeedToReadBackEffectorData;

        /// <summary>
        /// A triple buffer of velocity texture volumes.
        /// </summary>
        private RenderTexture[] velocityTextures;

        /// <summary>
        /// A double buffer of pressure texture volumes.
        /// </summary>
        private RenderTexture[] pressureTextures;

        /// <summary>
        /// A double buffer of reaction coordinate texture volumes.
        /// </summary>
        private RenderTexture[] reactionCoordTexture;

        /// <summary>
        /// The divergence texture volume.
        /// </summary>
        private RenderTexture divergenceTexture;

        /// <summary>
        /// The vorticity texture volume.
        /// </summary>
        private RenderTexture vorticityTexture;

        /// <summary>
        /// Compute buffer which holds all the effectors currently used by this simulation.
        /// </summary>
        private ComputeBuffer effectorBuffer;

        /// <summary>
        /// A CPU container for the effector data.
        /// </summary>
        private readonly Effector[] effectors = new Effector[kMaxEffectors];

        /// <summary>
        /// A map for the fluid effector component.
        /// </summary>
        private readonly Dictionary<Tools.Effector, Effector> linkedEffectors = new Dictionary<Tools.Effector, Effector>();

        /// <summary>
        /// The dimension data, precomputed for this simulation.
        /// </summary>
        internal readonly DimensionData dimensionData = new DimensionData();

        /// <summary>
        /// Accessor for the reaction coord texture, this is what's used by the renderer to visualize the fluid.
        /// </summary>
        public RenderTexture ReactionCoordTexture { get { return reactionCoordTexture[0]; } }

        /// <summary>
        /// Flag sed to early out of simulation evolve code.
        /// </summary>
        private bool ShouldUpdateSimulation { get { return (ignoreCulling || isVisible) && Mathf.Abs(TimeScale) >= 0.001f; } }

        /// <summary>
        /// Helper to get the game's timescale mixed with fluid timescale.
        /// </summary>
        private float TimeScale { get { return Time.timeScale*Controller.TimeScale; } }

        #region Public API

        /// <summary>
        /// Gets the index of a simulation with name.
        /// </summary>
        /// <param name="name">Name of simulation to find.</param>
        /// <returns>Index for specified simulation, -1 if no simulation with name found.</returns>
        public static int NameToIndex(string name)
        {
            if (Controller.Simulations == null)
            {
                throw new Exception("Something has gone wrong with the simulation setup.");
            }

            for (int i = 0; i < Controller.Simulations.Length; i++)
            {
                if (Controller.Simulations[i].name == name)
                {
                    return i;
                }
            }

            Debug.LogWarning("Cannot find simulation with name, " + name + ".");
            return -1;
        }

        /// <summary>
        /// Gets the name of simulation with index.
        /// </summary>
        /// <param name="simulationIndex">Index of simulation to find.</param>
        /// <returns>Name for specified simulation, empty string if no simulation at index found.</returns>
        public static string IndexToName(int simulationIndex)
        {
            if (Controller.Simulations == null)
            {
                throw new Exception("Something has gone wrong with the simulation setup.");
            }

            if (Controller.Simulations.Length <= simulationIndex || simulationIndex < 0)
            {
                Debug.LogWarning("Trying to get a name for simulation using out of bounds index, " + simulationIndex+ ".");
                return "";
            }

            return Controller.Simulations[simulationIndex].name;
        }

        #endregion

        #region Aux
        /// <summary>
        /// Creates the volume textures used by the simulation.
        /// </summary>
        protected void CreateVolumes()
        {
            velocityTextures = new RenderTexture[3];
            pressureTextures = new RenderTexture[2];
            reactionCoordTexture = new RenderTexture[2];

            bool linearColourSpace = QualitySettings.activeColorSpace == ColorSpace.Linear;

            reactionCoordTexture[0] = CreateRTVolume(maxDimensions.x,      maxDimensions.y, maxDimensions.z, "_ReactionCoordTex",  linearColourSpace ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32);
            reactionCoordTexture[1] = CreateRTVolume(maxDimensions.x,      maxDimensions.y, maxDimensions.z, "_ReactionCoordTexA", linearColourSpace ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32);
            velocityTextures[0]     = CreateRTVolume(maxDimensions.x,      maxDimensions.y, maxDimensions.z, "_VelocityTex",       RenderTextureFormat.ARGBHalf);
            velocityTextures[1]     = CreateRTVolume(maxDimensions.x,      maxDimensions.y, maxDimensions.z, "_VelocityTex",       RenderTextureFormat.ARGBHalf);
            velocityTextures[2]     = CreateRTVolume(maxDimensions.x,      maxDimensions.y, maxDimensions.z, "_VelocityTex",       RenderTextureFormat.ARGBHalf);
            vorticityTexture        = CreateRTVolume(maxDimensions.x,      maxDimensions.y, maxDimensions.z, "_VorticityTex",      RenderTextureFormat.ARGBHalf);
            pressureTextures[0]     = CreateRTVolume(maxDimensions.x >> 2, maxDimensions.y, maxDimensions.z, "_PressureTex",       RenderTextureFormat.ARGBHalf);
            pressureTextures[1]     = CreateRTVolume(maxDimensions.x >> 2, maxDimensions.y, maxDimensions.z, "_PressureTex",       RenderTextureFormat.ARGBHalf);
            divergenceTexture       = CreateRTVolume(maxDimensions.x >> 2, maxDimensions.y, maxDimensions.z, "_DivergenceTex",     RenderTextureFormat.ARGBHalf);
        }

        /// <summary>
        /// Sets the is visible flag on this simulation.
        /// </summary>
        internal void SetVisible()
        {
            isVisible = true;
        }

        /// <summary>
        /// Clears and destroys the buffers and texture volumes used by this simulation.
        /// </summary>
        internal void Destroy()
        {
            Clear();    // Really shouldn't have to do this...

            RenderTexture.active = null;

            foreach (RenderTexture rt in velocityTextures) rt.Release();
            foreach (RenderTexture rt in pressureTextures) rt.Release();
            foreach (RenderTexture rt in reactionCoordTexture) rt.Release();

            divergenceTexture.Release();
            vorticityTexture.Release();

            effectorBuffer.Dispose();
        }

        #endregion

        #region Kernel Dispatchers
        private void Advect()
        {
            using (new ProfilerMarker(name + " - Advect"))
            {
                const int kernel = (int)SimKernel.Advect;

                Controller.SimulationShader.SetTexture(kernel, "_VelocityTex", velocityTextures[0]);
                Controller.SimulationShader.SetTexture(kernel, "_ReactionTex", reactionCoordTexture[0]);

                RenderTexture.active = velocityTextures[1];
                Controller.SimulationShader.SetTexture(kernel, "_VelocityTexRW", velocityTextures[1]);
                RenderTexture.active = reactionCoordTexture[1];
                Controller.SimulationShader.SetTexture(kernel, "_ReactionTexRW", reactionCoordTexture[1]);

                Controller.SimulationShader.Dispatch(kernel, (maxDimensions.x + 7) >> 3, (maxDimensions.y + 7) >> 3, (maxDimensions.z + 7) >> 3);
            }
        }

        private void AdvectBackwards()
        {
            using (new ProfilerMarker(name + " - Advect Backwards"))
            {
                const int kernel = (int)SimKernel.AdvectBackward;

                Controller.SimulationShader.SetTexture(kernel, "_VelocityTex", velocityTextures[0]);
                Controller.SimulationShader.SetTexture(kernel, "_VelocityTexA", velocityTextures[1]);

                RenderTexture.active = velocityTextures[2];
                Controller.SimulationShader.SetTexture(kernel, "_VelocityTexRW", velocityTextures[2]);

                Controller.SimulationShader.Dispatch(kernel, (maxDimensions.x + 7) >> 3, (maxDimensions.y + 7) >> 3, (maxDimensions.z + 7) >> 3);
            }
        }

        private void AdvectMacCormack()
        {
            using (new ProfilerMarker(name + " - Advect MacCormack"))
            {
                const int kernel = (int)SimKernel.MacCormack;

                Controller.SimulationShader.SetTexture(kernel, "_ReactionTex", reactionCoordTexture[0]);
                Controller.SimulationShader.SetTexture(kernel, "_VelocityTex", velocityTextures[0]);
                Controller.SimulationShader.SetTexture(kernel, "_VelocityTexA", velocityTextures[2]);

                RenderTexture.active = velocityTextures[1];
                Controller.SimulationShader.SetTexture(kernel, "_VelocityTexRW", velocityTextures[1]);
                RenderTexture.active = reactionCoordTexture[1];
                Controller.SimulationShader.SetTexture(kernel, "_ReactionTexRW", reactionCoordTexture[1]);

                Controller.SimulationShader.Dispatch(kernel, (maxDimensions.x + 7) >> 3, (maxDimensions.y + 7) >> 3, (maxDimensions.z + 7) >> 3);
            }
        }

        private void Inject()
        {
            using (new ProfilerMarker(name + " - Inject"))
            {
                bool hasAnyAffectors = linkedEffectors.Count > 0;
                switch (emitterMode)
                {
                    case EmitterMode.Sphere:
                        int sKernel = ((int)SimKernel.EmitWithSphere) + (hasAnyAffectors ? 1 : 0);

                        Controller.SimulationShader.SetTexture(sKernel, "_VelocityTex", velocityTextures[1]);
                        Controller.SimulationShader.SetTexture(sKernel, "_ReactionTex", reactionCoordTexture[1]);

                        Controller.SimulationShader.SetBuffer(sKernel, "_AffectorsRW", effectorBuffer);

                        RenderTexture.active = velocityTextures[0];
                        Controller.SimulationShader.SetTexture(sKernel, "_VelocityTexRW", velocityTextures[0]);

                        RenderTexture.active = reactionCoordTexture[0];
                        Controller.SimulationShader.SetTexture(sKernel, "_ReactionTexRW", reactionCoordTexture[0]);

                        Controller.SimulationShader.Dispatch(sKernel, (maxDimensions.x + 7) >> 3, (maxDimensions.y + 7) >> 3, (maxDimensions.z + 7) >> 3);
                        break;

                    case EmitterMode.Texture:
                        int tKernel = (int)SimKernel.EmitWithTexture + (hasAnyAffectors ? 1 : 0);

                        Controller.SimulationShader.SetTexture(tKernel, "_EmitterTex", emitTexture);
                        Controller.SimulationShader.SetTexture(tKernel, "_VelocityTex", velocityTextures[1]);
                        Controller.SimulationShader.SetTexture(tKernel, "_ReactionTex", reactionCoordTexture[1]);

                        Controller.SimulationShader.SetBuffer(tKernel, "_AffectorsRW", effectorBuffer);

                        RenderTexture.active = velocityTextures[0];
                        Controller.SimulationShader.SetTexture(tKernel, "_VelocityTexRW", velocityTextures[0]);

                        RenderTexture.active = reactionCoordTexture[0];
                        Controller.SimulationShader.SetTexture(tKernel, "_ReactionTexRW", reactionCoordTexture[0]);

                        Controller.SimulationShader.Dispatch(tKernel, (maxDimensions.x + 7) >> 3, (maxDimensions.y + 7) >> 3, (maxDimensions.z + 7) >> 3);
                        break;
                }
            }
        }

        private void Divergence()
        {
            using (new ProfilerMarker(name + " - Divergence"))
            {
                const int kernel = (int)SimKernel.Divergence;

                Controller.SimulationShader.SetTexture(kernel, "_VelocityTex", velocityTextures[1]);

                RenderTexture.active = divergenceTexture;
                Controller.SimulationShader.SetTexture(kernel, "_DivergenceTexRW", divergenceTexture);

                Controller.SimulationShader.Dispatch(kernel, (maxDimensions.x + 31) >> 5, (maxDimensions.y + 7) >> 3, (maxDimensions.z + 7) >> 3);
            }
        }

        private void Jaccobian()
        {
            using (new ProfilerMarker(name + " - Jacobbi"))
            {
                int kernel = (int)SimKernel.JaccobiClamped + (useBounds ? 0 : 1);

                for (int a = 0; a < jaccobiIterations; a++)
                {
                    Controller.SimulationShader.SetTexture(kernel, "_PressureTex", pressureTextures[1]);
                    Controller.SimulationShader.SetTexture(kernel, "_DivergenceTex", divergenceTexture);

                    RenderTexture.active = pressureTextures[0];
                    Controller.SimulationShader.SetTexture(kernel, "_PressureTexRW", pressureTextures[0]);

                    Controller.SimulationShader.Dispatch(kernel, (maxDimensions.x + 31) >> 5, (maxDimensions.y + 7) >> 3, (maxDimensions.z + 7) >> 3);

                    Swap(ref pressureTextures[0], ref pressureTextures[1]);
                }
            }
        }

        private void Project()
        {
            using (new ProfilerMarker(name + " - Project"))
            {
                int kernel = (int)SimKernel.ProjectClamped + (useBounds ? 0 : 1);

                Controller.SimulationShader.SetTexture(kernel, "_VelocityTex", velocityTextures[1]);
                Controller.SimulationShader.SetTexture(kernel, "_PressureTex", pressureTextures[1]);
                Controller.SimulationShader.SetTexture(kernel, "_ReactionTex", reactionCoordTexture[1]);

                Controller.SimulationShader.SetBuffer(kernel, "_Affectors", effectorBuffer);

                RenderTexture.active = velocityTextures[1];
                Controller.SimulationShader.SetTexture(kernel, "_VelocityTexRW", velocityTextures[0]);
                RenderTexture.active = reactionCoordTexture[1];
                Controller.SimulationShader.SetTexture(kernel, "_ReactionTexRW", reactionCoordTexture[0]);

                Controller.SimulationShader.Dispatch(kernel, (maxDimensions.x + 31) >> 5, (maxDimensions.y + 7) >> 3, (maxDimensions.z + 7) >> 3);
            }
        }

        private void VorticityConfinement()
        {
            using (new ProfilerMarker(name + " - Vorticity Confinement"))
            {
                const int vKernel = (int)SimKernel.Vorticity;

                Controller.SimulationShader.SetTexture(vKernel, "_VelocityTex", velocityTextures[1]);

                RenderTexture.active = vorticityTexture;
                Controller.SimulationShader.SetTexture(vKernel, "_VorticityTexRW", vorticityTexture);
                RenderTexture.active = velocityTextures[0];
                Controller.SimulationShader.SetTexture(vKernel, "_VelocityTexRW", velocityTextures[0]);

                Controller.SimulationShader.Dispatch(vKernel, (maxDimensions.x + 7) >> 3, (maxDimensions.y + 7) >> 3, (maxDimensions.z + 7) >> 3);

                const int cKernel = (int)SimKernel.Confinement;

                Controller.SimulationShader.SetTexture(cKernel, "_VorticityTex", vorticityTexture);
                Controller.SimulationShader.SetTexture(cKernel, "_VelocityTex", velocityTextures[0]);

                RenderTexture.active = velocityTextures[0];
                Controller.SimulationShader.SetTexture(cKernel, "_VelocityTexRW", velocityTextures[1]);

                Controller.SimulationShader.Dispatch(cKernel, (maxDimensions.x + 7) >> 3, (maxDimensions.y + 7) >> 3, (maxDimensions.z + 7) >> 3);
            }
        }

        private void EvolveAIO()
        {
            using (new ProfilerMarker(name + " - AIO Evolve"))
            {
                int kernel = (int)SimKernel.SimulateAIOClamped + (useBounds ? 0 : 1);

                Controller.SimulationShader.SetTexture(kernel, "_VelocityTex", velocityTextures[1]);
                Controller.SimulationShader.SetTexture(kernel, "_PressureTex", pressureTextures[1]);
                Controller.SimulationShader.SetTexture(kernel, "_ReactionTex", reactionCoordTexture[1]);

                Controller.SimulationShader.SetBuffer(kernel, "_Affectors", effectorBuffer);

                RenderTexture.active = velocityTextures[1];
                Controller.SimulationShader.SetTexture(kernel, "_VelocityTexRW", velocityTextures[0]);
                RenderTexture.active = reactionCoordTexture[1];
                Controller.SimulationShader.SetTexture(kernel, "_ReactionTexRW", reactionCoordTexture[0]);
                RenderTexture.active = pressureTextures[0];
                Controller.SimulationShader.SetTexture(kernel, "_PressureTexRW", pressureTextures[0]);

                Controller.SimulationShader.Dispatch(kernel, (maxDimensions.x + 31) >> 5, (maxDimensions.y + 7) >> 3, (maxDimensions.z + 7) >> 3);
                Swap(ref pressureTextures[0], ref pressureTextures[1]);
            }
        }

        private void Clear()
        {
            const int kernel = (int)SimKernel.Clear;

            RenderTexture.active = divergenceTexture;
            Controller.SimulationShader.SetTexture(kernel, "_DivergenceTexRW", divergenceTexture);
            RenderTexture.active = velocityTextures[1];
            Controller.SimulationShader.SetTexture(kernel, "_VelocityTexRW", velocityTextures[0]);
            RenderTexture.active = reactionCoordTexture[1];
            Controller.SimulationShader.SetTexture(kernel, "_ReactionTexRW", reactionCoordTexture[1]);
            RenderTexture.active = pressureTextures[0];
            Controller.SimulationShader.SetTexture(kernel, "_PressureTexRW", pressureTextures[1]);
            RenderTexture.active = vorticityTexture;
            Controller.SimulationShader.SetTexture(kernel, "_VorticityTexRW", vorticityTexture);

            Controller.SimulationShader.Dispatch(kernel, (maxDimensions.y + 7) >> 3, (maxDimensions.y + 7) >> 3, (maxDimensions.z + 7) >> 3);
        }

        #endregion

        /// <summary>
        /// Evolves the simulation.
        /// 
        /// This is mainly all GPU work, this method simply binds the registers and dispatches the kernels.
        /// </summary>
        internal void Evolve()
        {
            if ( !ShouldUpdateSimulation ) return;

            UpdateStaticConstantBuffer();
            UpdateDynamicConstantBuffer();

            UpdateEffectorBuffer();

            // Draw sphere as source of fluid.
            Inject();

            Advect();

            // Here we do MacCormack calculations.
            if (useMacCormack)
            {
                // Backwards advection and correction.
                AdvectBackwards();

                // 2nd-order correction.
                AdvectMacCormack();
            }

            if( useVorticityConfinement )
            {
                VorticityConfinement();
            }

            if ( isHighQuality )
            {
                // calc speed divergence
                Divergence();

                // jaccobi
                Jaccobian();

                // project
                Project();
            }
            else
            {
                EvolveAIO();
            }

            Swap(ref velocityTextures[0], ref velocityTextures[1]);
            Swap(ref reactionCoordTexture[0], ref reactionCoordTexture[1]);

            ClearEffectors();
        }

        /// <summary>
        /// Updates the static cb for the compute simulation.
        /// </summary>
        private void UpdateStaticConstantBuffer()
        {
            Controller.SimulationShader.SetInts("_Dimensions", dimensionData.Dimensions.x, dimensionData.Dimensions.y, dimensionData.Dimensions.z);
            Controller.SimulationShader.SetInts("_DimensionsMinusOne", dimensionData.DimensionsMinusOne.x, dimensionData.DimensionsMinusOne.y, dimensionData.DimensionsMinusOne.z);
            Controller.SimulationShader.SetInts("_QuarterDimensionsMinusOne", dimensionData.DimensionsMinusOne.x>>2, dimensionData.DimensionsMinusOne.y, dimensionData.DimensionsMinusOne.z);
            Controller.SimulationShader.SetVector("_RecipDimensions", dimensionData.RecipDimensions);
            Controller.SimulationShader.SetVector("_DimensionAspectRatio", dimensionData.DimensionsAspectRatio);
            Controller.SimulationShader.SetVector("_HalfVoxel", dimensionData.HalfVoxel);
            Controller.SimulationShader.SetFloat("_MaxDimension", dimensionData.MaxDimension);
            Controller.SimulationShader.SetFloat("_RecipMaxDimension", dimensionData.RecipMaxDimension);
        }

        /// <summary>
        /// Updates the dynamic cb for the compute simulation.
        /// </summary>
        private void UpdateDynamicConstantBuffer()
        {
            Controller.SimulationShader.SetFloat("_InvRadius", 1.0f / emitterRadius);
            Controller.SimulationShader.SetFloat("_ConfinementScale", confinementScale);

            Controller.SimulationShader.SetVector("_SourcePosition", Vector3.Scale((sourcePosition + Vector3.one) * 0.5f, dimensionData.Dimensions));
            Controller.SimulationShader.SetVector("_Gravity", UnityEngine.Physics.gravity * Time.deltaTime * affectedByGravity);
            Controller.SimulationShader.SetVector("_Translation", isTranslatingFixedGrid ? Vector3.Scale(translation, dimensionData.Dimensions) : Vector3.zero);

            float force = (emitFluid ? emitterForce : 0) * TimeScale;
            Controller.SimulationShader.SetVector("_Force", sourceDirection * force);
            Controller.SimulationShader.SetFloat("_ForceMag", force * sourceDirection.magnitude);

            Controller.SimulationShader.SetFloat("_ReactionSpeed", reactionSpeed * 0.016f);
            Controller.SimulationShader.SetFloat("_Viscosity", viscosity);
            Controller.SimulationShader.SetFloat("_TimeScale", TimeScale);
        }

        /// <summary>
        /// Tells us that the frame has ended, resets the is visible flag until the next frame is ready.
        /// </summary>
        internal void EndFrame()
        {
            isVisible = false;
        }

        /// <summary>
        /// Binds an effector to this simulation.
        /// </summary>
        /// <param name="fluidEffector">Effector to bind to simulation.</param>
        /// <param name="volume">Volume linked to simulaton.</param>
        internal void LinkEffector(Tools.Effector fluidEffector, Volume.FluidVolume volume)
        {
            Vector3 posTS = Vector3.Scale(volume.WorldToTextureMatrix.MultiplyPoint(fluidEffector.transform.position), dimensionData.Dimensions);
            float radius = 1.0f/(Vector3.Scale(volume.WorldToTextureMatrix.MultiplyPoint(fluidEffector.transform.position + Vector3.up * fluidEffector.WorldRadius), dimensionData.Dimensions) - posTS).magnitude;

            Vector3 packedDirection = fluidEffector.WorldSpaceDirection*0.5f + 0.5f*Vector3.one;

            if(linkedEffectors.ContainsKey(fluidEffector))
            {
                linkedEffectors[fluidEffector] = new Effector(posTS, radius, (int)fluidEffector.ModeMask, packedDirection, fluidEffector.Power, fluidEffector.Channel - 1);
            }
            else
            {
                linkedEffectors.Add(fluidEffector, new Effector(posTS, radius, (int)fluidEffector.ModeMask, packedDirection, fluidEffector.Power, fluidEffector.Channel - 1));
            }

            if((fluidEffector.ModeMask & Tools.Effector.EffectorModes.Receiver) != 0)
            {
                doWeNeedToReadBackEffectorData = true;
            }
        }

        /// <summary>
        /// Clears the set of effectors linked to this simulation, ready for the next frame.
        /// </summary>
        private void ClearEffectors()
        {
            if (doWeNeedToReadBackEffectorData)
            {
                Effector[] e = new Effector[effectors.Length];
                effectorBuffer.GetData(e);

                Tools.Effector[] linkedE = new Tools.Effector[linkedEffectors.Count];
                linkedEffectors.Keys.CopyTo(linkedE, 0);

                for (int i = 0; i < e.Length; i++)
                {
                    if (e.Length <= i || linkedE.Length <= i) continue;

                    if ((linkedE[i].ModeMask & Tools.Effector.EffectorModes.Receiver) != 0)
                    {
                        linkedE[i].ApplyHeat(e[i].temperature);
                    }
                }
            }

            doWeNeedToReadBackEffectorData = false;

            linkedEffectors.Clear();
        }

        /// <summary>
        /// Updates the GPU compute buffer with the CPU side effector data.
        /// </summary>
        private void UpdateEffectorBuffer()
        {
            var affectorsSet = linkedEffectors.Values.GetEnumerator();
            for ( int i = 0; i < kMaxEffectors; i++ )
            {
                if (affectorsSet.MoveNext())
                {
                    effectors[i] = affectorsSet.Current;
                }
                else
                {
                    effectors[i] = new Effector(Vector3.zero, 1.0f, 0, Vector3.up, 0, 1);
                }
                effectors[i].power *= TimeScale;
            }

            effectorBuffer.SetData(effectors);
        }

        /// <summary>
        /// Initialize this simulation, builds buffers and textures and populates the dimension data object.
        /// </summary>
        internal void Initialize()
        {
            maxDimensions = int3.FloorTo4(maxDimensions);

            dimensionData.RecalculateData(maxDimensions);

            effectorBuffer = new ComputeBuffer(kMaxEffectors, Effector.ByteStride);

            CreateVolumes();
        }

        /// <summary>
        /// Swap a pair of render textures, used in the buffer chain.
        /// </summary>
        /// <param name="res0"></param>
        /// <param name="res1"></param>
        protected void Swap(ref RenderTexture res0, ref RenderTexture res1)
        {
            RenderTexture res = res0;
            res0 = res1;
            res1 = res;
        }

        /// <summary>
        /// Creates a render texture volume for use in the compute simulation.
        /// </summary>
        /// <param name="x">Width</param>
        /// <param name="y">Height</param>
        /// <param name="z">Depth</param>
        /// <param name="n">Name</param>
        /// <param name="type">Render texture format to use.</param>
        /// <returns></returns>
        protected RenderTexture CreateRTVolume(int x, int y, int z, string n, RenderTextureFormat type)
        {
            var rt = new RenderTexture(x, y, 0, type)
            {
                name = n,
                isVolume = true,
                volumeDepth = z,
                enableRandomWrite = true,
                filterMode = FilterMode.Trilinear,
                wrapMode = TextureWrapMode.Clamp,
                
            };

            rt.DiscardContents();
            rt.Create();

            return rt;
        }

        /// <summary>
        /// Clears all the textures used in this simulation.
        /// </summary>
        public void Reset()
        {
            Clear();
        }
    }
}