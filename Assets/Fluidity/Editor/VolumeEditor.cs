using Fluidity.Renderer;
using Fluidity.Volume;
using UnityEngine;
using UnityEditor;

namespace Fluidity.Editor
{
    [CustomEditor(typeof(Volume.FluidVolume))]
    public class VolumeEditor : UnityEditor.Editor
    {
        private Volume.FluidVolume fluidVolume;
        private SerializedProperty blendMode;
        private SerializedProperty useSelfShadowing;
        private SerializedProperty sun;
        private SerializedProperty lightDensity;
        private SerializedProperty useNoisePeturbation;
        private SerializedProperty noiseVolume;
        private SerializedProperty noiseFrequency;
        private SerializedProperty noiseAmplitude;
        private SerializedProperty noiseAnimation;
        private SerializedProperty colourGradient;
        private SerializedProperty channels;
        private SerializedProperty gradientScale;
        private SerializedProperty opacityModifier;
        private SerializedProperty myLight;
        private SerializedProperty passiveRenderSteps;
        private SerializedProperty edgeBlendMode;
        private SerializedProperty renderMask;
        private SerializedProperty edgeFadePower;
        private SerializedProperty useActiveLod;
        private SerializedProperty lodCurve;

        private SerializedProperty useHeatHaze;
        private SerializedProperty heatHazeTaps;
        private SerializedProperty heatHazeStrength;
        private SerializedProperty heatHazeSpread;

        private void OnEnable()
        {
            fluidVolume = target as Volume.FluidVolume;

            blendMode = serializedObject.FindProperty("blendMode");
            useSelfShadowing = serializedObject.FindProperty("useSelfShadowing");
            lightDensity = serializedObject.FindProperty("lightDensity");
            sun = serializedObject.FindProperty("sun");
            useNoisePeturbation = serializedObject.FindProperty("useNoisePeturbation");
            noiseVolume = serializedObject.FindProperty("noiseVolume");
            noiseFrequency = serializedObject.FindProperty("noiseFrequency");
            noiseAmplitude = serializedObject.FindProperty("noiseAmplitude");
            noiseAnimation = serializedObject.FindProperty("noiseAnimation");
            colourGradient = serializedObject.FindProperty("colourGradient");
            channels = serializedObject.FindProperty("channels");
            gradientScale = serializedObject.FindProperty("gradientScale");
            opacityModifier = serializedObject.FindProperty("opacityModifier");
            myLight = serializedObject.FindProperty("myLight");
            passiveRenderSteps = serializedObject.FindProperty("passiveRenderSteps");
            edgeBlendMode = serializedObject.FindProperty("edgeBlendMode");
            renderMask = serializedObject.FindProperty("renderMask");
            edgeFadePower = serializedObject.FindProperty("edgeFadePower");
            useActiveLod = serializedObject.FindProperty("useActiveLod");
            lodCurve = serializedObject.FindProperty("lodCurve");
            useHeatHaze = serializedObject.FindProperty("useHeatHaze");
            heatHazeTaps = serializedObject.FindProperty("heatHazeTaps");
            heatHazeStrength = serializedObject.FindProperty("heatHazeStrength");
            heatHazeSpread = serializedObject.FindProperty("heatHazeSpread");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();

            using (new Horizontal("toolbarbutton"))
            {
                EditorGUILayout.LabelField("Simulation", EditorStyles.boldLabel);
            }

            fluidVolume.SimulationIndex = EditorGUILayout.Popup("Select : ", fluidVolume.SimulationIndex, Controller.GetSimulationNames());
            EditorGUILayout.Space();

            using (new Horizontal("toolbarbutton"))
            {
                EditorGUILayout.LabelField("Rendering Controls", EditorStyles.boldLabel);
            }
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(blendMode, new GUIContent("Blend Mode : "));
            EditorGUILayout.Space();            
            
            EditorGUILayout.PropertyField(useSelfShadowing, new GUIContent("Use Self Shadowing : "));
            if(useSelfShadowing.boolValue)
            {
                EditorGUILayout.Slider(lightDensity, 0.001f, 8, new GUIContent("Gas Density : "));
                EditorGUILayout.PropertyField(sun, new GUIContent("Sun : "));
            }
            EditorGUILayout.Space();        
    
            EditorGUILayout.PropertyField(useNoisePeturbation, new GUIContent("Use Noise Peturbation : "));
            if (useNoisePeturbation.boolValue)
            {
                EditorGUILayout.PropertyField(noiseVolume, new GUIContent("Noise Volume : "));
                EditorGUILayout.PropertyField(noiseFrequency, new GUIContent("Noise Frequency : "));
                EditorGUILayout.PropertyField(noiseAmplitude, new GUIContent("Noise Amplitude : "));
                EditorGUILayout.PropertyField(noiseAnimation, new GUIContent("Noise Animation Speeds : "));
            }
            EditorGUILayout.Space();

            EditorGUILayout.IntSlider(channels, 1, 4, new GUIContent("Colour Channels :"));
            for (int i = 0; i < channels.intValue; i++)
            {
                EditorGUILayout.PropertyField(colourGradient.GetArrayElementAtIndex(i), new GUIContent("Channel " + (i + 1) + " : "), true);
            }
            EditorGUILayout.Space();

            EditorGUILayout.Slider(gradientScale, 0, 8, new GUIContent("Gradient Scale : "));
            EditorGUILayout.Slider(opacityModifier, 0, 1, new GUIContent("Opacity Modifier : "));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(edgeBlendMode, new GUIContent("Edge Blend Mode : "));
            
            switch ((EdgeBlendMode)edgeBlendMode.enumValueIndex)
            {
                case EdgeBlendMode.MaskTexture:
                    EditorGUILayout.PropertyField(renderMask, new GUIContent("  Render Mask : "));
                    break;

                case EdgeBlendMode.EdgeFade:
                    EditorGUILayout.Slider(edgeFadePower, 0.001f, 10.0f, new GUIContent("  Edge Fade Power : "));
                    break;
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(myLight, new GUIContent("Link Light Source : "));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(useHeatHaze, new GUIContent("Heat Haze : "));

            if (useHeatHaze.boolValue)
            {
                EditorGUILayout.IntSlider(heatHazeTaps, 1, 8, new GUIContent("  Diffusion Taps : "));
                EditorGUILayout.Slider(heatHazeStrength, 0, 10, new GUIContent("  Strength : "));
                EditorGUILayout.PropertyField(heatHazeSpread, new GUIContent("  Spread Amount : "));
            }
            EditorGUILayout.Space();


            using (new Horizontal("toolbarbutton"))
            {
                EditorGUILayout.LabelField("Quality", EditorStyles.boldLabel);
            }
            EditorGUILayout.PropertyField(useActiveLod, new GUIContent("Use Active LOD : "));
            EditorGUILayout.Space();
            if (useActiveLod.boolValue)
            {
                using (new Vertical("box"))
                {
                    EditorGUILayout.LabelField("Current Render Steps : " + fluidVolume.CurrentRenderSteps);
                    EditorGUILayout.LabelField("Current Distance : " + fluidVolume.CurrentCameraDistance);
                }

                EditorGUILayout.PropertyField(lodCurve, new GUIContent("LOD Curve :"));
            }
            else
            {
                EditorGUILayout.PropertyField(passiveRenderSteps, new GUIContent("Render Steps : "));
            }
            EditorGUILayout.Space();

            if(serializedObject.ApplyModifiedProperties())
            {
                (target as BaseVolume).SetDirty();
            }
        }
    }
}