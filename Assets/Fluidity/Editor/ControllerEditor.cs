//#define FLUIDITY_DEBUG_MODE

using System.Globalization;
using System.Linq;
using Fluidity;
using Fluidity.Auxilliary;
using Fluidity.Physics;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Controller))]
public class ControllerEditor : Editor
{
    private Controller controller;
    private bool[] openForEdit;

    private SerializedProperty globalRunSimulations;
    private SerializedProperty globalRenderQuality;
    private SerializedProperty globalDrawRenderMasks;
    private SerializedProperty fluidTimeScale;

#   if FLUIDITY_DEBUG_MODE
    private SerializedProperty globalRenderMode;
#   endif

    private void OnEnable()
    {
        controller = target as Controller;
        openForEdit = null;

        globalRunSimulations = serializedObject.FindProperty("globalRunSimulations");
        globalRenderQuality = serializedObject.FindProperty("globalRenderQuality");
        globalDrawRenderMasks = serializedObject.FindProperty("globalDrawRenderMasks");
        fluidTimeScale = serializedObject.FindProperty("fluidTimeScale");

#       if FLUIDITY_DEBUG_MODE
        globalRenderMode = serializedObject.FindProperty("globalRenderMode");
#       endif
    }

    public override void OnInspectorGUI()
    {
        if ( SystemInfo.graphicsShaderLevel < 50 )
        {
            DisplayApiNotSupportedError();
            return;
        }

        if ( FindObjectsOfType(typeof(Controller)).Length > 1 )
        {
            DisplayControllerNotFoundError();
            return;
        }

        serializedObject.Update();

        if (openForEdit == null)
        {
            openForEdit = new bool[Controller.SimulationCount];
        }

        EditorGUILayout.Space();

        using (new Horizontal("toolbarbutton"))
        {
            EditorGUILayout.LabelField("Global Settings", EditorStyles.boldLabel);
        }

#       if FLUIDITY_DEBUG_MODE
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(globalRenderMode, new GUIContent("Debug Render Mode : "));
        EditorGUILayout.Space();
#       endif

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(globalRunSimulations, new GUIContent("Run Simulations : "));
        EditorGUILayout.PropertyField(globalDrawRenderMasks, new GUIContent("Show Render Masks : "));
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(globalRenderQuality, new GUIContent("Quality : "));
        EditorGUILayout.Space();
        EditorGUILayout.Slider(fluidTimeScale, 0, 1, new GUIContent("Fluid Time Scale : "));
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        DisplaySimulations();

        serializedObject.ApplyModifiedProperties();
    }

