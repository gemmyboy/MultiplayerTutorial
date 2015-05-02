using UnityEngine;
using System.Collections;

public class fireworksSound : MonoBehaviour {
    public ParticleSystem system;
    public AudioSource source;
    public AudioClip firecrackerSound;

	// Use this for initialization
	void Start () {
        system = GetComponent<ParticleSystem>();
        source = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
}
