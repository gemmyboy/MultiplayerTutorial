using UnityEngine;
using System.Collections;

public class WSP_HRezBeamTurret : MonoBehaviour {

	public Transform LookAtTargetTrans;
	public Transform BeamWeaponFiringPoint;
	public WSP_HighRezLaser LaserWeapon;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (BeamWeaponFiringPoint != null) {
			if (LaserWeapon != null) {
				LaserWeapon.gameObject.transform.position = BeamWeaponFiringPoint.position;
				if (LaserWeapon.CurrentTarget != null) {
					LookAtTargetTrans.LookAt(LaserWeapon.CurrentTarget.position);
				}
			}
		}
	}
}
