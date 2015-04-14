using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Rigidbody))]
[RequireComponent (typeof (HingeJoint))]

public class TankGunController : MonoBehaviour {

	public GameObject tank;
	public GameObject barrel;
	private BoxCollider[] colliders;
	private HingeJoint joint;
	private JointLimits jointRotationLimit;
	 
	private float inputSteer;

	public int rotationTorque = 100;
	public float maximumAngularVelocity = 1f;
	public int maximumRotationLimit = 160;
	public float minimumElevationLimit = 10;
	public float maximumElevationLimit = 25;
	public bool useLimitsForRotation = true;

	//private float rotationVelocity;
	public float rotationOfTheGun;

	public Transform target;

	public Rigidbody bullet;
	public int bulletVelocity = 250;
	public int recoilForce = 500;
	public int ammo = 15;
	public float reloadTime = 3f;
	private float loadingTime = 3f;

    public float laserReloadTime = 15.0f;
    private float laserLoadingTime = 15.0f;
    public int laserRecoilForce = 500;

	public Transform barrelOut;

	public AudioClip fireSoundClip;
	private GameObject fireSoundSource;
	public GameObject groundSmoke;
	public GameObject fireSmoke;

    public PhotonView m_PhotonView;
    UIManager guiManager;
    public GameStartTimeManager timeManager;
	
	private float healthRefreshTimer;
	private float tempHealth;

	public Transform BoostSpawn;
	private float boostTime = 500.0f;
	private bool boostRegenerate = false;
	void Start () {
        timeManager = FindObjectOfType<GameStartTimeManager>();
        guiManager = FindObjectOfType<UIManager>();
        guiManager.ChangeAmmo(ammo);

        m_PhotonView = GetComponent<PhotonView>();
		rigidbody.maxAngularVelocity = maximumAngularVelocity;
		rigidbody.interpolation = RigidbodyInterpolation.None;
		rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

		colliders = GetComponentsInChildren<BoxCollider>();
		joint = GetComponent<HingeJoint>();

        if(m_PhotonView.isMine){
            foreach (BoxCollider col in colliders)
            {
                col.transform.gameObject.tag = "Player";
            }
        }
		healthRefreshTimer = Time.time;
	}

	void Update(){
        if (m_PhotonView.isMine && timeManager.IsItTimeYet)
        {
            Shooting();
			if (boostTime >= 500)
				boostRegenerate = false;
			if (boostRegenerate)
				boostTime = boostTime + 0.5f;
            //JointConfiguration();
        }
//		if(healthRefreshTimer <= Time.time)
//		{
//			healthRefreshTimer+=1.0f;
//
//			//tempHealth = (int)HealthSync.healthAmount;
//			if(m_PhotonView.isMine)
//			{
//				tempHealth = (int)m_PhotonView.owner.customProperties["Health"];
//				guiManager.ChangeHealth((int)tempHealth);
//			}
//		}
	}

