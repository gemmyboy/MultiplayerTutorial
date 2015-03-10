using UnityEngine;
using System.Collections;

public class GameMusicManager : MonoBehaviour {
    public PhotonView m_PhotonView;
    public GameObject gameMusic;
    public AudioClip hardcoreMusic;
    void Start()
    {
        m_PhotonView = GetComponent<PhotonView>();
        if(PhotonNetwork.isMasterClient){
            gameMusic = new GameObject("GameMusic");
            gameMusic.transform.position = transform.position;
            gameMusic.transform.rotation = transform.rotation;
            gameMusic.transform.parent = transform;
            gameMusic.AddComponent<AudioSource>();
            gameMusic.audio.clip = hardcoreMusic;
            gameMusic.audio.loop = true;
            gameMusic.audio.Play();
        }
    }
}
