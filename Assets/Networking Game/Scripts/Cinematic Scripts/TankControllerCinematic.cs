using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TankControllerCinematic : Photon.MonoBehaviour
{
    Transform[] children;
    GameObject trash;
    void Start()
    {
        //trash = new GameObject("TankTrash");

        GetComponent<TankController>().enabled = false;
        Destroy(GetComponentInChildren<TankGunController>());
        transform.DetachChildren();

        fixForExplosion(GameObject.Find("MainGun"));

        detachMultiple(GameObject.Find("WheelTransforms_L"));
        detachMultiple(GameObject.Find("WheelTransforms_R"));
        detachMultiple(GameObject.Find("UselessGearsTransforms_L"));
        detachMultiple(GameObject.Find("UselessGearsTransforms_R"));

        DestroyHinge(GameObject.Find("Skirts_L"));
        DestroyHinge(GameObject.Find("Skirts_R"));

        detachMultiple(GameObject.Find("Skirts_L"));
        detachMultiple(GameObject.Find("Skirts_R"));


        Destroy(GameObject.Find("Tracks"));
        Destroy(GameObject.Find("HeadLights"));
        Destroy(GameObject.Find("HeavyExhaust"));
        Destroy(GameObject.Find("NormalExhaust"));

        Collider[] colliders = Physics.OverlapSphere(gameObject.transform.position, 60000);
        foreach (Collider hit in colliders)
        {
            if (hit.name != "Terrain")
            {
                if(hit.GetComponent<Rigidbody>() == null){
                    hit.gameObject.AddComponent<Rigidbody>();
                }
            }
            if (hit && hit.rigidbody)
            {
                hit.rigidbody.isKinematic = false;
                hit.rigidbody.AddExplosionForce(1000, gameObject.transform.position, 500, 3);
            }
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R)){
            GameObject[] trash = GameObject.FindGameObjectsWithTag("Trash");
            foreach(GameObject obj in trash){
                Destroy(obj);
            }
        }
    }

    void fixForExplosion(GameObject obj)
    {
        obj.AddComponent<BoxCollider>();
        obj.GetComponent<Rigidbody>().useGravity = true;
        if(obj.GetComponent<HingeJoint>() != null){
            Destroy(obj.GetComponent<HingeJoint>());
        }
    }

    void DestroyHinge(GameObject obj)
    {
        foreach (Transform child in obj.transform)
        {
            Destroy(child.GetComponent<HingeJoint>());
        }
    }

    void detachMultiple(GameObject obj)
    {
        obj.tag = "Trash";
        if (checkObject(obj))
        {
            return;
        }
        foreach(Transform child in obj.transform){
            if(child.GetComponent<BoxCollider>() == null){
                child.gameObject.AddComponent<BoxCollider>();
            }
            child.tag = "Trash";
            child.DetachChildren();
        }
        obj.transform.DetachChildren();
    }
    bool checkObject(GameObject obj)
    {
        string[] list = new string[]{"Tracks","MainGun","Barrel"};
        foreach(string sting in list){
            Debug.Log(obj.name);
            if(sting == obj.name){
                return true;
            }
        }
        return false;
    }
}