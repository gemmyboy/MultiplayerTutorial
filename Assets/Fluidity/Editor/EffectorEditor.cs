using Fluidity.Tools;
using UnityEngine;
using UnityEditor;

namespace Fluidity.Editor
{
    [CustomEditor(typeof (Effector))]
    [CanEditMultipleObjects]
    public class EffectorEditor : UnityEditor.Editor
    {
        private SerializedProperty radius;
        private SerializedProperty modes;
        private SerializedProperty power;
        private SerializedProperty channel;
        private SerializedProperty heatResistance;
        private SerializedProperty coolDownRate;
        private SerializedProperty fatalTemperature;
        private SerializedProperty sendSignalOnFatal;
        private SerializedProperty axis;

        private const int modesThatUsePower = (int)Effector.EffectorModes.TempEmitter | (int)Effector.EffectorModes.Turbulence;
        private const int modesThatUseChannel = (int)Effector.EffectorModes.TempEmitter;
        private const int modesThatUseTemperature = (int)Effector.EffectorModes.Receiver;

        private void OnEnable()
        {
            radius = serializedObject.FindProperty("radius");
            modes = serializedObject.FindProperty("modes");
            power = serializedObject.FindProperty("power");
            channel = serializedObject.FindProperty("channel");
            heatResistance = serializedObject.FindProperty("heatResistance");
            coolDownRate = serializedObject.FindProperty("coolDownRate");
            fatalTemperature = serializedObject.FindProperty("fatalTemperature");
            sendSignalOnFatal = serializedObject.FindProperty("sendSignalOnFatal");
            axis = serializedObject.FindProperty("axis");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            using (new Horizontal("toolbarbutton"))
            {
                EditorGUILayout.LabelField("Mode", EditorStyles.boldLabel);
            }
            modes.intValue = EditorGUILayout.MaskField("Select Multiple : ", modes.intValue, System.Enum.GetNames(typeof(Effector.EffectorModes)));
            EditorGUILayout.Space();
            radius.floatValue = Mathf.Max(0, EditorGUILayout.FloatField("Radius : ", radius.floatValue));
            EditorGUILayout.Space();
                
            if((modes.intValue & modesThatUsePower) != 0)
            {
                using (new Horizontal("toolbarbutton"))
                {
                    EditorGUILayout.LabelField("Power Properties", EditorStyles.boldLabel);
                }
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(power, new GUIContent("Power : "));
                EditorGUILayout.PropertyField(axis, new GUIContent("Axis : "));
                EditorGUILayout.Space();
            }         
       
            if((modes.intValue & modesThatUseChannel) != 0)
            {
                using (new Horizontal("toolbarbutton"))
                {
                    EditorGUILayout.LabelField("Channel Properties", EditorStyles.boldLabel);
                }
                EditorGUILayout.IntSlider(channel, 1, 4, new GUIContent("Channel : "));
                EditorGUILayout.Space();
            }

            if ((modes.intValue & modesThatUseTemperature) != 0)
            {
                using (new Horizontal("toolbarbutton"))
                {
                    EditorGUILayout.LabelField("Temperature Properties", EditorStyles.boldLabel);
                }
                EditorGUILayout.PropertyField(coolDownRate, new GUIContent("Cool Down Rate : "));
                EditorGUILayout.Slider(heatResistance, 0, 1, new GUIContent("Heat Resistance : "));
                EditorGUILayout.Slider(fatalTemperature, 0, 100, new GUIContent("Fatal Temperature : "));
                EditorGUILayout.PropertyField(sendSignalOnFatal, new GUIContent("Send OnDeath Message? : "));
                EditorGUILayout.Space();
                EditorGUI.ProgressBar(GUILayoutUtility.GetRect(200, 30), (target as Effector).Temperature / 100, "Temperature");
                EditorGUILayout.Space();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}