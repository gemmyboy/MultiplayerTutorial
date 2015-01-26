/*
 * Copyright (c) 2013 LowLevelTech
 * Alex Dunn
 * llt@dunnalex.com
*/

using UnityEngine;

namespace Fluidity.DemoUtilities
{
    /// <summary>
    /// A component that will set the Y axis rotation of this object to a random number on awake, and then destroy itself.
    /// </summary>
    public class RandomRotation : MonoBehaviour 
    {
	    private void Awake () 
        {
	        transform.Rotate(Vector3.up, Random.Range(-180, 180));
            Destroy(this);
	    }
    }
}