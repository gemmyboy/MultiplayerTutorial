using UnityEngine;
using System.Collections;

public class GameMusicManager : Photon.MonoBehaviour {
    public PhotonView m_PhotonView;
    public AudioClip hardcoreMusic;
    void Start()
    {
        m_PhotonView = GetComponent<PhotonView>();
        if(PhotonNetwork.isMasterClient){
            GameObject gameMusic = PhotonNetwork.Instantiate("GameMusic", transform.position, transform.rotation, 0) as GameObject;
            int viewID = gameMusic.GetComponent<PhotonView>().viewID;
            m_PhotonView.RPC("createMusic", PhotonTargets.AllBuffered,viewID);
        }
    }

    GameObject gameMusic;
    [RPC]
    void createMusic(int viewid)
    {
        PhotonView[] photonViews = FindObjectsOfType<PhotonView>();
        foreach(PhotonView view in photonViews){
            if(view.viewID == viewid){
                gameMusic = view.gameObject;
            }
        }

        gameMusic.transform.position = transform.position;
        gameMusic.transform.rotation = transform.rotation;
        gameMusic.transform.parent = transform;
        gameMusic.AddComponent<AudioSource>();
        gameMusic.audio.clip = hardcoreMusic;
        gameMusic.audio.loop = true;
        gameMusic.audio.Play();
    }
}
