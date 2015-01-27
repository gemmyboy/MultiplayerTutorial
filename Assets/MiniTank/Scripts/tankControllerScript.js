//#pragma strict
    // Variables for caching transform and rigidbody.
    private var myTransform:Transform;
    private var myRigidbody:Rigidbody;
     
    //Variables for checking if the tank is on the ground and not upside down.
    private var myGrounder;
    private var grounded:boolean = false;
     
    //The acceleration we want the tank to have when vertical input is given.
    var acceleration:float = 10;
     
     //Variable for altering the friction force
     var frictionMultiplier:float = 2;
     
    //The angular acceleration we want the tank to have when horizontal input is given.
    var angularAcceleration:float = 10;
     
    //Maximum turning speed.
    var maxTurnSpeed = 1;
     
    //Maximum speed.
    var maxSpeed = 20;
     
    //Angular "drag", acceleration in the opposite direction when tank is turning but input is not given.
    var turnDrag:float = 10;
     
    function Start(){
        //Cache transform and rigidbody and find the grounder component.
        myTransform = transform;
        myGrounder = myTransform.GetComponentInChildren(grounder);
        //myGrounder = myTransform.
        myRigidbody = rigidbody;
    }
     
    function Update () {
        //Find out if the tank is grounded.
        grounded = myGrounder.grounded;
     
        //Get inputs into variables.
        var hInput = Input.GetAxis("Horizontal");
        var vInput = Input.GetAxis("Vertical");
       
        //Variable for checking if tracks have power.
        var neutral:boolean = true;
       
        //The faster the tank moves, the more we want it to slide,
        var traction:float = 1 / myRigidbody.velocity.magnitude;
       
        //Local velocity is needed for calculating the friction force and limiting speed.
        var locVelocity = myTransform.InverseTransformDirection(myRigidbody.velocity);
       
        //The forward direction vector in world coordinates.
        var fwd = myTransform.TransformDirection(Vector3.forward);
       
        //The angle between our velocity vector and forward direction in world coordinates. Used for
        //checking if we are moving forward or in reverse. When the tank is still the angle is 90,
        //we substract it just for clarity.
        var velocityAngle = Vector3.Angle(myRigidbody.velocity, fwd) - 90;
       
        //Apply force if the tank is grounded and not moving at maximum speed. Set neutral to false.
        if(vInput && grounded == true){
            neutral = false;
            if(locVelocity.z < maxSpeed && locVelocity.z > -maxSpeed){
                myRigidbody.AddForce(fwd * -vInput * acceleration * myRigidbody.mass);
            }
        }
       
        //Calculate friction force applied according to the difference between local velocity and forward direciton.
        //The more we are sliding sideways, the more friction force is applied. This is done to simulate the tracks
        // rolling freely when no power is applied to them.
        var frictionForce:float;
        var frictionAngle = Vector3.Angle(locVelocity, -Vector3.forward);
        if (frictionAngle < 90){
            frictionForce = frictionAngle;
        }
        else if (frictionAngle > 90){
            frictionForce = Mathf.Abs(frictionAngle - 180);
        }
       
        //Apply the friction force if we are grounded and moving.
        if (myRigidbody.velocity.magnitude >= 0.5 && grounded == true){
                var d = -myRigidbody.velocity.normalized;
                myRigidbody.AddForce(d * frictionForce * frictionMultiplier);
            }
       
        //Apply torque accoriding to horizontal input when the tank is grounded and not turning at maximum speed. Also set neutral to false
        //because the tracks have power.
        if(hInput && grounded == true){
            neutral = false;
            if (rigidbody.angularVelocity.magnitude < maxTurnSpeed){
                myRigidbody.AddTorque(Vector3.up * hInput * angularAcceleration * myRigidbody.mass);
            }
        }else{
            //If no input is given we should stop turning quit quickly.
            myRigidbody.AddTorque(-myRigidbody.angularVelocity * turnDrag * myRigidbody.mass);
        }
       
        //Changing direction when turning on the move is simulated with Vector3.Slerp. The faster we are moving the less we should slerp in order
        //to simulate traction.
        if(velocityAngle > 0 && neutral == false && grounded == true){
            myRigidbody.velocity = Vector3.Slerp(myRigidbody.velocity, -myTransform.forward * myRigidbody.velocity.magnitude, traction);
        }else if (velocityAngle < 0 && neutral == false && grounded == true){
            myRigidbody.velocity = Vector3.Slerp(myRigidbody.velocity, myTransform.forward * myRigidbody.velocity.magnitude, traction);
        }
    }
