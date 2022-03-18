using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorState : MonoBehaviour
{

    private const int CHARACTER_LAYER = 7;


    [SerializeField] private bool doorLocked = true;
    [SerializeField] private bool doorClosed = true;

    [SerializeField] private bool doorOpenedDirection1 = false;
    [SerializeField] private bool doorOpenedDirection2 = false;

    private Animator doorAnimator;

    [SerializeField] private BoxCollider triggerStopDoorDirection1;
    [SerializeField] private BoxCollider triggerStopDoorDirection2;


    public void Start() {
        doorAnimator = gameObject.GetComponent<Animator>();
    }

    public void openDoorDirection1() {


        doorAnimator.SetTrigger("openDirection1");

        if (doorAnimator.speed == 0) {
            doorAnimator.speed = 1;
        }

        doorOpenedDirection1 = true;
        doorOpenedDirection2 = false;

        triggerStopDoorDirection2.enabled = true;
        triggerStopDoorDirection1.enabled = false;
    }

    public void openDoorDirection2() {

        doorAnimator.SetTrigger("openDirection2");

        if(doorAnimator.speed == 0) {
            doorAnimator.speed = 1;
        }

        doorOpenedDirection2 = true;
        doorOpenedDirection1 = false;

        triggerStopDoorDirection1.enabled = true;
        triggerStopDoorDirection2.enabled = false;
    }

    //setter
    public void setDoorLocked(bool lockedState) {
        doorLocked = lockedState;
    }

    public void setDoorClosed(bool closeState) {
        doorClosed = closeState;

        if(doorClosed) {

            if(doorOpenedDirection1) {
                doorAnimator.speed = 1;
                triggerStopDoorDirection1.enabled = true;
                triggerStopDoorDirection2.enabled = false;
            }

            if (doorOpenedDirection2) {
                doorAnimator.speed = 1;
                triggerStopDoorDirection2.enabled = true;
                triggerStopDoorDirection1.enabled = false;
            }
            

            
            

            if (doorClosed == true) {
                
                doorAnimator.SetTrigger("close");
            }
                
        }
    }


    // getter
    public bool isDoorLocked() {
        return doorLocked;
    }

    public bool isDoorClosed() {
        return doorClosed;
    }


    private void OnTriggerEnter(Collider collision) {
        if (collision.gameObject.layer == CHARACTER_LAYER) {
            Debug.Log("Character collision");

            if (doorAnimator.IsInTransition(0)) {
                doorAnimator.speed = 0;
            }
        }
    }


    private void OnTriggerExit(Collider collision) {
        if (collision.gameObject.layer == CHARACTER_LAYER) {
            Debug.Log("Character collision exit");
            doorAnimator.speed = 1;
        }
    }
}
