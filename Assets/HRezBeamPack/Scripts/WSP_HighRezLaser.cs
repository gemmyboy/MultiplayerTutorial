using UnityEngine;
using System.Collections;

public class WSP_HighRezLaser : MonoBehaviour {

	// If set to true, Laser will fire automatically when it is recharged
	public bool AutoFire = false;

	// Display Name of the Beam Weapon, used for GUI
	public string GUIDisplayString = "Unnamed Beam Weapon";

	// Laser Targeting Variables

	// Current Target Transform of the Beam
	public Transform CurrentTarget;
	// Target That is being hit by the Line-Of-Sight Raycast
	public Transform TargetHit;
	// Function for setting target
	public void AssignNewTarget(Transform target) {
		if (CurrentTarget == null) {
			CurrentTarget = target;

			// Can Comment out this
			FireLaser();
		}
	}

	// Set to true will use raycast to find hitpoint, useful to keep beam from passing through objects, if set to false will just make beam go from start point to target position
	public bool UseLineOfSight = true;

	// Audio Source, will play firing sound if Audio Source is attached to this game object
	private AudioSource LaserFireSoundFX;

	// Firing Sound FX Loop Controls and Variables
	public AudioSource LaserFireLoopSFX;
	private float FireLoopFadeTime = 1.0f;
	private void UpdateFiringSFX() {
		if (LaserFireLoopSFX != null) {
			if (LaserFiring) {
				if (LaserFireLoopSFX.volume < 1.0f)
					LaserFireLoopSFX.volume += FireLoopFadeTime * Time.deltaTime;
			}
			else {
				if (LaserFireLoopSFX.volume > 0f)
					LaserFireLoopSFX.volume -= FireLoopFadeTime * Time.deltaTime;
				else 
					LaserFireLoopSFX.volume = 0;
			}
		}
	}

	// Used Internally to control only playing laser firing sound once
	private bool LaserSoundPlayed = false;
		
	public Transform LaserFireEmitPointTrans;
	public ParticleSystem LaserFireCenterEmitter;
	public float FiringParticleSize = 1;
	
	public Transform TargetHitTransform;
	private Light HitLight;
	private float storedHitLightIntensity = 0;
	public ParticleSystem TargetHitEmitter;
	public Transform TargetHit2Transform;
	public ParticleSystem TargetHit2Emitter;
	public float HitParticleSize = 1;

	// Target World Particle Emitter
	public Transform TargetWParticleSpawnTransform;
	public ParticleSystem TargetWParticleSpawnEmitter;
	public float TargetWParticleRate = 10;

	public LineRenderer HRezLaserLineRender;
	public float HRezLaserBeamWidth = 1.0f;
	public bool OffsetMaterialTexture = true;
	public float ScrollSpeed = 2.5F;
	public Material LaserYBeamMaterial;

	// Firing From and Firing To Points
	private Vector3 laserStartPoint = Vector3.zero;
	private Vector3 laserEndPoint = Vector3.zero;

	// Laser Firing Variables
	public bool LaserBeamActive = false;
	public bool UseExtendedLength = false;
	public bool LaserCanFire = false;
	public bool LaserFiring = false;
	public float FireSpeed = 10;
	private float laserLifeTimer = 0;
	public float LaserFireTime = 1.0f;
	private float laserRechargeTimer = 0;
	public float LaserRechargeTime = 2.5f;
	public float HRezLaserTileAmount = 1.0f;
	public float LaserGrowSpeed = 1.0f;

	// Physics Interaction Variables
	public float LaserPushForce = 0.5f;
		
	private WSP_HitSpawner hitSpawnerScript;
	public string HitSpawnGOName = "UnnamedHitGOName";

	private float hrezLaserStartWidth = 0;

	// Laser Damage Variables
	public float LaserDamage = 10;
	public float LaserDamagePerSecond = 2;
	private float damageOverTimeTimer = 0;
	private float damageOverTimeFreq = 1.0f; // 1 Second DPS (Damage Per Second Timer)
	private bool canDamageTarget = false;

