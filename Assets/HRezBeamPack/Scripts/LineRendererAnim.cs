using UnityEngine;
using System.Collections;

public class LineRendererAnim : MonoBehaviour {

	// Line Renderer Component to use for the animation, this is set at start and assumes it is attached to same game object
	private LineRenderer myLineRenderer;

	// The list, in decending order of animation frames for the animation of the line renderer texture
	public Texture2D[] AnimationTextures;

	// Used for timing animation
	private float animationTimer = 0;
	// Playback Speed, how fast to change to next frame of animation
	public float AnimationPlaybackSpeed = 0.25f;
	// Current Frame of Animation to keep track
	private int currentFrame = 0;
	// Number of Frames in the AnimationTextures list, this is set automatically at startup
	private int numberOfAnimationFrames = 0;
	
	// Use this for initialization
	void Start () {
		myLineRenderer = gameObject.GetComponent<LineRenderer>();
		numberOfAnimationFrames = AnimationTextures.Length;
	}
	
	// Update is called once per frame
	void Update () {
		if (myLineRenderer != null) {
			if (myLineRenderer.enabled) {
				
				// Update Animation
				if (animationTimer < AnimationPlaybackSpeed) {
					animationTimer += Time.deltaTime;
				}
				else {
					// Switch Frame
					SetNextAnimationFrame();
					animationTimer = 0;
				}
				
			}
		}
	}
	
	private void SetNextAnimationFrame() {
		if (currentFrame < numberOfAnimationFrames - 1) {
			currentFrame++;
		}
		else {
			currentFrame = 0;
		}
		myLineRenderer.sharedMaterial.SetTexture(0, AnimationTextures[currentFrame]);
	}
}
