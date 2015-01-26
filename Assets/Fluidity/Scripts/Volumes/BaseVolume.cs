/*
 * Copyright (c) 2013 LowLevelTech
 * Alex Dunn
 * llt@dunnalex.com
*/

using System.Collections;
using System.Collections.Generic;
using Fluidity.Auxilliary;
using Fluidity.Renderer;
using UnityEngine;

namespace Fluidity.Volume
{
    /// <summary>
    /// The abstract type for a renderable volume.
    /// </summary>
    public abstract class BaseVolume : MonoBehaviour
    {
        /// <summary>
        /// Container for the unitys gradient object <see cref="UnityEngine.Gradient"/>
        /// It seems the standard object doesnt work with serialized properties and arrays.
        /// </summary>
        [System.Serializable]
        public class Gradient
        {
            [SerializeField] protected UnityEngine.Gradient colourGradient;
            public Color Evaluate(float t)
            {
                return colourGradient.Evaluate(t);
            }
        }

        /// <summary>
        /// Which blend mode to use when rendering this volume.
        /// </summary>
        [SerializeField] private BlendMode blendMode;

        /// <summary>
        /// Which blend mode to use when rendering this volume.
        /// </summary>
        [SerializeField] private EdgeBlendMode edgeBlendMode;

        /// <summary>
        /// Use self shadowing.
        /// </summary>
        [SerializeField] private bool useSelfShadowing;

        /// <summary>
        /// Gas density, used for self shadowing.
        /// </summary>
        [SerializeField]  private float lightDensity = 1.0f;

        /// <summary>
        /// Transform of the light to use when calculating self shadowing.
        /// </summary>
        [SerializeField] private Transform sun;

        /// <summary>
        /// The noise volume to use when noise peturbation is enabled.
        /// </summary>
        [SerializeField] private Texture3D noiseVolume;

        /// <summary>
        /// Noise frequency.
        /// </summary>
        [SerializeField] private float noiseFrequency = 1;

        /// <summary>
        /// Noise Amplitude.
        /// </summary>
        [SerializeField] private float noiseAmplitude = 1;

        /// <summary>
        /// Noise animation speeds, seperable axis.
        /// </summary>
        [SerializeField] private Vector3 noiseAnimation;

        /// <summary>
        /// Turns noise peturbation on/off.
        /// </summary>
        [SerializeField] private bool useNoisePeturbation;

        /// <summary>
        /// Edge fade power, used when  rendering with edge fade enabled.
        /// </summary>
        [SerializeField] private float edgeFadePower = 5;
        
        /// <summary>
        /// For each renderable channel there can be a single gradient linked to it. A maximum of 4 channels is allowed.
        /// </summary>
        [SerializeField] protected Gradient[] colourGradient = new Gradient[4];

        /// <summary>
        /// Current number of channels to render with.
        /// </summary>
        [SerializeField] protected int channels = 1;

        /// <summary>
        /// Intensity modifier.  Really in the shader this just multiplies the reaction coordinate 
        /// as a constant, to scale the gradient before sampling it.
        /// </summary>
        [SerializeField] protected float gradientScale = 1;

        /// <summary>
        /// Opacity modifier, here for ease of access, directly modifies the strength of opacity of volume.
        /// </summary>
        [SerializeField] protected float opacityModifier = 1;

        /// <summary>
        /// Flag for active level of detail.
        /// </summary>
        [SerializeField] protected bool useActiveLod;

        /// <summary>
        /// Level of detail curve, camera distance over render steps.
        /// </summary>
        [SerializeField] protected AnimationCurve lodCurve = new AnimationCurve(defaultKeys);

        /// <summary>
        /// A rendermask to apply to the renderer.
        /// </summary>
        [SerializeField] protected Texture renderMask;

        /// <summary>
        /// The maximum number of rendersteps.  Use this only when active LOD is turned off.
        /// </summary>
        [SerializeField] protected int passiveRenderSteps = 20;

        /// <summary>
        /// A dirty transform object, use this to query if this transform has changed.
        /// </summary>
        private readonly DirtyTransform dirtyTransform = new DirtyTransform();

