using UnityEngine;
using System.Collections;

public class RotateYaOwl : MonoBehaviour 
{
    public float owlsPerSecond;

    public GameObject Anchor;

    Vector3 rotationAxis;

    

	void Start () 
    {
        if (Anchor != null)
        {
            Vector3 axis = transform.position - Anchor.transform.position;
            rotationAxis = Vector3.Cross(axis, Anchor.transform.forward);

        }
        
	}
	
	void Update () 
    {
        //transform.LookAt(Anchor.transform);


        //Debug.DrawLine(Anchor.transform.position, Anchor.transform.position + rotationAxis);

        if (Anchor != null)
        {
            transform.RotateAround(Anchor.transform.position, rotationAxis.normalized, owlsPerSecond);
        }

        else
        {
            transform.Rotate(Vector3.forward, owlsPerSecond, Space.World);
        }

        
	}


    public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion angle)
    {
        return angle * (point - pivot) + pivot;
    }
}
