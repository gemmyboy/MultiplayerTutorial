using UnityEngine;

public class Flamethrower : MonoBehaviour
{
    public Fluidity.Volume.FluidVolume fluidVolume;
    public Transform spawnPoint;

    private bool shouldFire;

    private void Update()
    {
        fluidVolume.Simulation.sourcePosition = fluidVolume.WorldToTextureMatrix.MultiplyPoint(spawnPoint.position) * 2 - Vector3.one;
        fluidVolume.Simulation.emitFluid = shouldFire;
        fluidVolume.Simulation.sourceDirection = transform.forward;
        fluidVolume.IsTranslatingFixedGrid = true;
	}

    private void OnStartFire()
    {
        shouldFire = true;

        audio.Play();
    }

    private void OnStopFire()
    {
        shouldFire = false;

        audio.Stop();
    }
}
