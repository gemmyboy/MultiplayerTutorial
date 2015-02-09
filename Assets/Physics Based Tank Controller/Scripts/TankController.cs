	using UnityEngine;
	using System;
	using System.Collections;
	using System.Collections.Generic;

	[RequireComponent (typeof (Rigidbody))]

	public class TankController : MonoBehaviour {

	private bool reversing = false;

	//Wheel colliders of the vehicle.
	public WheelCollider[] WheelColliders_L;
	public WheelCollider[] WheelColliders_R;

	// Wheel transforms of the vehicle.	
	public Transform[] WheelTransform_L;
	public Transform[] WheelTransform_R;
		
	public Transform[] UselessGearTransform_L;
	public Transform[] UselessGearTransform_R;
		
	public Transform[] TrackBoneTransform_L;
	public Transform[] TrackBoneTransform_R;
		
	// Track Customization.
	public GameObject LeftTrackMesh;
	public GameObject RightTrackMesh;
	public float trackOffset = 0f;
	public float trackScrollSpeedMultiplier = 1f;
		
	private float[] RotationValueL;
	private float[] RotationValueR;

	//Center of mass.
	public Transform COM;
	private Transform dynamicCOM;
	private float vehicleSizeX;
	private float vehicleSizeY;

	public int CurrentGear;
	public AnimationCurve EngineTorqueCurve;
	private float[] GearRatio;
	public float gearShiftRate = 7.0f;
	private float gearTimeMultiplier;
	public float EngineTorque = 250.0f;
	public float MaxEngineRPM = 5000.0f;
	public float MinEngineRPM = 1000.0f;
	public float SteerTorque = 3f;
	public float Speed;
	public float Brake = 250.0f;
	public float maxSpeed = 80.0f;
	private float defSteerAngle;
	private float acceleration = 0f;
	private float lastVelocity = 0f;

	private GameObject crashAudio;
	public AudioClip[] crashClips;

	private float EngineRPM = 0.0f;
	private float motorInput;
	private float steerInput;

	private GameObject engineStartUpAudio;
	private GameObject engineIdleAudio;
	private GameObject engineStartRunningAudio;
	private GameObject engineRunningAudio;
	private float pitchValue = 0.0f;
		
	public AudioClip engineStartUpAudioClip;
	public AudioClip engineIdleAudioClip;
	public AudioClip engineRunningAudioClip;

	//Smokes.
	public GameObject WheelSlipPrefab;
	private List <GameObject> WheelParticles = new List<GameObject>();
	private List <WheelCollider> AllWheelColliders = new List<WheelCollider>();
	public ParticleEmitter normalExhaustGas;
	public ParticleEmitter heavyExhaustGas;

	private float sidewaysSlipValue;
	private float defStiffness;
	private WheelFrictionCurve sidewaysFrictionCurve;
	private WheelFrictionCurve forwardFrictionCurve;

    PhotonView m_PhotonView;
		
	void  Start (){
        m_PhotonView = GetComponent<PhotonView>();
		if(m_PhotonView.isMine){
            SetTags();
            EngineStart();
            SoundsInit();
            GearInit();
            SetStiffness();

            if (WheelSlipPrefab)
                //SmokeInit();

            dynamicCOM = new GameObject("Dynamic Com").transform;
            dynamicCOM.parent = transform;

            rigidbody.maxAngularVelocity = 5f;

            RotationValueL = new float[WheelColliders_L.Length];
            RotationValueR = new float[WheelColliders_R.Length];

            Renderer[] r = GetComponentsInChildren<Renderer>();
            if (r.Length > 0)
            {
                Array.Sort(r, delegate(Renderer r1, Renderer r2)
                {
                    return r2.bounds.size.magnitude.CompareTo(r1.bounds.size.magnitude);
                });
                vehicleSizeX = r[0].bounds.size.x;
                vehicleSizeY = r[0].bounds.size.y;
            }
        }
	}

	void SetTags(){
		
		BoxCollider[] bColliders = GetComponentsInChildren<BoxCollider>();
		MeshCollider[] mColliders = GetComponentsInChildren<MeshCollider>();
		WheelCollider[] wColliders = GetComponentsInChildren<WheelCollider>();
		gameObject.layer = LayerMask.NameToLayer("TankCollider");
		
		if(LayerMask.LayerToName(8) != "TankCollider"){
			Debug.LogError ("Couldn't found ''TankCollider'' layer! Create ''TankCollider'' layer for 8th line. You can add layers from Project Settings --> Tags and Layers");
			Debug.Break();
		}else{
			foreach(BoxCollider bCol in bColliders){
				bCol.transform.gameObject.layer = LayerMask.NameToLayer("TankCollider");
			}
			foreach(MeshCollider mCol in mColliders){
				mCol.transform.gameObject.layer = LayerMask.NameToLayer("TankCollider");
			}
		}
		if(LayerMask.LayerToName(9) != "Wheel"){
			Debug.LogError ("Couldn't found ''Wheel'' layer! Create ''Wheel'' layer for 9th line. You can add layers from Project Settings --> Tags and Layers");
			Debug.Break();
		}else{
			foreach(WheelCollider wCol in wColliders){
				wCol.transform.gameObject.layer = LayerMask.NameToLayer("Wheel");
			}
		}
		if(LayerMask.LayerToName(10) != "Bullet"){
			Debug.LogError ("Couldn't found ''Bullet'' layer! Create ''Bullet'' layer for 10th line. You can add layers from Project Settings --> Tags and Layers");
			Debug.Break();
		}
		
	}
		
	void EngineStart(){
			
		engineStartUpAudio = new GameObject("EngineStartUpAudioClip");
		engineStartUpAudio.transform.position = transform.position;
		engineStartUpAudio.transform.rotation = transform.rotation;
		engineStartUpAudio.transform.parent = transform;
		engineStartUpAudio.AddComponent<AudioSource>();
		engineStartUpAudio.audio.clip = engineStartUpAudioClip;
		engineStartUpAudio.audio.minDistance = 10;
		engineStartUpAudio.audio.volume = 0;
		engineStartUpAudio.audio.Play();
		Destroy(engineStartUpAudio, engineStartUpAudioClip.length);
			
	}
		
	void SoundsInit(){

		engineIdleAudio = new GameObject("EngineIdleAudioClip");
		engineIdleAudio.transform.position = transform.position;
		engineIdleAudio.transform.rotation = transform.rotation;
		engineIdleAudio.transform.parent = transform;
		engineIdleAudio.AddComponent<AudioSource>();
		engineIdleAudio.audio.clip = engineIdleAudioClip;
		engineIdleAudio.audio.minDistance = 7;
		engineIdleAudio.audio.volume = .5f;
		engineIdleAudio.audio.loop = true;
		engineIdleAudio.audio.Play();
			
		engineRunningAudio = new GameObject("EngineRunningAudioClip");
		engineRunningAudio.transform.position = transform.position;
		engineRunningAudio.transform.rotation = transform.rotation;
		engineRunningAudio.transform.parent = transform;
		engineRunningAudio.AddComponent<AudioSource>();
		engineRunningAudio.audio.clip = engineRunningAudioClip;
		engineRunningAudio.audio.minDistance = 10;
		engineRunningAudio.audio.volume = 0;
		engineRunningAudio.audio.loop = true;
		engineRunningAudio.audio.Play();
			
	}

	void GearInit(){
		
		GearRatio = new float[EngineTorqueCurve.length];
		
		for(int i = 0; i < EngineTorqueCurve.length; i++){
			
			GearRatio[i] = EngineTorqueCurve.keys[i].value;
			
		}
		
	}

	void SetStiffness(){

		sidewaysFrictionCurve = WheelColliders_L[0].sidewaysFriction;
		forwardFrictionCurve = WheelColliders_L[0].forwardFriction;

		sidewaysFrictionCurve.stiffness = .05f;
		forwardFrictionCurve.stiffness = .1f;

		for(int i = 0; i < AllWheelColliders.Count; i++){
			AllWheelColliders[i].sidewaysFriction = sidewaysFrictionCurve;
			AllWheelColliders[i].forwardFriction = forwardFrictionCurve;
		}

		defStiffness = sidewaysFrictionCurve.stiffness;

	}

	void SmokeInit(){
		
		foreach(WheelCollider w in GameObject.FindObjectsOfType(typeof(WheelCollider)))
		{
			AllWheelColliders.Add (w);
		}
		
		for(int i = 0; i < AllWheelColliders.Count; i++){
			Instantiate(WheelSlipPrefab, AllWheelColliders[i].transform.position, transform.rotation);
		}
		
		foreach(GameObject go in GameObject.FindObjectsOfType(typeof(GameObject)))
		{
			if(go.name == "WheelSlip(Clone)")
				WheelParticles.Add (go);
		}
		
		
		
		for(int i = 0; i < AllWheelColliders.Count; i++){
			
			WheelParticles[i].transform.position = AllWheelColliders[i].transform.position;
			WheelParticles[i].transform.parent = AllWheelColliders[i].transform;
			
		}
		
		
		
	}

	void Update(){
            //WheelAlign();
	}
		
	void  FixedUpdate (){

        if (m_PhotonView.isMine)
        {
            //AnimateGears();
            Engine();
            ShiftGears();
            Bools();
            //SmokeInstantiateRate();

            Speed = rigidbody.velocity.magnitude * 3.6f;

            //Acceleration Calculation.
            acceleration = 0f;
            acceleration = (transform.InverseTransformDirection(rigidbody.velocity).z - lastVelocity) / Time.fixedDeltaTime;
            lastVelocity = transform.InverseTransformDirection(rigidbody.velocity).z;

            //Drag Limit.
            if (Speed < 100)
                rigidbody.drag = Mathf.Clamp((acceleration / 30), 0f, 1f);
            else
                rigidbody.drag = .04f;

            dynamicCOM.localPosition = new Vector3(Mathf.Clamp((transform.InverseTransformDirection(rigidbody.angularVelocity).y * 1), (-vehicleSizeX / 15), (vehicleSizeX / 15)) + COM.localPosition.x, -Mathf.Abs(Mathf.Clamp((transform.InverseTransformDirection(rigidbody.angularVelocity).z * 10), (-vehicleSizeY / 30), (vehicleSizeY / 30))) + COM.localPosition.y, COM.localPosition.z);
            dynamicCOM.rotation = transform.rotation;
            rigidbody.centerOfMass = new Vector3((dynamicCOM.localPosition.x) * transform.localScale.x, (dynamicCOM.localPosition.y) * transform.localScale.y, (dynamicCOM.localPosition.z) * transform.localScale.z);

            //EngineRPM
            EngineRPM = ((((Mathf.Abs((WheelColliders_R[Mathf.CeilToInt((WheelColliders_R.Length) / 2)].rpm * gearShiftRate * Mathf.Clamp01(motorInput)) + (WheelColliders_L[Mathf.CeilToInt((WheelColliders_L.Length) / 2)].rpm * gearShiftRate * Mathf.Clamp01(motorInput)))) * (GearRatio[CurrentGear]) * gearTimeMultiplier)) + MinEngineRPM);

            //Engine Curve
            if (EngineTorqueCurve.keys.Length >= 2)
            {
                if (CurrentGear == EngineTorqueCurve.length - 2) gearTimeMultiplier = (((-EngineTorqueCurve[CurrentGear].time / gearShiftRate) / (maxSpeed * 3)) + 1f); else gearTimeMultiplier = ((-EngineTorqueCurve[CurrentGear].time / (maxSpeed * 3)) + 1f);
            }
            else
            {
                gearTimeMultiplier = 1;
                Debug.Log("You DID NOT CREATE any engine torque curve keys!, Please create 1 key at least...");
            }

            //Audio
            engineIdleAudio.audio.pitch = Mathf.Clamp((Mathf.Abs(EngineRPM) / Mathf.Abs(MaxEngineRPM) + 1), 1f, 2f);
            pitchValue = Mathf.Clamp((Mathf.Abs((EngineRPM) / (MaxEngineRPM)) + .4f), .5f, 1.25f);
            engineRunningAudio.audio.pitch = Mathf.Lerp(engineRunningAudio.audio.pitch, pitchValue, Time.deltaTime * 5);
            engineRunningAudio.audio.volume = Mathf.Lerp(engineRunningAudio.audio.volume, Mathf.Clamp(Mathf.Abs(Input.GetAxis("Vertical") + WheelColliders_R[Mathf.CeilToInt((WheelColliders_R.Length) / 2)].rpm / 500), 0, .75f), Time.deltaTime * 5);

            if (engineStartUpAudio)
                engineStartUpAudio.audio.volume = Mathf.Lerp(engineStartUpAudio.audio.volume, 1, Time.deltaTime * 5);

            for (int i = 0; i < AllWheelColliders.Count; i++)
            {

                if (motorInput == 0)
                {
                    AllWheelColliders[i].brakeTorque = Brake / 5f;
                }
                else if (motorInput < 0 && AllWheelColliders[0].rpm > 0)
                {
                    AllWheelColliders[i].brakeTorque = Brake * (Mathf.Abs(motorInput));
                }
                else
                {
                    AllWheelColliders[i].brakeTorque = 0;
                }

            }

            WheelHit CorrespondingGroundHit;
            WheelColliders_R[Mathf.CeilToInt((WheelColliders_R.Length) / 2)].GetGroundHit(out CorrespondingGroundHit);
            if (WheelColliders_R[Mathf.CeilToInt((WheelColliders_R.Length) / 2)].isGrounded || WheelColliders_L[Mathf.CeilToInt((WheelColliders_L.Length) / 2)].isGrounded)
                sidewaysSlipValue = Mathf.Clamp(Mathf.Lerp(defStiffness, 0f, ((Mathf.Abs(CorrespondingGroundHit.sidewaysSlip)) / 5f)), .02f, defStiffness);
            else
                sidewaysSlipValue = 0;

            sidewaysFrictionCurve.stiffness = Mathf.Lerp(sidewaysFrictionCurve.stiffness, sidewaysSlipValue, Time.deltaTime * 1);

            for (int i = 0; i < AllWheelColliders.Count; i++)
            {

                AllWheelColliders[i].sidewaysFriction = sidewaysFrictionCurve;
                AllWheelColliders[i].forwardFriction = forwardFrictionCurve;

            }
        }

	}
		
	void Bools(){
		
		//Input For MotorInput.
		motorInput = Input.GetAxis("Vertical");
		
		//Input For SteerInput.
		steerInput = Input.GetAxis("Horizontal");
		
		//Reversing Bool.
		if(motorInput < 0  && AllWheelColliders[0].rpm < 50)
			reversing = true;
		else reversing = false;
		
	}
		
	void Engine(){

		//Speed Limiter.
		if(Speed > maxSpeed){
			
			for(int i = 0; i < AllWheelColliders.Count; i++){
				if(rigidbody.velocity.magnitude > maxSpeed)
					AllWheelColliders[i].motorTorque = 0;
			}
			
		}else{
			
			for(int i = 0; i < AllWheelColliders.Count; i++){
				
				if(!reversing){
					AllWheelColliders[i].motorTorque = EngineTorque  * Mathf.Clamp(motorInput, 0f, 1f) * EngineTorqueCurve.Evaluate(Speed);
				}else{
					if(Speed < 30){
						AllWheelColliders[i].motorTorque = (EngineTorque  * motorInput) / 1;
					}else{
						AllWheelColliders[i].motorTorque = 0;
					}
				}
				
			}
			
		}

		if(!reversing){
			if(WheelColliders_L[0].isGrounded || WheelColliders_R[0].isGrounded)
				if(Mathf.Abs(rigidbody.angularVelocity.y) < .5f)
					rigidbody.AddRelativeTorque((Vector3.up * steerInput) * SteerTorque, ForceMode.Acceleration);
		}else{
			if(WheelColliders_L[0].isGrounded || WheelColliders_R[0].isGrounded)
				if(Mathf.Abs(rigidbody.angularVelocity.y) < .5f)
					rigidbody.AddRelativeTorque((-Vector3.up * steerInput) * SteerTorque, ForceMode.Acceleration);
		}
			
	}

	void ShiftGears(){
		
		for(int i = 0; i < EngineTorqueCurve.length; i++){
			
			if(EngineTorqueCurve.Evaluate(Speed) < EngineTorqueCurve.keys[i].value)
				CurrentGear = i;
			
		}
		
	}
		
	void AnimateGears(){
			
			for(int i = 0; i < UselessGearTransform_R.Length; i++){
				UselessGearTransform_R[i].transform.rotation = WheelColliders_R[i].transform.rotation * Quaternion.Euler( RotationValueR[Mathf.CeilToInt((WheelColliders_R.Length) / 2)], WheelColliders_R[i].steerAngle, 0);
			}
			
			for(int i = 0; i < UselessGearTransform_L.Length; i++){
				UselessGearTransform_L[i].transform.rotation = WheelColliders_L[i].transform.rotation * Quaternion.Euler( RotationValueL[Mathf.CeilToInt((WheelColliders_L.Length) / 2)], WheelColliders_L[i].steerAngle, 0);
			}
			
	}

	void  WheelAlign (){
        if (m_PhotonView.isMine)
        {
            RaycastHit hit;
            WheelHit CorrespondingGroundHit;


            //Right Wheels Transform.
            for (int k = 0; k < WheelColliders_R.Length; k++)
            {

                Vector3 ColliderCenterPoint = WheelColliders_R[k].transform.TransformPoint(WheelColliders_R[k].center);

                if (Physics.Raycast(ColliderCenterPoint, -WheelColliders_R[k].transform.up, out hit, (WheelColliders_R[k].suspensionDistance + WheelColliders_R[k].radius) * transform.localScale.y))
                {
                    WheelTransform_R[k].transform.position = hit.point + (WheelColliders_R[k].transform.up * WheelColliders_R[k].radius) * transform.localScale.y;
                    TrackBoneTransform_R[k].transform.position = hit.point + (WheelColliders_R[k].transform.up * trackOffset) * transform.localScale.y;
                }
                else
                {
                    WheelTransform_R[k].transform.position = ColliderCenterPoint - (WheelColliders_R[k].transform.up * WheelColliders_R[k].suspensionDistance) * transform.localScale.y;
                    TrackBoneTransform_R[k].transform.position = ColliderCenterPoint - (WheelColliders_R[k].transform.up * (WheelColliders_R[k].suspensionDistance + WheelColliders_R[k].radius - trackOffset)) * transform.localScale.y;
                }

                WheelTransform_R[k].transform.rotation = WheelColliders_R[k].transform.rotation * Quaternion.Euler(RotationValueR[Mathf.CeilToInt((WheelColliders_R.Length) / 2)], 0, 0);
                RotationValueR[k] += WheelColliders_R[k].rpm * (6) * Time.deltaTime;
                WheelColliders_R[k].GetGroundHit(out CorrespondingGroundHit);

            }

            //Left Wheels Transform.
            for (int i = 0; i < WheelColliders_L.Length; i++)
            {

                Vector3 ColliderCenterPoint = WheelColliders_L[i].transform.TransformPoint(WheelColliders_L[i].center);

                if (Physics.Raycast(ColliderCenterPoint, -WheelColliders_L[i].transform.up, out hit, (WheelColliders_L[i].suspensionDistance + WheelColliders_L[i].radius) * transform.localScale.y))
                {
                    WheelTransform_L[i].transform.position = hit.point + (WheelColliders_L[i].transform.up * WheelColliders_L[i].radius) * transform.localScale.y;
                    TrackBoneTransform_L[i].transform.position = hit.point + (WheelColliders_L[i].transform.up * trackOffset) * transform.localScale.y;
                }
                else
                {
                    WheelTransform_L[i].transform.position = ColliderCenterPoint - (WheelColliders_L[i].transform.up * WheelColliders_L[i].suspensionDistance) * transform.localScale.y;
                    TrackBoneTransform_L[i].transform.position = ColliderCenterPoint - (WheelColliders_L[i].transform.up * (WheelColliders_L[i].suspensionDistance + WheelColliders_L[i].radius - trackOffset)) * transform.localScale.y;
                }

                WheelTransform_L[i].transform.rotation = WheelColliders_L[i].transform.rotation * Quaternion.Euler(RotationValueL[Mathf.CeilToInt((WheelColliders_L.Length) / 2)], 0, 0);
                RotationValueL[i] += WheelColliders_L[i].rpm * (6) * Time.deltaTime;
                WheelColliders_L[i].GetGroundHit(out CorrespondingGroundHit);

            }


            LeftTrackMesh.renderer.material.SetTextureOffset("_MainTex", new Vector2((RotationValueL[Mathf.CeilToInt((WheelColliders_L.Length) / 2)] / 1000) * trackScrollSpeedMultiplier, 0));
            RightTrackMesh.renderer.material.SetTextureOffset("_MainTex", new Vector2((RotationValueR[Mathf.CeilToInt((WheelColliders_R.Length) / 2)] / 1000) * trackScrollSpeedMultiplier, 0));
            LeftTrackMesh.renderer.material.SetTextureOffset("_BumpMap", new Vector2((RotationValueL[Mathf.CeilToInt((WheelColliders_L.Length) / 2)] / 1000) * trackScrollSpeedMultiplier, 0));
            RightTrackMesh.renderer.material.SetTextureOffset("_BumpMap", new Vector2((RotationValueR[Mathf.CeilToInt((WheelColliders_R.Length) / 2)] / 1000) * trackScrollSpeedMultiplier, 0));
        }	
	}


	void OnCollisionEnter( Collision collision ){
		
		
		if (collision.contacts.Length > 0){	
			
			if(collision.relativeVelocity.magnitude > 10 && crashClips.Length > 0){
				if (collision.contacts[0].thisCollider.gameObject.layer != LayerMask.NameToLayer("Wheel") && collision.transform.gameObject.layer != LayerMask.NameToLayer("Bullet")){
					
					crashAudio = new GameObject("CrashSound");
					crashAudio.transform.position = transform.position;
					crashAudio.transform.rotation = transform.rotation;
					crashAudio.transform.parent = transform;
					crashAudio.AddComponent<AudioSource>();
					crashAudio.audio.minDistance = 10;
					crashAudio.audio.volume = 1;
					
					crashAudio.audio.clip = crashClips[UnityEngine.Random.Range(0, crashClips.Length)];
					crashAudio.audio.pitch = UnityEngine.Random.Range (1f, 1.2f);
					crashAudio.audio.Play ();

					Destroy(crashAudio, crashAudio.audio.clip.length);
					
				}
			}
			
		}
		
	}
		
	void SmokeInstantiateRate () {


		if ( WheelParticles.Count > 0 ) {

			for(int i = 0; i < AllWheelColliders.Count; i++){

				WheelHit CorrespondingGroundHit;
				AllWheelColliders[i].GetGroundHit( out CorrespondingGroundHit );

				if(Mathf.Abs(CorrespondingGroundHit.sidewaysSlip) > .5f || Mathf.Abs(CorrespondingGroundHit.forwardSlip) > 1f ) 
					WheelParticles[i].particleEmitter.emit = true;
				else WheelParticles[i].particleEmitter.emit = false;

			}
			
		}
			
		if(normalExhaustGas){
			if(Speed < 15)
				normalExhaustGas.emit = true;
			else normalExhaustGas.emit = false;
		}
		
		if(heavyExhaustGas){
			if(motorInput > 0)
				heavyExhaustGas.emit = true;
			else heavyExhaustGas.emit = false;
		}
		
	}
		
	}