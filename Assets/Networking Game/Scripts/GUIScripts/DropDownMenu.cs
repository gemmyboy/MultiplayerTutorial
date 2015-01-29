using UnityEngine;
using System.Collections;

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
}
