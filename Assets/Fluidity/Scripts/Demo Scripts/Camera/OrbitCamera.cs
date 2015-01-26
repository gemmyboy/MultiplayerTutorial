/*
 * Copyright (c) 2013 LowLevelTech
 * Alex Dunn
 * llt@dunnalex.com
*/

using UnityEngine;

namespace Fluidity.Renderer
{
    /// <summary>
    /// Orbis the attached object around another Transform (<see cref="UnityEngine.Transform"/>) based on user input.
    /// 
    /// Note: Mouse input controls the rotation which the RMB is held down.
    /// </summary>
    public class OrbitCamera : MonoBehaviour
    {
        /// <summary>
        /// Orbital target.
        /// </summary>
        [SerializeField] private Transform target;

        /// <summary>
        /// Constant distance to remain from the target.
        /// </summary>
        [SerializeField] private float distance = 10.0f;

        /// <summary>
        /// Yaw speed.
        /// Note: Degrees per second.
        /// </summary>
        [SerializeField] private float xSpeed = 250.0f;

        /// <summary>
        /// Pitch speed.
        /// Note: Degrees per second.
        /// </summary>
        [SerializeField] private float ySpeed = 120.0f;

        /// <summary>
        /// Lower limit used to clamp the pitch within specified range.
        /// Note: Degrees.
        /// </summary>
        [SerializeField] private float yMinLimit = -20f;

        /// <summary>
        /// Upper limit used to clamp the pitch within specified range.
        /// Note: Degrees.
        /// </summary>
        [SerializeField] private float yMaxLimit = 80f;

        /// <summary>
        /// The current yaw angle of rotation.
        /// Note: Degrees.
        /// </summary>
        private float currentYaw;
        
        /// <summary>
        /// The current pitch angle of rotation.
        /// Note: Degrees.
        /// </summary>
        private float currentPitch;  

        /// <summary>
        /// Unity callback.
        /// 
        /// Initializes the current yaw and pitch values from the attached objects transform.
        /// Places the object in it's initial orbital location.
        /// </summary>
        private void Start()
        {
            Vector3 initialAngles = transform.eulerAngles;
            currentYaw = initialAngles.y;
            currentPitch = initialAngles.x;
            currentPitch = ClampAngle(currentPitch, yMinLimit, yMaxLimit);

            UpdateTransform(currentYaw, currentPitch);
        }

        /// <summary>
        /// Unity callback.
        /// 
        /// Updates the objects transform based of mouse input when the RMB is held down.
        /// 
        /// Note: This function terminates early if the RMB is not held down or no target has been specified in the editor.
        /// </summary>
        private void Update()
        {
            if (target == null || !Input.GetKey(KeyCode.Mouse1)) return;

            currentYaw += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            currentPitch -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

            currentPitch = ClampAngle(currentPitch, yMinLimit, yMaxLimit);

            UpdateTransform(currentYaw, currentPitch);
        }

        /// <summary>
        /// Updates the transform position and rotation based of the yaw pitch and distance values specified.
        /// </summary>
        /// <param name="yaw">Angle of yaw to update transform with. (Degrees)</param>
        /// <param name="pitch">Angle of pitch to update transform with. (Degrees)</param>
        private void UpdateTransform(float yaw, float pitch)
        {
            transform.rotation = Quaternion.Euler(pitch, yaw, 0);
            transform.position = transform.rotation * new Vector3(0.0f, 0.0f, -distance) + target.position;
        }

        /// <summary>
        /// Helper function used to clamp and angle between two specified min and max values.
        /// Note: Cannot use normal math clamp function (<see cref="Mathf.Clamp(float, float, float)"/> as the angle must take into account the wrap around at +- 360 degrees.
        /// </summary>
        /// <param name="angle">Angle to clamp.</param>
        /// <param name="min">Lower clamp bound.</param>
        /// <param name="max">Upper clamp bound.</param>
        /// <returns>Angle clamped between min and max.</returns>
        private static float ClampAngle (float angle, float min, float max ) 
        {
	        if (angle < -360)
	        {
	            angle += 360;
	        }

	        if (angle > 360)
	        {
	            angle -= 360;
	        }

	        return Mathf.Clamp (angle, min, max);
        }
    }
}

