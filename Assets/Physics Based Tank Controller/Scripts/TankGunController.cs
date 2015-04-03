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

	public Transform barrelOut;

	public AudioClip fireSoundClip;
	private GameObject fireSoundSource;
	public GameObject groundSmoke;
	public GameObject fireSmoke;

    public PhotonView m_PhotonView;
    UIManager guiManager;
    public GameStartTimeManager timeManager;

    public int m_LastProjectileID;
    public float m_LastShootTime;
	private float healthRefreshTimer;
	private float tempHealth;
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
            //JointConfiguration();
        }
		if(healthRefreshTimer <= Time.time)
		{
			healthRefreshTimer+=1.0f;

			//tempHealth = (int)HealthSync.healthAmount;
			if(m_PhotonView.isMine)
			{
				tempHealth = (int)m_PhotonView.owner.customProperties["Health"];
				guiManager.ChangeHealth((int)tempHealth);
			}
		}
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
    public GameObject lasers;
    public GameObject laserMuzzleFlash;
	void Shooting(){

		loadingTime += Time.deltaTime;

		if(Input.GetButtonDown("Fire1") && loadingTime > reloadTime && ammo > 0)
		{
			//rigidbody.AddForce(-transform.forward * recoilForce, ForceMode.VelocityChange);
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
			//StartCoroutine(bulletShoot());

            

		}

		if(Input.GetButtonDown("Fire2") && loadingTime > reloadTime && ammo > 0)
		{
			rigidbody.AddForce(-transform.forward * recoilForce, ForceMode.VelocityChange);

			StartCoroutine(laserShoot());
		}

	}
	/*
	IEnumerator bulletShoot()
	{
        Vector3 rotationDir = barrelOut.position - barrel.transform.position;
        //shot.transform.forward = rotationDir.normalized;
        //shot.transform.LookAt(shot.transform.position + shot.transform.forward);

        //shot.GetComponent<Rigidbody>().AddForce(barrelOut.forward * bulletVelocity, ForceMode.VelocityChange);
        ShootingSoundEffect();

		PhotonNetwork.Instantiate("Ground Smoke", new Vector3(tank.transform.position.x, tank.transform.position.y - 3, tank.transform.position.z), tank.transform.rotation, 0);
		//PhotonNetwork.Instantiate("Fluffy Smoke", barrelOut.transform.position, barrelOut.transform.rotation, 0);
		
		ammo--;
		guiManager.ChangeAmmo(ammo);
		loadingTime = 0;
	}
	*/
	IEnumerator laserShoot()
    {
        // play charge noise
        GameObject flash = (GameObject)Instantiate(laserMuzzleFlash, barrelOut.position, Quaternion.identity);

        flash.transform.parent = barrelOut.transform;

        yield return new WaitForSeconds(1.0f);


        GameObject laser = Instantiate(lasers, barrelOut.position, barrel.transform.rotation) as GameObject;
        laser.GetComponent<Rigidbody>().AddForce(barrelOut.forward * bulletVelocity * 5, ForceMode.VelocityChange);
        /*
        Vector3 rotationDir = barrelOut.position - barrel.transform.position;
        //shot.transform.forward = rotationDir.normalized;
        //shot.transform.LookAt(shot.transform.position + shot.transform.forward);

        //shot.GetComponent<Rigidbody>().AddForce(barrelOut.forward * bulletVelocity, ForceMode.VelocityChange);
        ShootingSoundEffect();
         * */
        PhotonNetwork.Instantiate("Ground Smoke", new Vector3(tank.transform.position.x, tank.transform.position.y - 3, tank.transform.position.z), tank.transform.rotation, 0);
        //PhotonNetwork.Instantiate("Fluffy Smoke", barrelOut.transform.position, barrelOut.transform.rotation, 0);

        ammo--;
        guiManager.ChangeAmmo(ammo);
        loadingTime = 0;

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
