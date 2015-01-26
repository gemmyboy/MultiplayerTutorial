/*
 * Copyright (c) 2013 LowLevelTech
 * Alex Dunn
 * llt@dunnalex.com
*/

using UnityEngine;

namespace Fluidity.Renderer
{
    /// <summary>
    /// Transforms the attached GameObject (<see cref="UnityEngine.GameObject"/>) relative to user inputs from the keyboard and mouse.
    /// The mouse controls the rotation (simple FPS style controls) and the keyboard controls movement.
    /// 
    /// Key Layout:
    /// -------------------------
    /// W       - Forward       
    /// S       - Backward      
    /// A       - Left          
    /// S       - Right         
    /// Q       - Up            
    /// E       - Down          
    /// LShift  - Pedal faster  
    /// -------------------------
    /// 
    /// Note: This script is typically used on a camera, it crudely mimics the behaviour of the Unity scene editor camera.
    /// </summary>
    public class FlyCamera : MonoBehaviour
    {
        /// <summary>
        /// The desired speed at which to move WHEN NOT holding the Left Shift key to move faster.  
        /// Note: Measurements in metres per second.
        /// </summary>
        [SerializeField]
        private float moveSpeed = 4.0f;

        /// <summary>
        /// The desired speed at which to move WHEN holding the Left Shift key to move faster.  
        /// Note: Measurements in metres per second.
        /// </summary>
        [SerializeField]
        private float fastMoveSpeed = 12.0f;

        /// <summary>
        /// The speed at which to spin on the Y axis (left to right).
        /// Note: Measurements in degrees per second.
        /// </summary>
        [SerializeField]
        private float yawSpeed = 250.0f;

        /// <summary>
        /// The speed at which to spin on the X axis (up or down).
        /// Note: Measurements in degrees per second.
        /// </summary>
        [SerializeField]
        private float pitchSpeed = 120.0f;

        /// <summary>
        /// The current angle of yaw.
        /// Note: In degrees.
        /// </summary>
        private float currentYaw;

        /// <summary>
        /// The current angle of pitch.
        /// Note: In degrees.
        /// </summary>
        private float currentPitch;

        /// <summary>
        /// Unity callback.
        /// 
        /// Initialize the currentYaw and currentPitch variables with that of this objects transform.
        /// </summary>
        private void Start()
        {
            Vector3 initialAngles = transform.eulerAngles;

            currentYaw = initialAngles.y;
            currentPitch = initialAngles.x;
        }

        /// <summary>
        /// Unity callback.
        /// 
        /// Updates this objects transform based on user input if the RMB is held down.
        /// 
        /// Note: This function terminates early if the RMB is not held down.
        /// </summary>
        private void Update()
        {
            if (!Input.GetKey(KeyCode.Mouse1)) return;

            currentYaw += Input.GetAxisRaw("Mouse X") * yawSpeed * Time.deltaTime;
            currentPitch -= Input.GetAxisRaw("Mouse Y") * pitchSpeed * Time.deltaTime;

            transform.rotation = Quaternion.Euler(currentPitch, currentYaw, 0);
            transform.position += transform.TransformDirection(new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Lift"), Input.GetAxis("Vertical")) * Time.deltaTime * (Input.GetKey(KeyCode.LeftShift) ? fastMoveSpeed : moveSpeed));
        }
    }
}