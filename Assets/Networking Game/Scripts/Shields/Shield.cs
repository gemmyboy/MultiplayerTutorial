using UnityEngine;
using System.Collections;

public class Shield : Photon.MonoBehaviour {
    public float lifeTime = 15.0f;
    public float timer = 0.0f;
	// Use this for initialization
	void Start () {
        timer = 0.0f;
	}
	
	// Update is called once per frame
	void Update () {
        if (photonView.isMine)
        {
            timer += Time.deltaTime;
            if (gameObject.activeSelf && timer > lifeTime)
            {
                GetComponent<Animator>().SetBool("Dying",true);
                StartCoroutine(Explosion());
            }
        }
	}
    IEnumerator Explosion()
    {
        yield return new WaitForSeconds(1.0f);
        PhotonNetwork.Instantiate("Shield_Explosion", transform.position, transform.rotation, 0);
        if (photonView.isMine && gameObject != null)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
