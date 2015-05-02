using UnityEngine;
using System.Collections;

public class PickUpFlag_CapFlag : MonoBehaviour
{
    public bool tankCanPickUp = true;
    public GameObject currentFlagPossession;
    UIManager manager;
    void Start()
    {
        manager = FindObjectOfType<UIManager>();
    }
	// Use this for initialization
    void OnTriggerEnter(Collider other)
    {
        //------------------------------------------------------------------------------------------------------------------------------------------------------
        //PICKUP THE FLAG
        if (other.gameObject.layer == LayerMask.NameToLayer("Flag"))
        {
            if (other.tag == "BloodFlag")
            {
                if (PhotonNetwork.player.customProperties["Team"].ToString() != "Angel" && tankCanPickUp && other.GetComponent<Flag>().canBePickedUp)
                {
                    other.gameObject.GetComponent<wisp>().enabled = false;
                    other.gameObject.GetComponent<OrbRotate>().enabled = true;
                    other.gameObject.GetComponent<OrbRotate>().orbitObject = gameObject;
                    other.GetComponent<Flag>().canBePickedUp = false;
                    tankCanPickUp = false;
                    currentFlagPossession = other.gameObject;
                }
            }
            else if (other.tag == "EagleFlag")
            {
                if (PhotonNetwork.player.customProperties["Team"].ToString() != "Eagles" && tankCanPickUp && other.GetComponent<Flag>().canBePickedUp)
                {
                    other.gameObject.GetComponent<wisp>().enabled = false;
                    other.gameObject.GetComponent<OrbRotate>().enabled = true;
                    other.gameObject.GetComponent<OrbRotate>().orbitObject = gameObject;
                    other.GetComponent<Flag>().canBePickedUp = false;
                    tankCanPickUp = false;
                    currentFlagPossession = other.gameObject;
                }
            }
            else if (other.tag == "WolfFlag")
            {
                if (PhotonNetwork.player.customProperties["Team"].ToString() != "Wolves" && tankCanPickUp && other.GetComponent<Flag>().canBePickedUp)
                {
                    other.gameObject.GetComponent<wisp>().enabled = false;
                    other.gameObject.GetComponent<OrbRotate>().enabled = true;
                    other.gameObject.GetComponent<OrbRotate>().orbitObject = gameObject;
                    other.GetComponent<Flag>().canBePickedUp = false;
                    tankCanPickUp = false;
                    currentFlagPossession = other.gameObject;
                }
            }
            else if (other.tag == "ExorcistFlag")
            {
                if (PhotonNetwork.player.customProperties["Team"].ToString() != "Exorcist" && tankCanPickUp && other.GetComponent<Flag>().canBePickedUp)
                {
                    other.gameObject.GetComponent<wisp>().enabled = false;
                    other.gameObject.GetComponent<OrbRotate>().enabled = true;
                    other.gameObject.GetComponent<OrbRotate>().orbitObject = gameObject;
                    other.GetComponent<Flag>().canBePickedUp = false;
                    tankCanPickUp = false;
                    currentFlagPossession = other.gameObject;
                }
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------
        //CAPTURING THE FLAG
        if (other.gameObject.layer == LayerMask.NameToLayer("FlagCapture"))
        {
            ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();

            if (other.tag == "BloodCapture")
            {
                if (PhotonNetwork.player.customProperties["Team"].ToString() == "Angel" && !tankCanPickUp && !other.GetComponent<Flag>().canBePickedUp)
                {
                    hash.Add("Captures", (int)PhotonNetwork.player.customProperties["Captures"] + 1);
                    PhotonNetwork.player.SetCustomProperties(hash);
                    resetFlag();
                    tankCanPickUp = true;
                    UpdateUI();
                    GameObject fireworks = PhotonNetwork.Instantiate("FireworksShow", transform.position, new Quaternion(270, 0, 0, 0), 0) as GameObject;
                    GetComponent<PhotonView>().RPC("fixLetters", PhotonTargets.All, fireworks.GetComponent<PhotonView>().viewID);
                }
            }
            else if (other.tag == "EagleCapture")
            {
                if (PhotonNetwork.player.customProperties["Team"].ToString() == "Eagles" && !tankCanPickUp) //&& !other.GetComponent<Flag>().canBePickedUp)
                {
                    hash.Add("Captures", (int)PhotonNetwork.player.customProperties["Captures"] + 1);
                    PhotonNetwork.player.SetCustomProperties(hash);
                    resetFlag();
                    tankCanPickUp = true;
                    UpdateUI();
                    GameObject fireworks = PhotonNetwork.Instantiate("FireworksShow", transform.position, new Quaternion(270, 0, 0, 0), 0) as GameObject;
                    GetComponent<PhotonView>().RPC("fixLetters", PhotonTargets.All, fireworks.GetComponent<PhotonView>().viewID);
                }
            }
            else if (other.tag == "WolfCapture")
            {
                if (PhotonNetwork.player.customProperties["Team"].ToString() == "Wolves" && !tankCanPickUp && !other.GetComponent<Flag>().canBePickedUp)
                {
                    hash.Add("Captures", (int)PhotonNetwork.player.customProperties["Captures"] + 1);
                    PhotonNetwork.player.SetCustomProperties(hash);
                    resetFlag();
                    tankCanPickUp = true;
                    UpdateUI();
                    GameObject fireworks = PhotonNetwork.Instantiate("FireworksShow", transform.position, new Quaternion(270, 0, 0, 0), 0) as GameObject;
                    GetComponent<PhotonView>().RPC("fixLetters", PhotonTargets.All, fireworks.GetComponent<PhotonView>().viewID);
                }
            }
            else if (other.tag == "ExorcistCapture")
            {
                if (PhotonNetwork.player.customProperties["Team"].ToString() == "Exorcist" && !tankCanPickUp && !other.GetComponent<Flag>().canBePickedUp)
                {
                    hash.Add("Captures", (int)PhotonNetwork.player.customProperties["Captures"] + 1);
                    PhotonNetwork.player.SetCustomProperties(hash);
                    resetFlag();
                    tankCanPickUp = true;
                    UpdateUI();
                    GameObject fireworks = PhotonNetwork.Instantiate("FireworksShow", transform.position, new Quaternion(270, 0, 0, 0), 0) as GameObject;
                    GetComponent<PhotonView>().RPC("fixLetters", PhotonTargets.All, fireworks.GetComponent<PhotonView>().viewID);
                }
            }
            //------------------------------------------------------------------------------------------------------------------------------------------------------
        }
    }

    void UpdateUI()
    {
        manager.changeCaptures((int)PhotonNetwork.player.customProperties["Captures"]);
    }

    void resetFlag()
    {
        if (currentFlagPossession.tag == "EagleFlag")
        {
            currentFlagPossession.GetComponent<OrbRotate>().enabled = false;
            currentFlagPossession.GetComponent<OrbRotate>().orbitObject = null;
            currentFlagPossession.transform.position = GameObject.Find("EaglesSpawnPoint").transform.position + new Vector3(0, 15, 0);
            currentFlagPossession.GetComponent<Flag>().canBePickedUp = true;
        }
        else if (currentFlagPossession.tag == "ExorcistFlag")
        {
            currentFlagPossession.GetComponent<OrbRotate>().enabled = false;
            currentFlagPossession.GetComponent<OrbRotate>().orbitObject = null;
            currentFlagPossession.transform.position = GameObject.Find("ExorcistSpawnPoint").transform.position + new Vector3(0, 15, 0);
            currentFlagPossession.GetComponent<Flag>().canBePickedUp = true;
        }
        else if (currentFlagPossession.tag == "BloodFlag")
        {
            currentFlagPossession.GetComponent<OrbRotate>().enabled = false;
            currentFlagPossession.GetComponent<OrbRotate>().orbitObject = null;
            currentFlagPossession.transform.position = GameObject.Find("BloodSpawnPoint").transform.position + new Vector3(0, 15, 0);
            currentFlagPossession.GetComponent<Flag>().canBePickedUp = true;
        }
        else if (currentFlagPossession.tag == "WolfFlag")
        {
            currentFlagPossession.GetComponent<OrbRotate>().enabled = false;
            currentFlagPossession.GetComponent<OrbRotate>().orbitObject = null;
            currentFlagPossession.transform.position = GameObject.Find("WolfSpawnPoint").transform.position + new Vector3(0, 15, 0);
            currentFlagPossession.GetComponent<Flag>().canBePickedUp = true;
        }
    }
    GameObject fireworks;
    [RPC]
    void fixLetters(int ID)
    {
        PhotonView[] views = FindObjectsOfType<PhotonView>();
        foreach (PhotonView view in views)
        {
            if(view.viewID == ID){
                fireworks = view.gameObject;
            }
        }
        fireworks.transform.Rotate(90, 0, 0);
    }
}