	// Use this for initialization
	void Awake () {		
		LaserFireSoundFX = gameObject.GetComponent<AudioSource>();				
		if (LaserFireCenterEmitter != null)
			LaserFireCenterEmitter.emissionRate = 0;
		HitLight = TargetHitTransform.gameObject.GetComponent<Light>();
		storedHitLightIntensity = HitLight.intensity;
		HitLight.intensity = 0;
		if (TargetHitEmitter != null)
			TargetHitEmitter.emissionRate = 0;
		if (TargetHit2Emitter != null)
			TargetHit2Emitter.emissionRate = 0;
		if (TargetWParticleSpawnEmitter != null)
			TargetWParticleSpawnEmitter.emissionRate = 0;
		if (HRezLaserLineRender != null) {
			HRezLaserLineRender.enabled = false;
		}
		hitSpawnerScript = gameObject.GetComponent<WSP_HitSpawner>();
	}

	void Start () {

	}

	// Update is called once per frame
	void Update () {
		// If Test Laser Beam Firing
		if (AutoFire) {
			FireLaser();
		}
		if (UseExtendedLength) {
			LaserFireTime = 5.0f;
		}

		// Check to See if Laser Beam is Active Still
		if (HRezLaserLineRender.enabled) {
			LaserBeamActive = true;
		}
		else {
			LaserBeamActive = false;
		}

		// Stop Beam if target Destroyed
		if (CurrentTarget == null) {
			if (LaserFiring) {
				StopLaserFire();
			}
		}

		if (LaserFiring) {
			if (UseLineOfSight) {
				// Handle Damage Over Time
				if (!canDamageTarget) {
					if (damageOverTimeTimer < damageOverTimeFreq) {
						damageOverTimeTimer += Time.deltaTime;
					}
					else {
						damageOverTimeTimer = 0;
						canDamageTarget = true;
					}
				}
			}
			
			if (laserLifeTimer < LaserFireTime) {
				laserLifeTimer += Time.deltaTime;
				if (!LaserSoundPlayed) {
					LaserFireSoundFX.Play();
					LaserSoundPlayed = true;
				}
			}
			else {
				if (CurrentTarget != null) {
					CurrentTarget.gameObject.SendMessage("Damage", LaserDamage, SendMessageOptions.DontRequireReceiver);
				}
				laserLifeTimer = 0;
				StopLaserFire();
			}

			// Raise Hit Light
			if (HitLight != null) {
				HitLight.intensity = Mathf.Lerp(HitLight.intensity, storedHitLightIntensity, Time.deltaTime);
			}
		}
		else {
			if (HitLight != null)
				if (HitLight.intensity > 0)
					HitLight.intensity = Mathf.Lerp(HitLight.intensity, 0, 2 * Time.deltaTime);

			if (!LaserCanFire) {
				if (laserRechargeTimer < LaserRechargeTime) {
					laserRechargeTimer += Time.deltaTime;
				}
				else {
					LaserCanFire = true;
				}
			}
		}
		
		UpdateLaserBeam();

	}

	// Function to Start Firing Laser Beam (Resets all Variables and Timers)
	public void FireLaser() {
		if (LaserCanFire) {
			// Set Colors
			laserLifeTimer = 0;
			if (!LaserSoundPlayed) {
				LaserFireSoundFX.Play();
				LaserSoundPlayed = true;
			}
			laserStartPoint = LaserFireEmitPointTrans.position;
			laserEndPoint = CurrentTarget.position;
			hrezLaserStartWidth = 0f;
			LaserFiring = true;
			laserRechargeTimer = 0;
			LaserCanFire = false;
		}
	}

	// Function to Stop Firing Laser Beam (Resets all Variables and Timers)
	public void StopLaserFire() {
		LaserFireCenterEmitter.emissionRate = 0;
		TargetHitEmitter.emissionRate = 0;
		TargetHit2Emitter.emissionRate = 0;
		LaserFiring = false;
		LaserSoundPlayed = false;
	}

	public void UpdateLaserBeamTiling() {
		// Set Laser Beam Line Material Tile Amount
		HRezLaserLineRender.material.mainTextureScale = new Vector2(HRezLaserTileAmount, 1);
	}
	
