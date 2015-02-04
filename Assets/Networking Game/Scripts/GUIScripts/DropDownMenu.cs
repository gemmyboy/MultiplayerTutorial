using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using ExitGames.Client.Photon;
using System.Collections.Generic;
using System.Linq;
public class DropDownMenu : MonoBehaviour {

    public void DropDown(Animator anim)
    {
        if (anim.GetBool("isOpen"))
        {
            anim.SetBool("isOpen", false);
        }
        else
        {
            anim.SetBool("isOpen", true);
        }
    }

    public void CloseMenu()
    {
        GetComponent<Animator>().SetBool("isOpen",false);
        GetComponent<Animator>().SetBool("Closing", true);
        StartCoroutine(DisablePanelDeleyed(gameObject.GetComponent<Animator>()));

    }

    public IEnumerator DisablePanelDeleyed(Animator anim)
    {
        bool closedStateReached = false;
        bool wantToClose = true;
        while (!closedStateReached && wantToClose)
        {
            if (!anim.IsInTransition(0))
                closedStateReached = anim.GetCurrentAnimatorStateInfo(0).IsName("Closed");

            wantToClose = anim.GetBool("Closing");

            yield return new WaitForEndOfFrame();
        }
        if (wantToClose)
        {
            anim.gameObject.SetActive(false);
        }

    }

}
