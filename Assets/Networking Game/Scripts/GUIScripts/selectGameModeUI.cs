using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class selectGameModeUI : MonoBehaviour {
    EventSystem system;
    GameObject otherButton;
    void Start()
    {
        system = FindObjectOfType<EventSystem>();
    }
    public void changeAnimator()
    {
        GameObject button = system.currentSelectedGameObject;
        if (button.GetComponent<Animator>().enabled)
        {
            button.GetComponent<Animator>().enabled = false;
        }
        changeOtherAnimator();
    }

    void changeOtherAnimator()
    {
        otherButton.GetComponent<Animator>().enabled = true;
    }
    public void OtherButton()
    {
        otherButton = system.currentSelectedGameObject;
    }
}
