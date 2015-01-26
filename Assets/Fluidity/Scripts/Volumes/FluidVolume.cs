/*
 * Copyright (c) 2013 LowLevelTech
 * Alex Dunn
 * llt@dunnalex.com
*/

using Fluidity.Auxilliary;
using Fluidity.Renderer;
using UnityEngine;

namespace Fluidity.Volume
{
    /// <summary>
    /// A fluid volume can be linked to a simulation and rendererd in the world.
    /// </summary>
    [AddComponentMenu("Fluidity/Fluid Volume...")]
    public class FluidVolume : BaseVolume
    {
        /// <summary>
        /// Simulation indexof the fluid system this volume is linked to.
        /// </summary>
        [SerializeField] private int simulationIndex;

        /// <summary>
        /// A light can be linked to this volume to give the impression this fluid is generating light.
        /// </summary>
        [SerializeField] private Light myLight;

        /// <summary>
        /// Use heat haze option, enable this to enable heat haze rendering.
        /// </summary>
        [SerializeField] private bool useHeatHaze;

        /// <summary>
        /// When heat haze is enabled this will set the number of taps to sample the back buffer in order to get the soft diffusion effect.
        /// </summary>
        [SerializeField] private int heatHazeTaps = 1;

        /// <summary>
        /// When heat haze is enabled this will control the strength (opacity) of the heat haze.
        /// </summary>
        [SerializeField] private float heatHazeStrength = 2;

        /// <summary>
        /// When heat haze is enabled this will control how much the heat haze peturbs the scene behind it.
        /// </summary>
        [SerializeField] private float heatHazeSpread = 0.03f;

        /// <summary>
        /// Instance of the linked simulation.
        /// </summary>
        private Physics.Simulation mySimulation;

        /// <summary>
        /// Is this volume linked to a translating fixed grid simulation?
        /// </summary>
        private bool isTranslatingFixedGrid;

        /// <summary>
        /// Cached original intensity.  We will be modulating this when a light is connected.
        /// </summary>
        private float originalLightIntensity;

        /// <summary>
        /// Getter for the heat haze option.
        /// </summary>
        public bool UseHeatHaze
        {
            get { return useHeatHaze; }
            set 
            {
                if (useHeatHaze != value)
                {
                    IsDirty = true;
                    useHeatHaze = value;
                }
            }
        }

        /// <summary>
        /// Accessor for the fluid simulation  index this volume is linked to.
        /// </summary>
        public int SimulationIndex
        {
            get { return simulationIndex; } 
            set
            {
                if (simulationIndex != value)
                {
                    IsDirty = true;
                    simulationIndex = value;
                }
            }
        }

        /// <summary>
        /// Returns true when a light has been linked to this volume.
        /// </summary>
        public bool ShouldEmitLight               { get { return myLight != null; } }

        /// <summary>
        /// Accessor for the light currently linked to this simulation (if one exists)
        /// </summary>
        public Light LinkedLight
        {
            get { return myLight; }
            set
            {
                if (myLight != value)
                {
                    IsDirty = true;
                    myLight = value;
                }
            }
        }

        /// <summary>
        /// Getter for the simulation this volume is linked to.
        /// </summary>
        public Physics.Simulation Simulation
        {
            get { return mySimulation; }
        }

        /// <summary>
        /// Getter for the amount of heat haze taps.
        /// </summary>
        public int HeatHazeTaps
        {
            get { return Mathf.Max(1, heatHazeTaps); }
            set
            {
                if (heatHazeTaps != value)
                {
                    IsDirty = true;
                    heatHazeTaps = Mathf.Max(1, value);
                }
            }
        }

        /// <summary>
        /// Getter for the strength of the heat haze.
        /// </summary>
        public float HeatHazeStrength
        {
            get { return heatHazeStrength; }
            set
            {
                if (heatHazeStrength != value)
                {
                    IsDirty = true;
                    heatHazeStrength = value;
                }
            }
        }

        /// <summary>
        /// Getter for the heat haze spread amount.
        /// </summary>
        public float HeatHazeSpread
        {
            get { return heatHazeSpread; }
            set
            {
                if (heatHazeSpread != value)
                {
                    IsDirty = true;
                    heatHazeSpread = value;
                }
            }
        }

        /// <summary>
        /// Is this volume in control of a translating fixed grid simulation?
        /// </summary>
        public bool IsTranslatingFixedGrid
        {
            get { return isTranslatingFixedGrid; }
            set
            {
                if (isTranslatingFixedGrid != value)
                {
                    IsDirty = true;
                    isTranslatingFixedGrid = value;
                }
            }
        }

        /// <summary>
        /// Volume texture to perform raymarching on.
        /// </summary>
        public override Texture VolumeTexture { get { return Simulation.ReactionCoordTexture; } }

        /// <summary>
        /// Dimensions of volume texture.
        /// </summary>
        protected override int3 VolumeDimensions { get{ return new int3(Simulation.ReactionCoordTexture.width, Simulation.ReactionCoordTexture.height, Simulation.ReactionCoordTexture.volumeDepth); } }

