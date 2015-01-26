using UnityEngine;

namespace Fluidity
{
    public class JetEngineAnimator : MonoBehaviour
    {
        [Range(0, 200)] public float thrust = 0;

        public Transform rotor;
        
        private void DrawGUI( int id )
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Engine Thrust : ");
            thrust = GUILayout.HorizontalSlider(thrust, 0, 200);
            GUILayout.EndHorizontal();
        }

        private void OnGUI()
        {
            GUI.Window(1, new Rect(Screen.width - 250, 0, 250, 50), DrawGUI, "Controls");
        }

        private void Update()
        {
            Physics.Simulation sim = Controller.GetSimulation(0);

            if (sim == null) return;

            sim.jaccobiIterations = 5;

            if(Input.GetKey(KeyCode.W)){
                thrust += 10f;
            }
            if (Input.GetKey(KeyCode.S))
            {
                thrust -= 10f;
            }
            thrust = Mathf.Clamp(thrust, 0, 200);
            if (thrust < 100)
            {
                sim.reactionSpeed = 1.75f;
                sim.confinementScale = 25.07f;
                sim.emitterForce = Mathf.Lerp(0, 1.2f, Mathf.Clamp01(thrust * 0.01f));
                sim.viscosity = 1;
                sim.emitterRadius = 80.0f;
            }
            else
            {
                float factor = Mathf.Clamp01( thrust * 0.01f - 1 );

                sim.reactionSpeed       = Mathf.Lerp(1.75f,  11.65f,    factor);
                sim.confinementScale    = Mathf.Lerp(25.07f, 6.2f,      factor);
                sim.emitterForce        = Mathf.Lerp(1.75f,  1.25f,     factor);
                sim.viscosity           = Mathf.Lerp(1.0f,   6.3f,      factor);
                sim.emitterRadius       = Mathf.Lerp(80.0f, 120.0f,     factor);
            }

            rotor.Rotate(Vector3.up, thrust * 2 * Time.deltaTime);
        }
    }
}