        /// <summary>
        /// The colour gradient texture, used in the render shader to map colours to the reaction coordinates.
        /// </summary>
        [HideInInspector]  private Texture2D colourGradientTex;

        /// <summary>
        /// Array used to hold the pixels from the colour gradient tex.
        /// </summary>
        [HideInInspector]  private Color[] gradientTextureColours;

        /// <summary>
        /// Opacity map volume texture, used for self shadowing.
        /// </summary>
        [HideInInspector] private RenderTexture opacityMap;

        /// <summary>
        /// Some default animation keys for the active LOD.
        /// </summary>
        private static readonly Keyframe[] defaultKeys = new[]
                                                      {
                                                          new Keyframe(0.0f, 2.0f), 
                                                          new Keyframe(10.0f, 30.0f), 
                                                          new Keyframe(20.0f, 20.0f), 
                                                          new Keyframe(100.0f, 1.0f)
                                                      };
        /// <summary>
        /// Vertices for the optimized cube used to render the stencil masks for this volume.
        /// </summary>
        private static readonly Vector3[] v = new[]
                                            {
                                                new Vector3(-0.5f, -0.5f, -0.5f),
                                                new Vector3( 0.5f, -0.5f, -0.5f),
                                                new Vector3( 0.5f,  0.5f, -0.5f),
                                                new Vector3(-0.5f,  0.5f, -0.5f),

                                                new Vector3(-0.5f, -0.5f,  0.5f),
                                                new Vector3( 0.5f, -0.5f,  0.5f),
                                                new Vector3( 0.5f,  0.5f,  0.5f),
                                                new Vector3(-0.5f,  0.5f,  0.5f)
                                            };

        /// <summary>
        /// The colour of this objects gizmo when selected.
        /// </summary>
        private static readonly Color SelectedColour = new Color(0.9f, 0.3f, 0.4f, 1);

        /// <summary>
        /// The colour of this objects gizmo when not selected.
        /// </summary>
        private static readonly Color UnselectedColour = new Color(0.3f, 0.9f, 0.4f, 1);

        /// <summary>
        /// Dirty state for the parameters cbuffer.  In future could maybe break this down even further into more cbuffers.
        /// </summary>
        protected internal bool IsDirty             { get; protected set; }

        /// <summary>
        /// Dirty state for the gradient texture used when rendering the volume.
        /// </summary>
        protected internal bool IsGradientDirty     { get; protected set; }

        /// <summary>
        /// Accessor for the current render steps used when raymarching this volume.
        /// </summary>
        public int CurrentRenderSteps { get; private set; }

        /// <summary>
        /// The current camera distance, used for active LOD.
        /// </summary>
        public float CurrentCameraDistance { get; private set; }

        /// <summary>
        /// The AABB bounds of this volume, used for culling and effectors.
        /// </summary>
        internal Bounds Bounds { get; private set; }

        /// <summary>
        /// Flag which controls whether or not to render this object, used to determine if the gameobject/component is enabled/disabled.
        /// </summary>
        internal bool ShouldRender { get; private set; }

        /// <summary>
        /// Texture generated from the gradients used when raymarching the fluid.
        /// </summary>
        internal Texture2D GradientTexture { get { return colourGradientTex; } }

        /// <summary>
        /// The shader used to render the volumes.
        /// </summary>
        [SerializeField]
        private Material[] renderMaterials;

        /// <summary>
        /// Getter for the active LOD curve.
        /// NOTE : Just calling this getter will trigger a dirty state CBuffer bind.
        /// </summary>
        public AnimationCurve ActiveLODCurve
        {
            get 
            { 
                IsDirty = true;
                return lodCurve;
            }
        }

        /// <summary>
        /// Accessor for the number of render steps to use when not in active LOD mode.
        /// </summary>
        public int PassiveRenderSteps
        {
            get { return passiveRenderSteps; }
            set
            {
                if (passiveRenderSteps != value)
                {
                    IsDirty = true;
                    passiveRenderSteps = value;
                }
            }
        }

