using UnityEngine;
using System.Collections;

public class PauseMenu : MonoBehaviour {
    public GameObject pauseMenu;
    public bool pausemenuActivated = false;
    public bool optionsActivated = false;
    PanelManager manager;
    public Animator menu;
    public Animator options;
	// Use this for initialization
	void Start () {
        manager = FindObjectOfType<PanelManager>();
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
                if(optionsActivated){
                    manager.OpenPanel(menu);
                    optionsActivated = false;
                }
                else
                {
                    pauseMenu.SetActive(false);
                    Screen.lockCursor = true;
                    pausemenuActivated = false;
                }
            }


        }
	}

    public void resumeButton()
    {
        pauseMenu.SetActive(false);
        Screen.lockCursor = true;
        pausemenuActivated = false;
    }

    public void optionButton()
    {
        optionsActivated = true;
    }
}
