using UnityEngine;
using System.Collections;

public class CraneControl_QR2 : MonoBehaviour 
{
    public Transform craneRotationObject = null;
    public Transform trollyObject = null;
    public Transform trollyInStop = null;
    public Transform trollyOutStop = null;

    public float rotationVelocity = 0;
    public float rotationAccel = 3;
    public float maxRotationVelocity = 100;
    public float rotationDampening = 0.985f;

    private float trollyVelocity = 0;
    public float trollyAccel = 3;
    public float maxTrollyVelocity = 7;
    public float trollyDampening = 0.985f;
    private float trollyPosition = 0.5f;

	void Update () 
    {
        rotationVelocity = Mathf.Clamp(rotationVelocity, -maxRotationVelocity, maxRotationVelocity);
        craneRotationObject.Rotate(0, rotationVelocity * Time.deltaTime, 0);
	}
}
