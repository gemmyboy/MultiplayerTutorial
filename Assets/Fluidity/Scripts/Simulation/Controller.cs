/*
 * Copyright (c) 2013 LowLevelTech
 * Alex Dunn
 * llt@dunnalex.com
*/

using System;
using System.Linq;
using System.Collections.Generic;
using Fluidity.Auxilliary;
using Fluidity.Physics;
using Fluidity.Renderer;
using Fluidity.Volume;
using UnityEngine;

namespace Fluidity
{
    /// <summary>
    /// Map logical names to GPU programs, each used to render the system in a different state.
    /// Some of these are used for debugging purposes.
    /// </summary>
    public enum RenderMode
    {
        RaymarchOneChannel = 0,
        RaymarchTwoChannels,
        RaymarchThreeChannels,
        RaymarchFourChannels,
        DrawMasks,
        RayEntryPoint,
        RayExitPoint,
        RayDirection,
        Steps,
        Finalize,
        GrabScreen
    }

    /// <summary>
    /// How the fluid should transparency should be handled when rendering.
    /// </summary>
    public enum BlendMode
    {
        Alpha = 0,
        Additive,
    }

    /// <summary>
    /// How should we blend the edges of the volume, if at all.
    /// </summary>
    public enum EdgeBlendMode
    {
        None = 0,
        MaskTexture,
        EdgeFade
    }

    /// <summary>
    /// Singleton controller for all fluid objects.  
    /// Holds shaders, simulations, data and performs error checking required to run the simulations and render them.
    /// 
    /// Sorting is also performed here.
    /// </summary>
    [AddComponentMenu("Fluidity/Scene Controller Component...")]
    public class Controller : Singleton<Controller>
    {
        /// <summary>
        /// Global quality settings for rendering.
        /// </summary>
        public enum QualitySettings
        {
            VeryLow     = 1,
            Low         = 2,
            Medium      = 4,
            High        = 8,
            VeryHigh    = 10,
            Ultra       = 12
        }

        /// <summary>
        /// The compute shader used to run the simulations.
        /// </summary>
        [SerializeField] private ComputeShader simulationShader;

        /// <summary>
        /// The compute shader used to calculate fluid lighting.
        /// </summary>
        [SerializeField]  private ComputeShader lightingShader;

        /// <summary>
        /// The volume light shader.
        /// </summary>
        [SerializeField] private Shader volumeLightShader;

        /// <summary>
        /// Texture to use when applying heat haze.
        /// </summary>
        [SerializeField] private Texture2D noiseTexture;

        /// <summary>
        /// A container for all the simulations we want to run.
        /// DEBT: This should be private... 
        /// </summary>
        public Physics.Simulation[] simulations;

        /// <summary>
        /// Global switch for simulations.
        /// 
        /// Can be used to pause all the simulations at once.
        /// </summary>
        [SerializeField] private bool globalRunSimulations = true;

        /// <summary>
        /// A seperate time scale used only for fluids.
        /// </summary>
        [SerializeField] private float fluidTimeScale = 1;

        /// <summary>
        /// Globel render quality setting.
        /// </summary>
        [SerializeField] private QualitySettings globalRenderQuality = QualitySettings.Medium;

        /// <summary>
        /// Global render mode setting.
        /// </summary>
        [SerializeField] private RenderMode globalRenderMode;

        /// <summary>
        /// Global switch to draw all render masks.
        /// </summary>
        [SerializeField] private bool globalDrawRenderMasks;

        #region Static Accessors
        /// <summary>
        /// A container for all the active fluid volumes in the scene, used for intersection tests.
        /// </summary>
        private static readonly List<BaseVolume> volumes = new List<BaseVolume>();
        
        /// <summary>
        /// Static getter for all the fluid volumes currently registered in the scene.
        /// </summary>
        public static List<BaseVolume> Volumes { get { return volumes; } }

        /// <summary>
        /// Static accessor for the global render mode, sets to draw masks if global draw render masks flag is set.
        /// </summary>
        public static RenderMode GlobalRenderMode
        {
            get { return self.globalDrawRenderMasks ? RenderMode.DrawMasks : self.globalRenderMode; }
            set { self.globalRenderMode = value; }
        }

        /// <summary>
        /// Static accessor for run simulations flag.
        /// </summary>
        public static bool GlobalRunSimulations
        {
            get { return self.globalRunSimulations; }
            set { self.globalRunSimulations = value; }
        }

        /// <summary>
        /// Static accessor for render quality.
        /// </summary>
        public static QualitySettings GlobalRenderQuality
        {
            get { return self.globalRenderQuality; }
            set { self.globalRenderQuality = value; }
        }

        /// <summary>
        /// Static accessor for fluid time scale.
        /// </summary>
        public static float TimeScale
        {
            get { return self.fluidTimeScale; }
            set { self.fluidTimeScale = value; }
        }

        /// <summary>
        /// Static getter for global render quality in a format ready for shaders.
        /// </summary>
        internal static float ConditionedGlobalRenderQuality { get { return (float) self.globalRenderQuality /4; } }

        /// <summary>
        /// Static getter for total number of simulations.
        /// </summary>
        public static int SimulationCount
        {
            get
            {
                if (self.simulations == null)
                {
                    self.simulations = new Simulation[0];
                }
                return self.simulations.Length;
            }
        }

        /// <summary>
        /// Static getter for the simulations associated with this controller.
        /// </summary>
        internal static Simulation[] Simulations { get { return self.simulations; }}

        /// <summary>
        /// Static getter for the compute shader which performs all the simulation updates.
        /// </summary>
        internal static ComputeShader SimulationShader    { get { return self.simulationShader; } }

