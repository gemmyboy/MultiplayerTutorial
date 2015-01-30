using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class DropDownMenu : MonoBehaviour {

    void Update()
    {
    }
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