	void FixedUpdate () {
        if (m_PhotonView.isMine && timeManager.IsItTimeYet)
        {
            if (transform.localEulerAngles.y > 0 && transform.localEulerAngles.y < 180)
                rotationOfTheGun = transform.localEulerAngles.y;
            else
                rotationOfTheGun = transform.localEulerAngles.y - 360;

            Vector3 targetPosition = transform.InverseTransformPoint(new Vector3(target.transform.position.x, target.transform.position.y, target.transform.position.z));

            inputSteer = (targetPosition.x / targetPosition.magnitude);
            //rotationVelocity = rigidbody.angularVelocity.y;

            if (inputSteer > 0)
            {
                rigidbody.AddRelativeTorque(0, (rotationTorque) * Mathf.Abs(inputSteer), 0, ForceMode.Acceleration);
            }
            else
            {
                rigidbody.AddRelativeTorque(0, (-rotationTorque) * Mathf.Abs(inputSteer), 0, ForceMode.Acceleration);
            }

            Quaternion targetRotation = Quaternion.LookRotation(target.transform.position - transform.position);
            barrel.transform.rotation = Quaternion.Slerp(barrel.transform.rotation, targetRotation, Time.deltaTime * 5);

            if (barrel.transform.localEulerAngles.x > 0 && barrel.transform.localEulerAngles.x < 180)
                barrel.transform.localEulerAngles = new Vector3(Mathf.Clamp(barrel.transform.localEulerAngles.x, -minimumElevationLimit, minimumElevationLimit), 0, 0);
            if (barrel.transform.localEulerAngles.x > 180 && barrel.transform.localEulerAngles.x < 360)
                barrel.transform.localEulerAngles = new Vector3(Mathf.Clamp(barrel.transform.localEulerAngles.x - 360, -maximumElevationLimit, maximumElevationLimit), 0, 0);
        }
	}
	void Shooting(){

		loadingTime += Time.deltaTime;

		if(Input.GetButtonDown("Fire1") && loadingTime > reloadTime && ammo > 0)
		{
			rigidbody.AddForce(-transform.forward * recoilForce, ForceMode.VelocityChange);
            GameObject shot = PhotonNetwork.Instantiate("TankBullet", barrelOut.position, barrel.transform.rotation,0) as GameObject;
		
			Vector3 rotationDir = barrelOut.position - barrel.transform.position;
			shot.transform.forward = rotationDir.normalized;
			shot.transform.LookAt(shot.transform.position + shot.transform.forward);
			
			shot.GetComponent<Rigidbody>().AddForce(barrelOut.forward * bulletVelocity, ForceMode.VelocityChange);
			ShootingSoundEffect();
			
			PhotonNetwork.Instantiate("Ground Smoke", new Vector3(tank.transform.position.x, tank.transform.position.y - 3, tank.transform.position.z), tank.transform.rotation, 0);
			//PhotonNetwork.Instantiate("Fluffy Smoke", barrelOut.transform.position, barrelOut.transform.rotation, 0);
			
			ammo--;
			guiManager.ChangeAmmo(ammo);
			loadingTime = 0;
            guiManager.bulletShot = true;
            guiManager.setToZero(guiManager.bulletTimerRect.parent.gameObject);
		}

        if(PhotonNetwork.room.customProperties["GameType"].ToString() == "OmegaTank" && PhotonNetwork.player.customProperties["TheOmega"].ToString() == "1"){
			Debug.Log("Omeaga Shooting");
			laserLoadingTime += Time.deltaTime;
            if (Input.GetButtonDown("Fire2") && laserLoadingTime > laserReloadTime)
            {
                StartCoroutine(laserShoot());
            }
        }

		if (Input.GetButton ("Jump")) { 
			if (boostTime > 0 && !boostRegenerate) {
				rigidbody.AddForce (BoostSpawn.transform.forward * -10, ForceMode.VelocityChange);
				rigidbody.AddForce (BoostSpawn.transform.up * -10, ForceMode.VelocityChange);
				GameObject BoostClone = PhotonNetwork.Instantiate ("Boost", BoostSpawn.position, BoostSpawn.rotation, 0) as GameObject;

				ShootingSoundEffect ();

				boostRegenerate = false;
				boostTime = boostTime -1;
			}
			else if (boostTime >= 500)
				boostRegenerate = false;
			else 
				boostRegenerate = true;
		}
	}

	IEnumerator laserShoot()
    {
        // play charge noise
        GameObject flash = (GameObject)PhotonNetwork.Instantiate("MuzzleFlashlaser", barrelOut.position, Quaternion.identity, 0);

        flash.transform.parent = barrelOut.transform;
        yield return new WaitForSeconds(1.0f);

        rigidbody.AddForce(-transform.forward * laserRecoilForce, ForceMode.VelocityChange);
        GameObject laser = PhotonNetwork.Instantiate("Laserssss", barrelOut.position + (barrelOut.forward * 10), barrel.transform.rotation, 0) as GameObject;
        laser.GetComponent<Rigidbody>().AddForce(barrelOut.forward * bulletVelocity * 5, ForceMode.VelocityChange);

        PhotonNetwork.Instantiate("Ground Smoke", new Vector3(tank.transform.position.x, tank.transform.position.y - 3, tank.transform.position.z), tank.transform.rotation, 0);

        laserLoadingTime = 0;

        yield return null;
    }


	void ShootingSoundEffect(){

		fireSoundSource = new GameObject("FireSound");
		fireSoundSource.transform.position = transform.position;
        fireSoundSource.transform.rotation = transform.rotation;
		fireSoundSource.transform.parent = transform;
		fireSoundSource.AddComponent<AudioSource>();
		fireSoundSource.audio.minDistance = 30;
		fireSoundSource.audio.clip = fireSoundClip;
		fireSoundSource.audio.pitch = UnityEngine.Random.Range (.9f, 1.2f);
		fireSoundSource.audio.Play ();
		Destroy(fireSoundSource, fireSoundSource.audio.clip.length);

	}
	
	void JointConfiguration(){

		if(useLimitsForRotation){

			jointRotationLimit.min = -maximumRotationLimit;
			jointRotationLimit.max = maximumRotationLimit;

			joint.limits = jointRotationLimit;

		}else{

			joint.useLimits = false;

		}

	}

}
