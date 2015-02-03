﻿using UnityEngine;
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
}
