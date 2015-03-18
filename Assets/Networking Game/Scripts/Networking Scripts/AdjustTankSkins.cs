using UnityEngine;
using System.Collections;

public class AdjustTankSkins : Photon.MonoBehaviour {

	public Material eagleMaterial;
	public Material excorsistMaterial;
	public Material wolfMaterial;
	public Material angelMaterial;
	private MeshRenderer[] meshes;
	private bool teamed;
	
	// Use this for initialization
	void Start () 
	{
		teamed = false;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(!teamed && photonView.isMine)
		{
			if(this.photonView.owner.customProperties["Team"] == "Eagles")
			{
				teamed = true;
				photonView.RPC("ChangeMyTexture",PhotonTargets.All,1);

			}else if(this.photonView.owner.customProperties["Team"] == "Excorcist")
			{
				teamed = true;
				photonView.RPC("ChangeMyTexture",PhotonTargets.All,2);

			}else if(this.photonView.owner.customProperties["Team"] == "Wolves")
			{
				teamed = true;
				photonView.RPC("ChangeMyTexture",PhotonTargets.All,3);
				
			}else if(this.photonView.owner.customProperties["Team"] == "Angel")
			{
				teamed = true;
				photonView.RPC("ChangeMyTexture",PhotonTargets.All,4);

			}
		}
	}

	[RPC]
	void ChangeMyTexture(int team)
	{
		meshes = this.GetComponentsInChildren<MeshRenderer>();

		if(team == 1)
		{
			foreach(MeshRenderer theMesh in meshes)
			{
				if ((theMesh.name == "MainGun Mesh"))
				{
					theMesh.gameObject.renderer.materials[1].CopyPropertiesFromMaterial(eagleMaterial);
					Debug.Log("EAGLE TURRET MESH SHOULD BE SET");
				}
				else{
					theMesh.gameObject.renderer.material = eagleMaterial;
					Debug.Log("EAGLE MESH SHOULD BE SET");
				}
			}
		}else if(team == 2)
		{
			foreach(MeshRenderer theMesh in meshes)
			{
				if ((theMesh.name == "MainGun Mesh"))
				{
					theMesh.gameObject.renderer.materials[1].CopyPropertiesFromMaterial(excorsistMaterial);
					Debug.Log("EXCORSIST TURRET MESH SHOULD BE SET");
				}
				else{
					theMesh.gameObject.renderer.material = excorsistMaterial;
					Debug.Log("EXCORSIST MESH SHOULD BE SET");
				}
			}
		}else if(team == 3)
		{
			foreach(MeshRenderer theMesh in meshes)
			{
				if ((theMesh.name == "MainGun Mesh"))
				{
					theMesh.gameObject.renderer.materials[1].CopyPropertiesFromMaterial(wolfMaterial);
					Debug.Log("WOLVES TURRET MESH SHOULD BE SET");
				}
				else{
					theMesh.gameObject.renderer.material = wolfMaterial;
					Debug.Log("WOLVES MESH SHOULD BE SET");
				}
			}
		}else if(team == 4)
		{
			foreach(MeshRenderer theMesh in meshes)
			{
				if ((theMesh.name == "MainGun Mesh"))
				{
					theMesh.gameObject.renderer.materials[1].CopyPropertiesFromMaterial(angelMaterial);
					Debug.Log("ANGEL TURRET MESH SHOULD BE SET");
				}
				else{
					theMesh.gameObject.renderer.material = angelMaterial;
					Debug.Log("ANGEL MESH SHOULD BE SET");
				}
			}
		}
	}
}
