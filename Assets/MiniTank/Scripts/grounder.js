    var grounded:boolean = false;
     
    function OnTriggerEnter(other:Collider){
        if (other.name == "Terrain"){
            grounded = true;
        }
    }
     
    function OnTriggerExit(other:Collider){
        if (other.name == "Terrain"){
            grounded = false;
        }
    }
