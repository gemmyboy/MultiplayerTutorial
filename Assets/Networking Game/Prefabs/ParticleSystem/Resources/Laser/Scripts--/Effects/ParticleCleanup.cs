using UnityEngine;
using System.Collections;

/// <summary>
/// destroy particle systems once they stop emitting
/// </summary>
public class ParticleCleanup : MonoBehaviour 
{
    ParticleSystem system;
    void Start()
    {
        system = particleSystem;
    }
	void Update () 
    {
        if (particleSystem.emissionRate == 0)
        {
            Destroy(gameObject, particleSystem.duration);
        }
	}
}
