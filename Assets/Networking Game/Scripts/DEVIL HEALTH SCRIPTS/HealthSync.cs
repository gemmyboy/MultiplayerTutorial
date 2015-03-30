using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthSync : Photon.MonoBehaviour {

	public UIManager uiManager;
	private UIManager testForManager;
	private bool dead;
	private bool uiManagerStillNull;
	public float TankShellDamage;
	private PhotonPlayer hurtPlayer;
	private Text myText;
	private RectTransform myRect;
	public GameObject myTextObj;
	public GameObject myRectObj;
	public static int healthAmount;
	private GameObject theBullet;
	void Start()
	{
		dead = false;
		uiManagerStillNull = true;
		uiManager = null;
		testForManager = null;
		hurtPlayer = null;
		myText = myTextObj.GetComponent<Text> ();
		myRect = myRectObj.GetComponent<RectTransform> ();

		hurtPlayer = gameObject.GetPhotonView().owner;
		healthAmount = (int)gameObject.GetPhotonView ().owner.customProperties ["Health"];
		photonView.RPC("AdjustPercent",PhotonTargets.OthersBuffered,gameObject.GetPhotonView().ownerId,healthAmount);
	}

    void Update()
    {
		if(uiManagerStillNull && photonView.isOwnerActive && (photonView.ownerId == gameObject.GetPhotonView().ownerId))
		{
			testForManager = FindObjectOfType<UIManager>();
			if(testForManager != null)
			{
				uiManagerStillNull = false;
				uiManager = testForManager;
				uiManager.ChangeHealth(healthAmount);
				photonView.RPC("AdjustPercent",PhotonTargets.OthersBuffered,gameObject.GetPhotonView().ownerId,healthAmount);
			}
		}
    }

	void OnCollisionEnter(Collision col)
	{
		if (photonView.isOwnerActive && (photonView.ownerId == gameObject.GetPhotonView().ownerId)) 
		{
			healthAmount = (int)gameObject.GetPhotonView().owner.customProperties["Health"];

			if((healthAmount >= 0) && (!dead) && (col.gameObject.tag == "TankShell"))
			{
				theBullet = new GameObject();
				theBullet = col.gameObject;
				if(theBullet.GetPhotonView().owner.customProperties["Team"].ToString() != gameObject.GetPhotonView().owner.customProperties["Team"].ToString())
				{
					if(((healthAmount-(int)TankShellDamage)) <= 0 && !dead)
					{
						dead = true;
						//if(!photonView.isMine){}
						photonView.RPC("ReduceMyHealth",PhotonTargets.Others,gameObject.GetPhotonView().ownerId,1,theBullet.GetPhotonView().ownerId,dead);
                        photonView.RPC("tankGoBoom", PhotonTargets.All, gameObject.GetPhotonView().viewID);
					}else if((healthAmount-(int)TankShellDamage) > 0)
					{
						photonView.RPC("ReduceMyHealth",PhotonTargets.Others,gameObject.GetPhotonView().ownerId,2,theBullet.GetPhotonView().ownerId,dead);
					}
				}
			}
		}
	}


	[RPC]
	void ReduceMyHealth(int myViewID,int theCase,int theKiller,bool isDead)
	{
		if(theCase == 1 && (photonView.ownerId == myViewID) && isDead)
		{
			hurtPlayer = gameObject.GetPhotonView().owner;

			ExitGames.Client.Photon.Hashtable hash3 = new ExitGames.Client.Photon.Hashtable();
			hash3.Add("Health",0);
			hurtPlayer.SetCustomProperties(hash3);
			healthAmount = (int)hurtPlayer.customProperties["Health"];

			transform.rigidbody.AddExplosionForce(150000.0f,transform.position,10.0f,0.0f,ForceMode.Impulse);
			photonView.RPC ("AdjustHealthBar",PhotonTargets.OthersBuffered,gameObject.GetPhotonView().ownerId,1);
			photonView.RPC("AdjustPercent",PhotonTargets.OthersBuffered,gameObject.GetPhotonView().ownerId,healthAmount);

		}else if(theCase == 2 && (photonView.ownerId == myViewID) && !isDead){

			hurtPlayer = gameObject.GetPhotonView().owner;
			ExitGames.Client.Photon.Hashtable hash2 = new ExitGames.Client.Photon.Hashtable();

			healthAmount = (int)hurtPlayer.customProperties["Health"] - (int)TankShellDamage;
			hash2.Add("Health",healthAmount);
			hurtPlayer.SetCustomProperties(hash2);

			transform.rigidbody.AddExplosionForce(15000.0f,transform.position,10.0f,0.0f,ForceMode.Impulse);
			
			photonView.RPC("AdjustPercent",PhotonTargets.OthersBuffered,gameObject.GetPhotonView().ownerId,healthAmount);
			photonView.RPC ("AdjustHealthBar",PhotonTargets.OthersBuffered,gameObject.GetPhotonView().ownerId,2);

		}else if(true)//if photonView == theKiller && theCase == 1
		{

		}
	}

	[RPC]
	void AdjustPercent(int myViewID, int health)
	{
		if(photonView.ownerId == myViewID)
			myText.text = health.ToString ();
	}

	[RPC]
	void AdjustHealthBar(int myViewID, int bounds)
	{
		if(bounds == 1 && (photonView.ownerId == myViewID))
		{
			if(myRect.tag == "GreenHealthBar")
			{
				Vector2 tempVectTwo = new Vector2(7.01f,0.0f);
				myRect.offsetMin = tempVectTwo;
			}
		}else if(bounds == 2 && (photonView.ownerId == myViewID))
		{
			if(myRect.tag == "GreenHealthBar")
			{
				Vector2 tempVectTwoTwo = new Vector2(((TankShellDamage/100.0f)*7.1f),0.0f);
				myRect.offsetMin += tempVectTwoTwo;
			}
		}
	}

    GameObject tank;
    Transform[] children;
    GameObject trash;

    [RPC]
    void tankGoBoom(int viewID)
    {
        Debug.Log(viewID);
        PhotonView[] views = FindObjectsOfType<PhotonView>();
        foreach(PhotonView view in views){
            if(view.viewID == viewID){
                tank = view.gameObject;
            }
        }

        trash = new GameObject("TankTrash");

        Destroy(tank.GetComponent<TankController>());
        Destroy(tank.GetComponentInChildren<TankGunController>());
        Destroy(tank.GetComponent<TankInterpolationMovement>());
        Destroy(tank.GetComponent<RotateEnemyHealth>());
        Destroy(tank.GetComponentInChildren<TankGunColliders>());

        tank.transform.DetachChildren();

        fixForExplosion(tank.transform.Find("MainGun").gameObject);

        detachMultiple(tank.transform.Find("WheelTransforms_L").gameObject);
        detachMultiple(tank.transform.Find("WheelTransforms_R").gameObject);
        detachMultiple(tank.transform.Find("UselessGearsTransforms_L").gameObject);
        detachMultiple(tank.transform.Find("UselessGearsTransforms_R").gameObject);

        DestroyHinge(tank.transform.Find("Skirts_L").gameObject);
        DestroyHinge(tank.transform.Find("Skirts_R").gameObject);

        detachMultiple(tank.transform.Find("Skirts_L").gameObject);
        detachMultiple(tank.transform.Find("Skirts_R").gameObject);


        Destroy(tank.transform.Find("Tracks").gameObject);
        Destroy(tank.transform.Find("HeadLights").gameObject);
        Destroy(tank.transform.Find("HeavyExhaust").gameObject);
        Destroy(tank.transform.Find("NormalExhaust").gameObject);

        Collider[] colliders = Physics.OverlapSphere(gameObject.transform.position, 50);
        foreach (Collider hit in colliders)
        {
            if (hit.name != "Terrain")
            {
                if (hit.GetComponent<Rigidbody>() == null)
                {
                    hit.gameObject.AddComponent<Rigidbody>();
                }
            }
            if (hit && hit.rigidbody)
            {
                hit.rigidbody.isKinematic = false;
                hit.rigidbody.AddExplosionForce(1000, gameObject.transform.position, 500, 3);
            }
        }
    }
    void fixForExplosion(GameObject obj)
    {
        obj.AddComponent<BoxCollider>();
        obj.GetComponent<Rigidbody>().useGravity = true;
        if (obj.GetComponent<HingeJoint>() != null)
        {
            Destroy(obj.GetComponent<HingeJoint>());
        }
    }

    void DestroyHinge(GameObject obj)
    {
        foreach (Transform child in obj.transform)
        {
            Destroy(child.GetComponent<HingeJoint>());
        }
    }

    void detachMultiple(GameObject obj)
    {
        obj.tag = "Trash";
        if (checkObject(obj))
        {
            return;
        }
        foreach (Transform child in obj.transform)
        {
            if (child.GetComponent<BoxCollider>() == null)
            {
                child.gameObject.AddComponent<BoxCollider>();
            }
            child.tag = "Trash";
            child.DetachChildren();
        }
        obj.transform.DetachChildren();
    }
    bool checkObject(GameObject obj)
    {
        string[] list = new string[] { "Tracks", "MainGun", "Barrel" };
        foreach (string sting in list)
        {
            Debug.Log(obj.name);
            if (sting == obj.name)
            {
                return true;
            }
        }
        return false;
    }
}
