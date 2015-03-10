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

	private float rotationVelocity;
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

    public int m_LastProjectileID;
    public float m_LastShootTime;
	void Start () {
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
	}

	void Update(){
        if (m_PhotonView.isMine)
        {
            Shooting();
            //JointConfiguration();
        }
	}

	void FixedUpdate () {
        if (m_PhotonView.isMine)
        {
            if (transform.localEulerAngles.y > 0 && transform.localEulerAngles.y < 180)
                rotationOfTheGun = transform.localEulerAngles.y;
            else
                rotationOfTheGun = transform.localEulerAngles.y - 360;

            Vector3 targetPosition = transform.InverseTransformPoint(new Vector3(target.transform.position.x, target.transform.position.y, target.transform.position.z));

            inputSteer = (targetPosition.x / targetPosition.magnitude);
            rotationVelocity = rigidbody.angularVelocity.y;

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

		if(Input.GetButtonDown("Fire1") && loadingTime > reloadTime && ammo > 0){
            m_LastProjectileID++;
			rigidbody.AddForce(-transform.forward * recoilForce, ForceMode.VelocityChange);

            m_PhotonView.RPC("shootShell",PhotonTargets.All,barrelOut.position,barrelOut.rotation,m_LastProjectileID);

            //PhotonNetwork.Instantiate("Ground Smoke", new Vector3(tank.transform.position.x, tank.transform.position.y - 3, tank.transform.position.z), tank.transform.rotation, 0);
            //PhotonNetwork.Instantiate("Fluffy Smoke", barrelOut.transform.position, barrelOut.transform.rotation, 0);
			//ShootingSoundEffect();

			ammo --;
            guiManager.ChangeAmmo(ammo);
			loadingTime = 0;

		}

	}

    [RPC]
    void shootShell(Vector3 position, Quaternion rotation, int projectileID, PhotonMessageInfo info)
    {
        Debug.Log("Hola");
        double timeStamp = PhotonNetwork.time;
        timeStamp = info.timestamp;
        m_LastShootTime = Time.realtimeSinceStartup;

        GameObject shot = (GameObject)Instantiate(Resources.Load<GameObject>("Bullet"), position, rotation) as GameObject;
        shot.name = "Bullet_" + shot.name;

        TankBullet bullet = shot.GetComponent<TankBullet>();
        bullet.SetProjectileId(projectileID);
        bullet.SetCreationTime(timeStamp);

        shot.GetComponent<Rigidbody>().AddForce(barrelOut.forward * bulletVelocity, ForceMode.VelocityChange);
        ShootingSoundEffect(position);
    }
	void ShootingSoundEffect(Vector3 position){

		fireSoundSource = new GameObject("FireSound");
		fireSoundSource.transform.position = position;
		//fireSoundSource.transform.rotation = 
		//fireSoundSource.transform.parent = transform;
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
