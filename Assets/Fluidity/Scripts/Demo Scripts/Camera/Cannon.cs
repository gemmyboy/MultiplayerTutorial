/*
 * Copyright (c) 2013 LowLevelTech
 * Alex Dunn
 * llt@dunnalex.com
*/

using UnityEngine;

namespace Fluidity.Renderer
{
    /// <summary>
    /// A simple script that will fire Rigidbody (<see cref="UnityEngine.Rigidbody"/>) projectiles from the screen whenever the user clicks the LMB.
    /// 
    /// Note: This script requires a Camera (<see cref="UnityEngine.Camera"/>) component.
    /// </summary>
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class Cannon : MonoBehaviour
    {
        /// <summary>
        /// Desired Rigidbody (<see cref="UnityEngine.Rigidbody"/>) prefab to fire on mouse click.
        /// </summary>
        [SerializeField]
        private Rigidbody projectile;

        /// <summary>
        /// The force to which gets applied to the projectile as it is spawned.
        /// </summary>
        [SerializeField]
        private float fireForce = 1000;

        #region Unity API
        /// <summary>
        /// Unity callback.  
        /// 
        /// Draws a message on the screen explaining how the end user benefits from this component.
        /// </summary>
        private void OnGUI()
        {
            GUILayout.Label("Press the LMB to fire projectile.");
        }

        /// <summary>
        /// Unity callback.
        /// 
        /// Checks for a left mouse button press and spawns a projectile on succesful check.
        /// Note: If no projectile has been specified in the inspector, this function terminates early.
        /// </summary>
        private void Update()
        {
            if (projectile == null) return;

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Ray fireRay = camera.ScreenPointToRay(Input.mousePosition + Vector3.forward);
                Rigidbody r;
                (r = Instantiate(projectile, fireRay.origin, Quaternion.LookRotation(fireRay.direction)) as Rigidbody).AddForce(fireRay.direction * fireForce);
                Destroy(r.gameObject, 4);
            }
        }

        #endregion
    }
}