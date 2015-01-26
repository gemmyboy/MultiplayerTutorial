using UnityEngine;
using System.Collections;

public class Start_Screen_Sound : MonoBehaviour {
    PhotonView view;
    public AudioClip countdown;
    void Start()
    {
        view = gameObject.GetComponent<PhotonView>();
    }

    public void StartGameCountdown()
    {
        view.RPC("PlayCountDown",PhotonTargets.AllViaServer);
    }

    [RPC]
    void PlayCountDown()
    {
        audio.PlayOneShot(countdown);
    }

}