        #region Unity API
        /// <summary>
        /// Unity callback,
        /// 
        /// Draws the additional fluid related gizmos for this object.
        /// </summary>
        /// <param name="colour">Which colour to render with.</param>
        protected override void DrawGizmos(Color colour)
        {
            mySimulation = Controller.GetSimulation(simulationIndex);

            Vector3 worldSpaceSourcePosition = transform.position + transform.rotation * Vector3.Scale(mySimulation.sourcePosition, transform.lossyScale * 0.5f);
            Gizmos.DrawIcon(worldSpaceSourcePosition, "FluidityLogo.png", true);
            Vector3 worldSpaceSourceDirection = transform.TransformDirection( mySimulation.sourceDirection );
            Helpers.DrawArrow(worldSpaceSourcePosition, worldSpaceSourceDirection);

            base.DrawGizmos(colour);
        }
        
        /// <summary>
        /// Unity callback,
        /// 
        /// Bind the simulation with this volume, and link the light source if one has been supplied.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            mySimulation = Controller.GetSimulation(simulationIndex);

            if(myLight != null)
            {
                originalLightIntensity = myLight.intensity;
            }

            SetName();
        }

        private Vector3 previousPosition;
        /// <summary>
        /// Unity callback,
        /// 
        /// Update the light postion, based on the position of the emitter from the simulation.
        /// </summary>
        protected override void Update()
        {
            mySimulation = Controller.GetSimulation(simulationIndex);

            if (mySimulation == null) return;

            mySimulation.isTranslatingFixedGrid = IsTranslatingFixedGrid;

            UpdateVolumeLight();

            base.Update();
        }

        /// <summary>
        /// Unity callback,
        /// 
        /// Updates some translating fixed grid data, and calls the base method.
        /// </summary>
        protected override void LateUpdate()
        {
            if (IsTranslatingFixedGrid)
            {
                mySimulation.translation = Vector3.Scale((transform.position - previousPosition), new Vector3(1.0f / transform.lossyScale.x, 1.0f / transform.lossyScale.y, 1.0f / transform.lossyScale.z));
                previousPosition = transform.position;
                transform.rotation = Quaternion.identity;
            }

            base.LateUpdate();
        }

        #endregion

        #region Initialization
        /// <summary>
        /// Set the name of this object to include the simulation it has been linked to.
        /// This allows us to see quickly in the hierarchy which simulation this object is linked to.
        /// </summary>
        private void SetName()
        {
            name += string.Format(" - {0} ({1})", mySimulation.name, simulationIndex);
        }

        #endregion

        #region Helper
        /// <summary>
        /// Gets the simulation emitter source position in local space.
        /// </summary>
        /// <returns>Emitter position.</returns>
        private Vector3 GetSourcePosition()
        {
            Vector3 output = mySimulation.sourcePosition;
            output.y += 0.5f;
            return output;
        }

        /// <summary>
        /// Updates the light colour and intensity to match the gradient used for this fluid volume,
        /// and modulate it a little.
        /// </summary>
        /// <param name="updatedColour"></param>
        private void UpdateLightAttributes(Color updatedColour)
        {
            myLight.color = Color.Lerp(myLight.color, updatedColour * updatedColour.a, Time.deltaTime * 2.5f);
            myLight.intensity = updatedColour.a * originalLightIntensity;
        }

        /// <summary>
        /// Simple method that very crudely updates a light to give the impression that it's 
        /// colour and intensity are linked to this volume.
        /// </summary>
        private void UpdateVolumeLight()
        {
            if (!ShouldEmitLight) return;

            myLight.transform.localPosition = GetSourcePosition();
            myLight.transform.position += transform.up;

            // Less accurate method but still pleasing results.  
            Physics.Simulation ourSimulation = Simulation;
            if (ourSimulation == null)
            {
                Debug.LogWarning("No simulation found for the supplied index.");
                return;
            }

            Color lightColour = new Color(0, 0, 0, 0);
            if (ourSimulation.emitterForce > 0 && ourSimulation.emitFluid)
            {
                if (Random.value > 0.4f) return;

                lightColour = colourGradient[0].Evaluate(Random.Range(0.0f, 1.0f)) * 2;
                lightColour.a = ourSimulation.emitterForce * 2; // Magic numbers... Yay!
            }

            UpdateLightAttributes(lightColour);
        }
        #endregion


        #region Rendering
        /// <summary>
        /// Binds the necesarry extra parameters to the shader that will be required to render fluid.
        /// </summary>
        protected override void PerformRaymarch()
        {
            base.PerformRaymarch();

            Simulation.SetVisible();

            if (UseHeatHaze)
            {
                renderer.sharedMaterial.SetTexture("_HeatHazeOffsetTex", Controller.NoiseTexture);

                renderer.sharedMaterial.SetFloat("_HeatHazeDiffusionTaps", HeatHazeTaps);
                renderer.sharedMaterial.SetFloat("_RecipHeatHazeDiffusionTaps", 1.0f / HeatHazeTaps);
                renderer.sharedMaterial.SetFloat("_HeatHazeStrength", Mathf.Max(0, HeatHazeStrength + 1));
                renderer.sharedMaterial.SetFloat("_HeatHazeSpread", HeatHazeSpread);

                renderer.sharedMaterial.DisableKeyword("FLUIDITY_DISABLE_HEAT_HAZE");
                renderer.sharedMaterial.EnableKeyword("FLUIDITY_ENABLE_HEAT_HAZE");
            }
            else
            {
                renderer.sharedMaterial.EnableKeyword("FLUIDITY_DISABLE_HEAT_HAZE");
                renderer.sharedMaterial.DisableKeyword("FLUIDITY_ENABLE_HEAT_HAZE");
            }
        }
        #endregion
    }
}