        /// <summary>
        /// Static getter for the compute shader which performs all the lighting updates.
        /// </summary>
        internal static ComputeShader LightingShader { get { return self.lightingShader; } }

        /// <summary>
        /// Static getter for the rendering shader which renders lights.
        /// </summary>
        internal static Shader VolumeLightShader { get { return self.volumeLightShader; } }

        /// <summary>
        /// Getter for the noise texture used when applying heat haze to an object.
        /// </summary>
        internal static Texture NoiseTexture { get { return self.noiseTexture; } }

        /// <summary>
        /// Queries all the fluid volumes held by this controller agains an axis aligned bounding box.
        /// </summary>
        /// <param name="bounds">AABB to query volumes with.</param>
        /// <returns>A list of volumes that are instersecting the AABB provided.</returns>
        public static List<Volume.FluidVolume> QueryVolumeIntersection(Bounds bounds)
        {
            List<Volume.FluidVolume> o = new List<Volume.FluidVolume>();

            foreach (Volume.BaseVolume v in volumes)
            {
                if (v as Volume.FluidVolume == null) continue; 

                if (BoxIntersect(v.Bounds, bounds))
                {
                    o.Add(v as Volume.FluidVolume);
                }
            }

            return o;
        }

        /// <summary>
        /// AABB to AABB intersection check.
        /// </summary>
        /// <param name="a">AABB 1 to query.</param>
        /// <param name="b">AABB 2 to query.</param>
        /// <returns>True if the bounds overlap. False otherwise.</returns>
        private static bool BoxIntersect(Bounds a, Bounds b)
        {
            return  b.min.x >= a.min.x && b.max.x <= a.max.x &&
                    b.min.y >= a.min.y && b.max.y <= a.max.y &&
                    b.min.z >= a.min.z && b.max.z <= a.max.z ||
                    a.max.x >= b.min.x && a.min.x <= b.max.x &&
                    (a.max.y >= b.min.y && a.min.y <= b.max.y &&
                    (a.max.z >= b.min.z && a.min.z <= b.max.z));
        }

        /// <summary>
        /// Returns the simulation at the specified index. 
        /// NOTE : Performs error checking.
        /// </summary>
        /// <param name="index">Desired simulation index.</param>
        /// <returns>Simulation instance at index.</returns>
        public static Physics.Simulation GetSimulation(int index)
        {
            if (self.simulations == null)
            {
                throw new Exception("Something has gone wrong with the simulation setup.");
            }

            if (index < 0 || index > self.simulations.Length)
            {
                throw new ArgumentOutOfRangeException("index", "Simulation index is out of range.");
            }

            return self.simulations[index];
        }

        /// <summary>
        /// Gets the set of names which belong to the active simulations held by this controller.
        /// </summary>
        /// <returns>Simulation names.</returns>
        public static string[] GetSimulationNames()
        {
            if (self.simulations == null)
            {
                throw new Exception("Something has gone wrong with the simulation setup.");
            }

            string[] names = new string[self.simulations.Length];

            for (int i = 0; i < names.Length; i++ )
            {
                names[i] = self.simulations[i].name;
            }

            return names;
        }

        /// <summary>
        /// Adds a fluid volume to the collection of volumes held by this controller.
        /// </summary>
        /// <param name="volume">Volume to add to collection.</param>
        public static void AddVolume(BaseVolume volume)
        {
            if (volume == null) return;

            volumes.Add(volume);
        }

        /// <summary>
        /// Removes a fluid volume from the collection held by this controller.
        /// </summary>
        /// <param name="volume">Instance of the volume to remove from collection.</param>
        public static void RemoveVolume(BaseVolume volume)
        {
            if (volume == null) return;

            volumes.Remove(volume);
        }

        #endregion

        #region Unity API

        /// <summary>
        /// Unity callback,
        /// 
        /// Perform some initialization on the simulations and create the renderers required to render the volumes.
        /// </summary>
        private void Awake()
        {
            InitializeSimulations();

            if(noiseTexture == null)
            {
                noiseTexture = Resources.Load("Noise", typeof(Texture2D)) as Texture2D;
            }
        }

        /// <summary>
        /// Unity callback,
        /// 
        /// Kills all the simulations connected to this controller.
        /// </summary>
        private void OnDestroy()
        {
            KillSimulations();
        }

        /// <summary>
        /// Unity callback,
        /// 
        /// Evolve the simulations if the global run simulations flag is set.
        /// </summary>
        private void Update()
        {
            if (!globalRunSimulations) return;

            EvolveSimulations();
        }

        /// <summary>
        /// Unity callback,
        /// 
        /// Ends the simulation frame.
        /// </summary>
        private void LateUpdate()
        {
            EndSimulationFrame();
        }

        #endregion

        #region Simulation Stuff
        /// <summary>
        /// Performs some initialization on all the simulations.
        /// </summary>
        private void InitializeSimulations()
        {
            if (simulations == null) return;

            foreach (Physics.Simulation simulation in simulations)
            {
                simulation.Initialize();
            }
        }

        /// <summary>
        /// Destroys all the simulations.
        /// </summary>
        private void KillSimulations()
        {
            if (simulations == null) return;

            foreach (Physics.Simulation simulation in simulations)
            {
                simulation.Destroy();
            }
        }

        /// <summary>
        /// Advance the simulations one step in the physics simulation.
        /// </summary>
        private void EvolveSimulations()
        {
            if (simulations == null) return;

            foreach (Physics.Simulation simulation in simulations)
            {
                simulation.Evolve();
            }
        }

        /// <summary>
        /// Informs the simulations that it's the end of the frame.
        /// </summary>
        private void EndSimulationFrame()
        {
            if (simulations == null) return;

            foreach (Physics.Simulation simulation in simulations)
            {
                simulation.EndFrame();
            }
        }

        #endregion
    }
}