	private void UpdateLaserBeam() {
		// Update Laser Firing Loop SFX
		UpdateFiringSFX();

		// Update Firing and Hitting Particles
		if (LaserFireCenterEmitter.startSize != FiringParticleSize)
			LaserFireCenterEmitter.startSize = FiringParticleSize;
		if (TargetHitEmitter.startSize != HitParticleSize)
			TargetHitEmitter.startSize = HitParticleSize;

		if (CurrentTarget != null && LaserFiring) {
			// Always Have Laser Start at Center Emit Point
			laserStartPoint = LaserFireEmitPointTrans.position;

			LaserFireCenterEmitter.emissionRate = 2;
						
			// Set Laser Beam Line Start and End Width
			if (hrezLaserStartWidth < HRezLaserBeamWidth)
				hrezLaserStartWidth += LaserGrowSpeed * Time.deltaTime;
			HRezLaserLineRender.SetWidth(hrezLaserStartWidth, hrezLaserStartWidth);
			
			if (!HRezLaserLineRender.enabled)
				HRezLaserLineRender.enabled = true;
			
			Vector3 targetHitPosition = CurrentTarget.position;
			// If Line of Sight Enables Use LOS Hit Check
			if (UseLineOfSight) {
				targetHitPosition = CheckLOSOnTarget();
			}

			TargetHitTransform.LookAt(laserStartPoint);
			TargetHit2Transform.LookAt(laserStartPoint);
					

			float distanceToTarget = Vector3.Distance(laserEndPoint, targetHitPosition);
			if (distanceToTarget > 1.0f) {
				laserEndPoint = Vector3.Lerp(laserEndPoint, targetHitPosition, FireSpeed * Time.deltaTime);
				TargetHitEmitter.emissionRate = 0;
				TargetHit2Emitter.emissionRate = 0;
				if (TargetWParticleSpawnEmitter != null) {
					TargetWParticleSpawnEmitter.emissionRate = 0;
				}
			}
			else {
				laserEndPoint = targetHitPosition;

				// Spawn Hit GOs
				if (hitSpawnerScript != null) {
					hitSpawnerScript.RunHitSpawner(targetHitPosition, HitSpawnGOName);
				}

				TargetHitTransform.position = targetHitPosition;
				TargetHitEmitter.emissionRate = 3;
				TargetHit2Transform.position = targetHitPosition;
				TargetHit2Emitter.emissionRate = 20;
				if (TargetWParticleSpawnTransform != null) {
					TargetWParticleSpawnTransform.position = targetHitPosition;
					TargetWParticleSpawnEmitter.emissionRate = TargetWParticleRate;
				}
			}			
			if (OffsetMaterialTexture) {
				float offset = Time.time * ScrollSpeed;
				//				InnerLaserLineRender.material.mainTextureScale = new Vector2(-offset, innerLaserTileAmount);
				HRezLaserLineRender.material.SetTextureOffset("_MainTex", new Vector2(-offset, 0));
			}
		}
		else {
			LaserFireCenterEmitter.emissionRate = 0;
			TargetHitEmitter.emissionRate = 0;
			TargetHit2Emitter.emissionRate = 0;
			if (TargetWParticleSpawnEmitter != null) {
				TargetWParticleSpawnEmitter.emissionRate = 0;
			}
//			laserEndPoint = laserStartPoint;
		}

		if (!LaserFiring) {
			float distanceToTarget = Vector3.Distance(laserStartPoint, laserEndPoint);
			if (distanceToTarget > 1.0f) {
				laserStartPoint = Vector3.Lerp(laserStartPoint, laserEndPoint, FireSpeed * Time.deltaTime);
			}
			else {
				if (HRezLaserLineRender.enabled)
					HRezLaserLineRender.enabled = false;
			}
		}

		if (HRezLaserLineRender != null) {
			HRezLaserLineRender.SetPosition(0, laserStartPoint);
			HRezLaserLineRender.SetPosition(1, laserEndPoint);
		}
	}
	
	private RaycastHit myhit = new RaycastHit();	
	private Ray myray = new Ray();
	
	private Vector3 CheckLOSOnTarget() {
		Vector3 hitPoint = Vector3.zero;
		
		Vector3 rayDirection = CurrentTarget.position - LaserFireEmitPointTrans.position;
		myray = new Ray(LaserFireEmitPointTrans.position, rayDirection);
		if (Physics.Raycast(myray, out myhit, 1000.0f)) {
			//			print(myhit.collider.name);
			if (canDamageTarget) {
				myhit.collider.gameObject.transform.root.SendMessage("Damage", LaserDamagePerSecond, SendMessageOptions.DontRequireReceiver);
				canDamageTarget = false;
			}
			if (myhit.collider.rigidbody != null) {
				myhit.collider.rigidbody.AddForce(rayDirection * LaserPushForce);
			}
			if (TargetHit != myhit.collider.transform)
				TargetHit = myhit.collider.transform;
			hitPoint = myhit.point;
		}
		
		return hitPoint;
	}

}