        /// <summary>
        /// Accessor for the use active LOD mode.
        /// </summary>
        public bool UseActiveLOD
        {
            get { return useActiveLod; }
            set
            {
                if (useActiveLod != value)
                {
                    IsDirty = true;
                    useActiveLod = value;
                }
            }
        }

        /// <summary>
        /// Getter for the blend mode to use when rendering this volume.
        /// </summary>
        public BlendMode BlendMode
        {
            get { return blendMode; }
            set
            {
                if (blendMode != value)
                {
                    IsDirty = true;
                    blendMode = value;
                }
            }
        }

        /// <summary>
        /// Accessor for the edge blend mode used when rendering this volume.
        /// </summary>
        public EdgeBlendMode EdgeBlendMode
        {
            get { return edgeBlendMode; }
            set
            {
                if (edgeBlendMode != value)
                {
                    IsDirty = true;
                    edgeBlendMode = value;
                }
            }
        }

        /// <summary>
        /// Accessor for the self shadowing mode used when rendering this volume.
        /// </summary>
        public bool UseSelfShadowing
        {
            get { return useSelfShadowing; }
            set
            {
                if (useSelfShadowing != value)
                {
                    IsDirty = true;
                    useSelfShadowing = value;
                }
            }
        }

        /// <summary>
        /// Accessor for the light density when using self shadowing.
        /// </summary>
        public float LightDensity
        {
            get { return lightDensity; }
            set
            {
                if (lightDensity != value)
                {
                    IsDirty = true;
                    lightDensity = value;
                }
            }
        }



        /// <summary>
        /// Accessor for the noise peturbation mode used when rendering this volume.
        /// </summary>
        public bool UseNoisePeturbation
        {
            get { return useNoisePeturbation; }
            set
            {
                if (useNoisePeturbation != value)
                {
                    IsDirty = true;
                    useNoisePeturbation = value;
                }
            }
        }

        /// <summary>
        /// Accessor for the noise volume 3d texture.
        /// </summary>
        public Texture3D NoiseVolume
        {
            get { return noiseVolume; }
            set
            {
                if (noiseVolume != value)
                {
                    IsDirty = true;
                    noiseVolume = value;
                }
            }
        }

        /// <summary>
        /// Accesor for noise frequency.
        /// </summary>
        public float NoiseFrequency
        {
            get { return noiseFrequency; }
            set
            {
                if( noiseFrequency != value )
                {
                    IsDirty = true;
                    noiseFrequency = value;
                }
            }
        }

        /// <summary>
        /// Accesor for noise Amplitude.
        /// </summary>
        public float NoiseAmplitude
        {
            get { return noiseAmplitude; }
            set
            {
                if (noiseAmplitude != value)
                {
                    IsDirty = true;
                    noiseAmplitude = value;
                }
            }
        }

        /// <summary>
        /// Accesor for noise Animation.
        /// </summary>
        public Vector3 NoiseAnimation
        {
            get { return noiseAnimation; }
            set
            {
                if (noiseAnimation != value)
                {
                    IsDirty = true;
                    noiseAnimation = value;
                }
            }
        }

        /// <summary>
        /// Accessor for the transform of the sun when using self shadowing.
        /// </summary>
        public Transform Sun
        {
            get { return sun; }
            set
            {
                if (sun != value)
                {
                    IsDirty = true;
                    sun = value;
                }
            }
        }

        /// <summary>
        /// Accessor for the edge fade power.
        /// </summary>
        public float EdgeFadePower
        {
            get { return edgeFadePower; }
            set
            {
                if (edgeFadePower != value)
                {
                    IsDirty = true;
                    edgeFadePower = value;
                }
            }
        }

        /// <summary>
        /// Getter for the colour gradient array used to represent each render channel.
        /// NOTE : Just calling this getter will trigger a dirty update of the gradient texture, use it wisely.
        /// </summary>
        public Gradient[] ColourGradient
        {
            get
            {
                IsDirty = IsGradientDirty = true;
                return colourGradient;
            }
        }

