using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorState : MonoBehaviour
{

    private const int CHARACTER_LAYER = 7;

    [SerializeField] public Boolean isDoorPickLocking = new Boolean(false);
    [SerializeField] private Boolean doorLocked = new Boolean(true);
    [SerializeField] private Boolean doorClosed = new Boolean(true);
    [SerializeField] private Boolean doorLockedByKey = new Boolean(false);

    [SerializeField] private bool doorOpenedDirection1 = false;
    [SerializeField] private bool doorOpenedDirection2 = false;

    private Animator doorAnimator;

    [SerializeField] private BoxCollider triggerStopDoorDirection1;
    [SerializeField] private BoxCollider triggerStopDoorDirection2;


    [SerializeField] private int doorTimeOut = 5; // stabilisce il valore di time out dopo cui la porta si chiude automaticamente
    [SerializeField] private int _doorLockPickTime = 5; // stabilisce il valore di tempo che bisogna aspettare durante lo scassinamento

    [Header("door sound refs")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openDoorClip;
    [SerializeField] private AudioClip closeDoorClip;

    public int doorLockPickTime {
        get { return _doorLockPickTime; }
    }

    // getter 
    public int getDoorTimeOut() {
        return doorTimeOut;
    }

    public Boolean getDoorClosed() {
        return doorClosed;
    }

    public void Start() {
        doorAnimator = gameObject.GetComponent<Animator>();
    }

    public void openDoorDirection1() {
        resetDoor();

        doorAnimator.SetTrigger("openDirection1");

        if (doorAnimator.speed == 0) {
            doorAnimator.speed = 1;
        }

        doorOpenedDirection1 = true;
        doorOpenedDirection2 = false;

        if(triggerStopDoorDirection2 != null) {
            triggerStopDoorDirection2.enabled = true;
        }
        if (triggerStopDoorDirection1 != null) {
            triggerStopDoorDirection1.enabled = false;
        }

        


        
    }

    public void openDoorDirection2() {
        resetDoor();

        doorAnimator.SetTrigger("openDirection2");

        if(doorAnimator.speed == 0) {
            doorAnimator.speed = 1;
        }

        doorOpenedDirection2 = true;
        doorOpenedDirection1 = false;

        if (triggerStopDoorDirection2 != null) {
            triggerStopDoorDirection2.enabled = false;
        }
        if (triggerStopDoorDirection1 != null) {
            triggerStopDoorDirection1.enabled = true;
        }
        
        


    }

    //setter
    public void setDoorLocked(bool lockedState) {
        doorLocked.value = lockedState;
    }

    public void closeDoor(bool closeState) {
        doorClosed.value = closeState;

        if(doorClosed.value) {

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
            

            
            

            if (doorClosed.value == true) {
                
                doorAnimator.SetTrigger("close");
            }
                
        }
    }


    // getter
    public Boolean isDoorLocked() {
        return doorLocked;
    }

    public Boolean isDoorClosed() {
        return doorClosed;
    }

    public Boolean isDoorLockedByKey() {
        return doorLockedByKey;
    }


    /// <summary>
    /// Gestisce il bloccaggio della porta in caso di collisioni con il character
    /// Caso entrata collisione
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter(Collider collision) {
        if (collision.gameObject.layer == CHARACTER_LAYER) {
            
            if(!collision.gameObject.GetComponent<CharacterManager>().isDead) {
                if (doorAnimator.IsInTransition(0)) {
                    doorAnimator.speed = 0;
                }
            }
        }
    }

    /// <summary>
    /// Gestisce il bloccaggio della porta in caso di collisioni con il character
    /// Caso uscita collisione
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerExit(Collider collision) {
        if (collision.gameObject.layer == CHARACTER_LAYER) {
            doorAnimator.speed = 1;
        }
    }

    // resetta stato animazione
    public void resetDoor() {
        doorAnimator.ResetTrigger("openDirection1");
        doorAnimator.ResetTrigger("openDirection2");
        doorAnimator.ResetTrigger("close");
    }

    private void openDoorSound() {
        audioSource.clip = openDoorClip;
        audioSource.Play();
    }

    private void closeDoorSound() {
        audioSource.clip = closeDoorClip;
        audioSource.Play();
    }
}
