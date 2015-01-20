using UnityEngine;
using System.Collections;

public class Player : Photon.MonoBehaviour
{
    //Player Variables
    public float speed = 10f;
    public float turningSpeed = 30.0f;
    public float jumpHeight = 300.0f;
    //NETWORKING
    private float lastSynchronizationTime = 0f;
    private float syncDelay = 0f;
    private float syncTime = 0f;
    private Vector3 syncStartPosition = Vector3.zero;
    private Vector3 syncEndPosition = Vector3.zero;

    private Quaternion syncStartRotation = Quaternion.identity;
    private Quaternion syncEndRotation = Quaternion.identity;

    public GameObject gameCamera;
    public GameObject bullet;
    public GameObject gunTip;

    public bool changingColor = false;
    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        Vector3 syncPosition = Vector3.zero;
        Vector3 syncVelocity = Vector3.zero;
        Quaternion syncRotation = Quaternion.identity;
        if (stream.isWriting)
        {
            syncPosition = rigidbody.position;
            stream.Serialize(ref syncPosition);

            syncPosition = rigidbody.velocity;
            stream.Serialize(ref syncVelocity);

            //Quaternion rot = transform.rotation;
            //stream.Serialize(ref rot);

            syncRotation = rigidbody.rotation;
            stream.Serialize(ref syncRotation);
        }
        else
        {
            stream.Serialize(ref syncPosition);
            stream.Serialize(ref syncVelocity);
            stream.Serialize(ref syncRotation);

            syncTime = 0f;
            syncDelay = Time.time - lastSynchronizationTime;
            lastSynchronizationTime = Time.time;

            syncEndPosition = syncPosition + syncVelocity * syncDelay;
            syncStartPosition = rigidbody.position;

            syncStartRotation = rigidbody.rotation;
            syncEndRotation = syncRotation;
        }
    }

    void Awake()
    {
        lastSynchronizationTime = Time.time;
        if (photonView.isMine)
        {
            gameCamera.SetActive(true);
        }else{
            gameCamera.SetActive(false);
        }
    }

    void Update()
    {
        if (photonView.isMine)
        {
            InputMovement();
            InputColorChange();
        }
        else
        {
            SyncedMovement();
        }
    }


    private void InputMovement()
    {
        //if (Input.GetKey(KeyCode.W))
        //    rigidbody.MovePosition(rigidbody.position + Vector3.forward * speed * Time.deltaTime);

        //if (Input.GetKey(KeyCode.S))
        //    rigidbody.MovePosition(rigidbody.position - Vector3.forward * speed * Time.deltaTime);

        //if (Input.GetKey(KeyCode.D))
        //    rigidbody.MovePosition(rigidbody.position + Vector3.right * speed * Time.deltaTime);

        //if (Input.GetKey(KeyCode.A))
        //    rigidbody.MovePosition(rigidbody.position - Vector3.right * speed * Time.deltaTime);
        float horizontal = Input.GetAxis("Horizontal") * turningSpeed * Time.deltaTime;
        transform.Rotate(0, horizontal, 0);

        float vertical = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        transform.Translate(0, 0, vertical);
        if(Input.GetKeyDown(KeyCode.Space)){
            rigidbody.AddForce(Vector3.up * jumpHeight * Time.deltaTime,ForceMode.VelocityChange);
        }
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            //Rigidbody instantiatedProjectile = PhotonNetwork.Instantiate(bullet, gunTip.transform.position, gunTip.transform.rotation,0) as Rigidbody;
        }
    }

    private void SyncedMovement()
    {
        syncTime += Time.deltaTime;

        rigidbody.position = Vector3.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay);
        rigidbody.rotation = Quaternion.Lerp(syncStartRotation, syncEndRotation, syncTime / syncDelay);
    }

    private void InputColorChange()
    {
        if (Input.GetKeyDown(KeyCode.R) && !changingColor)
        {
            Debug.Log("ChangingMenu");
            ChangeColorTo(new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));
        }
    }

    [RPC] void ChangeColorTo(Vector3 color)
    {
        Color newColor = new Color(color.x,color.y,color.z,1);
        changingColor = true;
        StartCoroutine(LerpColor(newColor));

        if (photonView.isMine)
            photonView.RPC("ChangeColorTo", PhotonTargets.OthersBuffered, color);
    }

    //TIME VARIABLES FOR LERPING
    float duration = 1.5f; // This will be your time in seconds.
    float smoothness = 0.09f; // This will determine the smoothness of the lerp. Smaller values are smoother. Really it's the time between updates.
    Color currentColor;
    //--------------------------------------------------------------------------------------------------------------------------------------------
    IEnumerator LerpColor(Color newColor)
    {
        currentColor = renderer.material.GetColor("_Color");
        float progress = 0; //This float will serve as the 3rd parameter of the lerp function.
        float increment = smoothness / duration; //The amount of change to apply.
        while (progress < 1)
        {
            currentColor = Color.Lerp(currentColor, newColor, progress);
            progress += increment;
            renderer.material.color = currentColor;
            yield return new WaitForSeconds(smoothness);
        }
        changingColor = false;
        yield return true;
    }
}