        /// <summary>
        /// Accessor for the gradient scale.
        /// </summary>
        public float GradientScale
        {
            get { return gradientScale; }
            set
            {
                if (gradientScale != value)
                {
                    IsDirty = true;
                    gradientScale = value;
                }
            }
        }

        /// <summary>
        /// Accessor for the opacity modifier.
        /// </summary>
        public float OpacityModifier
        {
            get { return opacityModifier; }
            set
            {
                if (opacityModifier != value)
                {
                    IsDirty = true;
                    opacityModifier = value;
                }
            }
        }

        /// <summary>
        /// Accessor for the number of channels.
        /// </summary>
        public int Channels
        {
            get { return channels; }
            set
            {
                if (channels != value)
                {
                    IsDirty = true;
                    IsGradientDirty = true;
                    channels = value;
                }
            }
        }

        /// <summary>
        /// Accessor for the render mask texture.
        /// </summary>
        public Texture RenderMask
        {
            get { return renderMask; }
            set
            {
                if (renderMask != value)
                {
                    IsDirty = true;
                    renderMask = value;
                }
            }
        }

        /// <summary>
        /// Calculates the world to texture space matrix and returns it.
        /// </summary>
        public Matrix4x4 WorldToTextureMatrix
        {
            get
            {
                Matrix4x4 offset = Matrix4x4.TRS
                (
                    Vector3.one * 0.5f,
                    Quaternion.identity,
                    Vector3.one
                );

                return offset * transform.worldToLocalMatrix;
            }
        }

        /// <summary>
        /// Volume texture to perform raymarching on.
        /// </summary>
        public abstract Texture VolumeTexture { get; }

        /// <summary>
        /// Dimensions of volume texture.
        /// </summary>
        protected abstract int3 VolumeDimensions { get; }

        /// <summary>
        /// Draws the volume gizmos.
        /// </summary>
        /// <param name="colour"></param>
        protected virtual void DrawGizmos(Color colour)
        {
            CalculateAABB();

            Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Gizmos.matrix = rotationMatrix;

            Gizmos.color = colour.SetAlpha(1.0f);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            Gizmos.color = colour.SetAlpha(0.2f);
            Gizmos.DrawCube(Vector3.zero, Vector3.one);
        }

        #region Unity API
        protected virtual void Awake()
        {
            if( renderer == null )
            {
                gameObject.AddComponent<MeshRenderer>();
                renderer.castShadows = false;
                renderer.receiveShadows = false;
            }

            if( GetComponent<MeshFilter>() == null )
            {
                gameObject.AddComponent<MeshFilter>().sharedMesh = Helpers.CreateCube();
            }

            renderMaterials = new[]
            {
                new Material( Shader.Find("Hidden/Fluidity/Render1") ), 
                new Material( Shader.Find("Hidden/Fluidity/Render2") ), 
                new Material( Shader.Find("Hidden/Fluidity/Render3") ), 
                new Material( Shader.Find("Hidden/Fluidity/Render4") )
            };
        }

        /// <summary>
        /// Unity callback,
        /// 
        /// Draws the volume gizmo with the unselected colour.
        /// </summary>
        private void OnDrawGizmos()
        {
            DrawGizmos(UnselectedColour);
        }

        /// <summary>
        /// Unity callback,
        /// 
        /// Draws the volume gizmo with the selected colour.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            DrawGizmos(SelectedColour);
        }

        /// <summary>
        /// Unity callback,
        /// 
        /// When the object is enabled we might need to know the AABB immediately.  So here we calculate it.
        /// </summary>
        protected virtual void OnEnable()
        {
            IsDirty = true;
            IsGradientDirty = true;

            ShouldRender = true;
            CalculateAABB();
            Controller.AddVolume(this);
        }

        /// <summary>
        /// Unity callback,
        /// 
        /// When the object is disabled we need to stop the renderer from rendering it.
        /// </summary>
        protected virtual void OnDisable()
        {
            ShouldRender = false;
            Controller.RemoveVolume(this);
        }

