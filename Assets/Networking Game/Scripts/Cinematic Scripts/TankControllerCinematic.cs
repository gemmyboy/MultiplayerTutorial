using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TankControllerCinematic : Photon.MonoBehaviour
{
    Transform[] children;
    void Start()
    {
        GetComponent<TankController>().enabled = false;
        GetComponentInChildren<TankGunController>().enabled = false;

        transform.DetachChildren();
        GameObject.Find("MainGun").AddComponent<BoxCollider>();
        GameObject.Find("Barrel").AddComponent<BoxCollider>();

        detachMultiple(GameObject.Find("WheelTransforms_L"));
        detachMultiple(GameObject.Find("WheelTransforms_R"));
        Destroy(GameObject.Find("Tracks"));


        Collider[] colliders = Physics.OverlapSphere(gameObject.transform.position, 60000);
        foreach (Collider hit in colliders)
        {
            if (hit.name != "Terrain")
            {
                hit.gameObject.AddComponent<Rigidbody>();
            }
            if (hit && hit.rigidbody)
            {
                hit.rigidbody.isKinematic = false;
                hit.rigidbody.AddExplosionForce(100, gameObject.transform.position, 500, 3);
            }
        }
    }

    void Update()
    {

    }


    void detachMultiple(GameObject obj)
    {
        foreach(Transform child in obj.transform){
            child.gameObject.AddComponent<BoxCollider>();
            child.DetachChildren();
        }
    }
}