using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class selectGameModeUI : MonoBehaviour {
    EventSystem system;

    void Start()
    {
        system = FindObjectOfType<EventSystem>();
    }
    public void changeAnimator(GameObject otherButton)
    {
        GameObject button = system.currentSelectedGameObject;
        if (button.GetComponent<Animator>().enabled)
        {
            button.GetComponent<Animator>().enabled = false;
        }
        changeOtherAnimator(otherButton);
    }

    void changeOtherAnimator(GameObject otherButton)
    {
        otherButton.GetComponent<Animator>().enabled = true;
    }
}