        /// <summary>
        /// Unity callback,
        /// 
        /// Updating the AABB can be expensive, so we only do it when we need to, when the transform has changed.
        /// </summary>
        protected virtual void Update()
        {
            if (dirtyTransform.Set(transform))
            {
                CalculateAABB();
            }

            UpdateColourGradientTexture();
        }

        /// <summary>
        /// Unity callback,
        /// </summary>
        protected virtual void LateUpdate()
        {
        }

        protected virtual void OnWillRenderObject()
        {
            RenderMe();
        }
        #endregion

        #region Aux
        /// <summary>
        /// Calculates the AABB for this volume.
        /// </summary>
        private void CalculateAABB()
        {
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = -min;

            for (int i = 0; i < 8; i++)
            {
                Vector3 vWS = transform.localToWorldMatrix.MultiplyPoint(v[i]);

                min.x = Mathf.Min(min.x, vWS.x);
                min.y = Mathf.Min(min.y, vWS.y);
                min.z = Mathf.Min(min.z, vWS.z);

                max.x = Mathf.Max(max.x, vWS.x);
                max.y = Mathf.Max(max.y, vWS.y);
                max.z = Mathf.Max(max.z, vWS.z);
            }

            Bounds = new Bounds(transform.position, max - min);
        }

        /// <summary>
        /// Calculated the render steps to use when rendering this volume.
        /// </summary>
        /// <param name="c">Camera to use as reference when calculating with active LOD in mind.</param>
        public void CalculateRenderSteps(Camera c)
        {
            if (useActiveLod)
            {
                CurrentCameraDistance = Vector3.Distance(c.transform.position, transform.position);
                CurrentRenderSteps = Mathf.FloorToInt(lodCurve.Evaluate(CurrentCameraDistance));
            }
            else
            {
                CurrentRenderSteps = passiveRenderSteps;
            }
        }

        /// <summary>
        /// Sets the colour gradient texture from the gradients set in the editor.
        /// </summary>
        private void UpdateColourGradientTexture()
        {
            if (!IsGradientDirty) return;

            const int gradientResolution = 128;

            if (colourGradientTex == null || colourGradientTex.height != Channels || colourGradientTex.width != gradientResolution)
            {
                colourGradientTex = new Texture2D(gradientResolution, Channels, TextureFormat.RGBA32, true, true );//QualitySettings.activeColorSpace == ColorSpace.Linear);
            }

            int desiredArraySize = gradientResolution * Channels;
            if (gradientTextureColours == null || gradientTextureColours.Length != desiredArraySize)
            {
                gradientTextureColours = new Color[desiredArraySize];
            }

            int k = 0;
            for (int j = 0; j < Channels; j++)
            {
                for (float i = 0; i < gradientResolution; i++)
                {
                    gradientTextureColours[k++] = ColourGradient[j].Evaluate(1.0f - i / gradientResolution);
                }
            }

            colourGradientTex.SetPixels(gradientTextureColours);
            colourGradientTex.Apply();

            IsGradientDirty = false;
        }

        /// <summary>
        /// Sets the volume dirty so gradients can be updated.
        /// </summary>
        public void SetDirty()
        {
            IsDirty = true;
            IsGradientDirty = true;
        }

        #endregion

        #region Rendering
        /// <summary>
        /// Sets whether or not to run the rendering programs.
        /// </summary>
        [SerializeField]
        protected bool render = true;
        
        /// <summary>
        /// Camera position in volume space.
        /// </summary>
        protected Vector3 EyePositionVolumeSpace { get { return WorldToTextureMatrix.MultiplyPoint(Camera.current.transform.position ); } }

        /// <summary>
        /// Camera forward direction in volume space.
        /// </summary>
        protected Vector3 EyeForwardVolumeSpace { get { return WorldToTextureMatrix.MultiplyVector(Camera.current.transform.forward ); } }

        /// <summary>
        /// Matrix for transforming from volume space to view space.
        /// </summary>
        protected Matrix4x4 VolumeToViewMatrix { get { return Camera.current.worldToCameraMatrix * WorldToTextureMatrix.inverse; } }

