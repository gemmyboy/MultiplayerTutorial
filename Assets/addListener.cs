using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class addListener : MonoBehaviour {
    Start_Menu_Server_Check networkCheck;
    GameObject currentSelectedObject;
    void Start()
    {
        networkCheck = FindObjectOfType<Start_Menu_Server_Check>();
        gameObject.GetComponent<Button>().onClick.AddListener(delegate { networkCheck.ChangeColor(gameObject); });
    }

    void OnClick()
    {
        if(!currentSelectedObject == null){
            currentSelectedObject.SetActive(true);

        }
        else
        {
            currentSelectedObject = gameObject;
            gameObject.SetActive(false);
        }
    }
}