    private void DisplaySimulations()
    {
        using (new Horizontal("toolbarbutton"))
        {
            EditorGUILayout.LabelField("Simulations", EditorStyles.boldLabel);
        }

        for (int i = 0; i < controller.simulations.Length; i++)
        {
            Simulation simulation = controller.simulations[i];
            using (new Horizontal("toolbarbutton"))
            {
                if (simulation == null)
                {
                    ArrayUtility.RemoveAt(ref controller.simulations, i);
                    break;
                }

                simulation.name = EditorGUILayout.TextField(simulation.name);
                ValidateSimulation( simulation );

                if (openForEdit[i])
                {
                    using (new GUIBackgroundColor(new Color(1.0f, 0.8f, 0.8f)))
                    {
                        if (GUILayout.Button("Close", "toolbarbutton"))
                        {
                            openForEdit[i] = false;
                        }
                    }
                }
                else
                {
                    using (new GUIBackgroundColor(new Color(0.8f, 1.0f, 0.8f)))
                    {
                        if (GUILayout.Button("Edit", "toolbarbutton"))
                        {
                            openForEdit[i] = true;
                        }
                    }
                }

                using (new GUIBackgroundColor(Color.red))
                {
                    if (GUILayout.Button("X", "toolbarbutton", GUILayout.Width(30)))
                    {
                        Undo.RecordObject(controller, "Delete Simulation...");
                        ArrayUtility.RemoveAt(ref controller.simulations, i);
                        ArrayUtility.RemoveAt(ref openForEdit, i);
                        break;
                    }
                }
            }

            if (openForEdit[i])
            {
                using (new Vertical("box"))
                {
                    EditorGUILayout.LabelField("Name : " + simulation.name, EditorStyles.boldLabel);
                    using (new Vertical("box"))
                    {
                        EditorGUILayout.LabelField("Grid Resolution");
                        using (new Horizontal())
                        {
                            simulation.maxDimensions.x = EditorGUILayout.IntField(simulation.maxDimensions.x);
                            simulation.maxDimensions.y = EditorGUILayout.IntField(simulation.maxDimensions.y);
                            simulation.maxDimensions.z = EditorGUILayout.IntField(simulation.maxDimensions.z);
                            simulation.maxDimensions = int3.FloorTo4(simulation.maxDimensions);
                        }

                        simulation.emitFluid = EditorGUILayout.Toggle("Emit Fluid : ", simulation.emitFluid);
                        EditorGUILayout.Space();
                        simulation.ignoreCulling = EditorGUILayout.Toggle("Ignore Culling : ", simulation.ignoreCulling);
                        EditorGUILayout.Space();
                        simulation.isHighQuality = EditorGUILayout.Toggle("High Quality Simulation : ", simulation.isHighQuality);
                        simulation.useMacCormack = EditorGUILayout.Toggle("MacCormack Advection : ", simulation.useMacCormack);
                        simulation.useVorticityConfinement = EditorGUILayout.Toggle("Vorticity Confinement : ", simulation.useVorticityConfinement);
                        EditorGUILayout.Space();
                        simulation.useBounds = EditorGUILayout.Toggle("Bound To Volume : ", simulation.useBounds);
                        EditorGUILayout.Space();

                        simulation.reactionSpeed = EditorGUILayout.FloatField("Reaction Speed : ", simulation.reactionSpeed);
                        simulation.viscosity = EditorGUILayout.FloatField("Viscosity : ", simulation.viscosity);
                        simulation.affectedByGravity = EditorGUILayout.FloatField("Affected By Gravity : ", simulation.affectedByGravity);
                        EditorGUILayout.Space();

                        if (simulation.useVorticityConfinement)
                        {
                            simulation.confinementScale = EditorGUILayout.FloatField("Vorticity : ", simulation.confinementScale);
                        }

                        if (simulation.isHighQuality)
                        {
                            simulation.jaccobiIterations = EditorGUILayout.IntSlider("Iterations : ", simulation.jaccobiIterations, 0, 50);
                        }
                        EditorGUILayout.Space();
                        using (new Vertical("box"))
                        {
                            EditorGUILayout.LabelField("Emitter Settings", EditorStyles.miniLabel);
                            simulation.emitterMode = (Simulation.EmitterMode)EditorGUILayout.EnumPopup("Emitter Mode : ", simulation.emitterMode);
                            simulation.emitterForce = EditorGUILayout.FloatField("Emission Force : ", simulation.emitterForce);
                            EditorGUILayout.Space();
                            switch (simulation.emitterMode)
                            {
                                case Simulation.EmitterMode.Sphere:
                                    simulation.emitterRadius = EditorGUILayout.FloatField("Radius : ", simulation.emitterRadius);
                                    EditorGUILayout.Space();
                                    simulation.sourcePosition = EditorGUILayout.Vector3Field("Position : ", simulation.sourcePosition);
                                    simulation.sourceDirection = EditorGUILayout.Vector3Field("Direction : ", simulation.sourceDirection);
                                    EditorGUILayout.Space();
                                    simulation.emitTexture = null;
                                    break;

                                case Simulation.EmitterMode.Texture:
                                    simulation.emitTexture = EditorGUILayout.ObjectField("Emitter Texture : ", simulation.emitTexture, typeof(Texture), false) as Texture;

                                    break;
                            }
                        }
                    }
                }
            }
        }

        using (new Horizontal("toolbarbutton"))
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+", "toolbarbutton", GUILayout.Width(30)))
            {
                Undo.RecordObject(controller, "Add Simulation...");
                var newSimulation = new Simulation { name = "Click here to change name... " };
                ValidateSimulation( newSimulation);
                ArrayUtility.Add(ref controller.simulations, newSimulation);
                ArrayUtility.Add(ref openForEdit, false);
            }
        }
    }

    public void ValidateSimulation(Simulation simulation)
    {
        for (int i = 0; i < controller.simulations.Length; i++)
        {
            Simulation s = controller.simulations[i];

            if (simulation == s) continue;

            if (s.name != simulation.name) continue;

            int j;
            if (int.TryParse(simulation.name.Last().ToString(CultureInfo.InvariantCulture), out j))
            {
                simulation.name = simulation.name.Remove(simulation.name.Length - 1);
            }
            else
            {
                j = 0;
            }

            while (s.name == (simulation.name + ++j))
            {
            }

            simulation.name = simulation.name + j;
            i = 0;
        }
    }

    private void DisplayControllerNotFoundError()
    {
        EditorGUILayout.LabelField("Error!  There can only be one instance of this component in the scene.", EditorStyles.largeLabel);
        EditorGUILayout.LabelField("Please delete one...", EditorStyles.largeLabel);
        EditorGUILayout.Space();
    }

    private void DisplayApiNotSupportedError()
    {
        EditorGUILayout.LabelField("Error!  You MUST be in DX11 mode and have a DX11 compatible graphics card to use Fluidity.", EditorStyles.largeLabel);
        EditorGUILayout.LabelField("Please enable DX11 mode in the editor...", EditorStyles.largeLabel);
        EditorGUILayout.Space();
    }
}
