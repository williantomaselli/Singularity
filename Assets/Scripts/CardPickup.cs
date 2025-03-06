using UnityEngine;

public class CardPickup : MonoBehaviour
{
    public GameObject doorObj;
    public DoorController doorController;

    public void Start(){
        doorController = doorObj.GetComponent<DoorController>(); 
    }
    void OnTriggerEnter(Collider other){
        if(other.CompareTag("Player")){
            doorController.haveCard = true;
        }
    }

}
