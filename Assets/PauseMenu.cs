using UnityEngine;
using System.Collections;

public class PauseMenu : MonoBehaviour {
    public GameObject pauseMenu;
    public bool pausemenuActivated = false;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(!pausemenuActivated){
                pauseMenu.SetActive(true);
                Screen.lockCursor = false;
                pausemenuActivated = true;
            }
            else
            {
                pauseMenu.SetActive(false);
                Screen.lockCursor = true;
                pausemenuActivated = true;
            }


        }
	}
}
