using UnityEngine;
using System.Collections;

public class PickUpFlag : MonoBehaviour {
    public bool tankCanPickUp = true;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Flag"))
        {
            if(other.tag == "BloodFlag"){
                if(PhotonNetwork.player.customProperties["Team"].ToString() != "Angel" && tankCanPickUp && other.GetComponent<Flag>().canBePickedUp){
                    other.gameObject.transform.parent = gameObject.transform;
                    //other.gameObject.transform.localPosition = new Vector3(0,0,0);
                    //other.gameObject.transform.localScale = new Vector3(1, 1, 1);
                    other.GetComponent<Flag>().canBePickedUp = false;
                    tankCanPickUp = false;
                }
            }else if(other.tag == "EagleFlag"){
                if (PhotonNetwork.player.customProperties["Team"].ToString() != "Eagles" && tankCanPickUp && other.GetComponent<Flag>().canBePickedUp)
                {
                    other.gameObject.transform.SetParent(gameObject.transform);
                    other.gameObject.transform.localPosition = new Vector3(0, 0, 0);
                    other.gameObject.transform.localScale = new Vector3(1, 1, 1);
                    other.GetComponent<Flag>().canBePickedUp = false;
                    tankCanPickUp = false;
                }
            }else if(other.tag == "WolfFlag"){
                if (PhotonNetwork.player.customProperties["Team"].ToString() != "Wolves" && tankCanPickUp && other.GetComponent<Flag>().canBePickedUp){
                    other.gameObject.transform.SetParent(gameObject.transform);
                    other.gameObject.transform.localPosition = new Vector3(0, 0, 0);
                    other.gameObject.transform.localScale = new Vector3(1, 1, 1);
                    other.GetComponent<Flag>().canBePickedUp = false;
                    tankCanPickUp = false;
                }
            }
            else if (other.tag == "ExorcistFlag")
            {
                if (PhotonNetwork.player.customProperties["Team"].ToString() != "Exorcist" && tankCanPickUp && other.GetComponent<Flag>().canBePickedUp)
                {
                    other.gameObject.transform.SetParent(gameObject.transform);
                    other.gameObject.transform.localPosition = new Vector3(0, 0, 0);
                    other.gameObject.transform.localScale = new Vector3(1, 1, 1);
                    other.GetComponent<Flag>().canBePickedUp = false;
                    tankCanPickUp = false;
                }
            }
        }
    }
}