        #region IBaseRenderer
        /// <summary>
        /// The render shader is run.
        /// 
        /// This method terminates early if there is no material present or if the volume isn't visible.
        /// </summary>
        void RenderMe()
        {
            if (!render || !ShouldRender) return;

            PerformRaymarch();
        }

        #endregion

        #region Rendering
        /// <summary>
        /// Raymarching is performed on the GPU here.
        /// Set up the shaders params and buffers.
        /// </summary>
        protected virtual void PerformRaymarch()
        {
            renderer.sharedMaterial = renderMaterials[channels - 1];
            
            if (RenderMask != null)
            {
                renderer.sharedMaterial.SetTexture("_RenderMaskTex", RenderMask);
            }

            Camera.current.depthTextureMode |= DepthTextureMode.Depth;

            renderer.sharedMaterial.SetFloat("_RenderQuality", Controller.ConditionedGlobalRenderQuality);
            renderer.sharedMaterial.SetFloat("_IsPerspectiveProjection", Camera.current.isOrthoGraphic ? 0 : 1);
            renderer.sharedMaterial.SetFloat("_IsDeferred", Camera.current.renderingPath != RenderingPath.DeferredLighting ? 0 : 1);

            renderer.sharedMaterial.SetTexture("_GradientTex", GradientTexture);
            renderer.sharedMaterial.SetTexture("_Volume", VolumeTexture);

            renderer.sharedMaterial.SetMatrix("_TextureToView", VolumeToViewMatrix);
            renderer.sharedMaterial.SetMatrix("_WorldToTexture", WorldToTextureMatrix);
            renderer.sharedMaterial.SetVector("_EyePositionTS", EyePositionVolumeSpace);
            renderer.sharedMaterial.SetVector("_EyeDirectionTS", EyeForwardVolumeSpace);

            CalculateRenderSteps(Camera.current);

            renderer.sharedMaterial.SetFloat("_EdgeFadePower", EdgeFadePower);
            renderer.sharedMaterial.SetFloat("_SaneMaxSteps", CurrentRenderSteps);
            renderer.sharedMaterial.SetFloat("_Intensity", GradientScale);
            renderer.sharedMaterial.SetFloat("_Opacity", OpacityModifier);

            SetEdgeBlendMode();
            SetAlphaBlendMode();
            SetSelfShadowing();
            SetNoisePeturbation();
        }

        /// <summary>
        /// Enables the keyword flags used in the shader to enable edge blending.
        /// </summary>
        private void SetEdgeBlendMode()
        {
            switch (EdgeBlendMode)
            {
                case EdgeBlendMode.None:
                    renderer.sharedMaterial.EnableKeyword("FLUIDITY_USE_NONE");
                    renderer.sharedMaterial.EnableKeyword("FLUIDITY_USE_EDGE_FADE");
                    renderer.sharedMaterial.EnableKeyword("FLUIDITY_USE_MASK");
                    break;

                case EdgeBlendMode.MaskTexture:
                    renderer.sharedMaterial.DisableKeyword("FLUIDITY_USE_NONE");
                    renderer.sharedMaterial.DisableKeyword("FLUIDITY_USE_EDGE_FADE");
                    renderer.sharedMaterial.EnableKeyword("FLUIDITY_USE_MASK");
                    break;

                case EdgeBlendMode.EdgeFade:
                    renderer.sharedMaterial.DisableKeyword("FLUIDITY_USE_NONE");
                    renderer.sharedMaterial.EnableKeyword("FLUIDITY_USE_EDGE_FADE");
                    renderer.sharedMaterial.DisableKeyword("FLUIDITY_USE_MASK");
                    break;
            }
        }

