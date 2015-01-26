using Fluidity.Physics;
using UnityEngine;

namespace Fluidity
{
    /// <summary>
    /// A simple example to switch simulations rendered within a volume using API.
    /// </summary>
    [RequireComponent(typeof (Volume.FluidVolume))]
    public class ApiDemo : MonoBehaviour
    {
        [SerializeField] private bool useFirstSimulation;
        [SerializeField] private bool animateSimulationValues;

        private float initialRadius;

        private void Start()
        {
            initialRadius = GetComponent<Volume.FluidVolume>().Simulation.emitterRadius;
        }

        private void Update()
        {
            // Can set simulation using indices directly.
            GetComponent<Volume.FluidVolume>().SimulationIndex = useFirstSimulation ? 0 : 1;

            // Can also fetch the name of the simulation and apply the index.
            GetComponent<Volume.FluidVolume>().SimulationIndex = useFirstSimulation ? Simulation.NameToIndex("Smaller Fluid") : Simulation.NameToIndex("Main Fire");

            // Once a simulation has been set we can query/alter variables belonging to it.
            if (animateSimulationValues)
            {
                GetComponent<Volume.FluidVolume>().Simulation.emitterRadius = initialRadius * (Mathf.Sin(Time.time * 4) * 0.5f + 0.5f);
            }
        }
    }
}