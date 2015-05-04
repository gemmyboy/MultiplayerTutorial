using UnityEngine;
using System.Collections;

public class PauseMenu : MonoBehaviour {
    public GameObject pauseMenu;
    public bool pausemenuActivated = false;
    public bool optionsActivated = false;
    PanelManager manager;
    public Animator menu;
    public Animator options;

    public GameTimeManager gameTime;
	// Use this for initialization
	void Start () {
        gameTime = FindObjectOfType<GameTimeManager>();
        manager = FindObjectOfType<PanelManager>();
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Escape) && !gameTime.IsItTimeYet)
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