        /// <summary>
        /// Enables the keyword flags used in the shader to enable alpha blending.
        /// </summary>
        private void SetAlphaBlendMode()
        {
            switch (BlendMode)
            {
                case BlendMode.Alpha:
                    renderer.sharedMaterial.EnableKeyword("FLUIDITY_ALPHA_BLEND");
                    renderer.sharedMaterial.DisableKeyword("FLUIDITY_ADDITIVE_BLEND");
                    break;

                case BlendMode.Additive:
                    renderer.sharedMaterial.DisableKeyword("FLUIDITY_ALPHA_BLEND");
                    renderer.sharedMaterial.EnableKeyword("FLUIDITY_ADDITIVE_BLEND");
                    break;

            }
        }

        /// <summary>
        /// Enables the keyword flags used in the shader to enable self shadowing.
        /// </summary>
        private void SetSelfShadowing()
        {
            if (useSelfShadowing && sun != null)
            {
                renderer.sharedMaterial.EnableKeyword(  "FLUIDITY_ENABLE_SELF_SHADOWING");
                renderer.sharedMaterial.DisableKeyword("FLUIDITY_DISABLE_SELF_SHADOWING");

                if (opacityMap == null)
                {
                    opacityMap = new RenderTexture(VolumeDimensions.x / 4, VolumeDimensions.y / 4, 0, RenderTextureFormat.RHalf)
                    {
                        isVolume = true,
                        enableRandomWrite = true,
                        volumeDepth = VolumeDimensions.z / 4,
                        filterMode = FilterMode.Trilinear
                    };
                }

                Vector4 marchDirectionTS = WorldToTextureMatrix.MultiplyVector(-sun.forward);
                marchDirectionTS.w = lightDensity;

                Controller.LightingShader.SetFloat("_Intensity", GradientScale);
                Controller.LightingShader.SetVector("_MarchDirectionTS_density", marchDirectionTS );
                Controller.LightingShader.SetVector("_RecipDimensions", new Vector3(1.0f / opacityMap.width, 1.0f / opacityMap.height, 1.0f / opacityMap.volumeDepth));
                Controller.LightingShader.SetVector("_HalfVoxel",       new Vector3(0.5f / opacityMap.width, 0.5f / opacityMap.height, 0.5f / opacityMap.volumeDepth));

                Controller.LightingShader.SetTexture(0, "_GradientTex", GradientTexture);
                Controller.LightingShader.SetTexture(0, "_Volume", VolumeTexture);
                
                RenderTexture.active = opacityMap;
                Controller.LightingShader.SetTexture(0, "_OpacityMapRW", opacityMap);
                
                Controller.LightingShader.Dispatch( 0, (opacityMap.width + 7) / 8, (opacityMap.height + 7) / 8, (opacityMap.volumeDepth + 7) / 8);

                renderer.sharedMaterial.SetTexture("_OpacityMap", opacityMap);
            }
            else
            {
                renderer.sharedMaterial.DisableKeyword("FLUIDITY_ENABLE_SELF_SHADOWING");
                renderer.sharedMaterial.EnableKeyword("FLUIDITY_DISABLE_SELF_SHADOWING");
            }
        }

        /// <summary>
        /// Enables the keyword flags used in the shader to enable noise distortion.
        /// </summary>
        private void SetNoisePeturbation()
        {
            if (useNoisePeturbation && noiseVolume != null)
            {
                renderer.sharedMaterial.SetTexture("_NoiseVolume", noiseVolume);
                renderer.sharedMaterial.SetFloat("_NoiseFrequency",  noiseFrequency);
                renderer.sharedMaterial.SetFloat("_NoiseAmplitude",  noiseAmplitude);
                renderer.sharedMaterial.SetVector("_NoiseAnimation", noiseAnimation * Time.time);

                renderer.sharedMaterial.EnableKeyword(  "FLUIDITY_ENABLE_NOISE_PETURBATION");
                renderer.sharedMaterial.DisableKeyword("FLUIDITY_DISABLE_NOISE_PETURBATION");
            }
            else
            {
                renderer.sharedMaterial.DisableKeyword("FLUIDITY_ENABLE_NOISE_PETURBATION");
                renderer.sharedMaterial.EnableKeyword("FLUIDITY_DISABLE_NOISE_PETURBATION");
            }
        }

        #endregion
        #endregion
    }

}