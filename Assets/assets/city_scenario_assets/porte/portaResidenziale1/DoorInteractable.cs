using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class DoorInteractable : Interactable {

    [SerializeField] private bool doorLocked = true;
    [SerializeField] private bool doorClosed = true;

    [SerializeField] string lockPickingEventName = "Use lockpick";
    UnityEventPlayer lockPickingEvent = new UnityEventPlayer();

    [SerializeField] string openDoorEventName = "Open door";
    UnityEventPlayer openDoorEvent = new UnityEventPlayer();

    [SerializeField] string closeDoorEventName = "Close door";
    UnityEventPlayer closeDoorEvent = new UnityEventPlayer();

    public void Start() {
        


        lockPickingEvent.AddListener(openDoor);
    }

    public void openDoor(PlayerInteractionController p) {

        Debug.Log("Apertura porta");
    }

    public void closeDoor(PlayerInteractionController p) {

        Debug.Log("Chiusura porta");
    }

    public void lockPicking(PlayerInteractionController p) {

        Debug.Log("Scassinamento porta");
    }

    /*public IEnumerator openDoor() {

        busyInteractable = true;
        yield return new WaitForSeconds(0);
        busyInteractable = false;
    }

    public IEnumerator closeDoor() {

        busyInteractable = true;
        yield return new WaitForSeconds(0);
        busyInteractable = false;
    }

    public IEnumerator lockPicking() {

        busyInteractable = true;
        yield return new WaitForSeconds(5);
        busyInteractable = false;
    }*/

    public override List<Interaction> getInteractable() {

        List<Interaction> eventRes = new List<Interaction>();

        if (doorLocked) {
            eventRes.Add(
                new Interaction(lockPickingEvent, lockPickingEventName)
            );
        } else {

            if (doorClosed) {
                eventRes.Add(
                    new Interaction(openDoorEvent, openDoorEventName)
                );
            } else {
                eventRes.Add(
                    new Interaction(closeDoorEvent, closeDoorEventName)
                );
            }
        }

        return eventRes;
    }
}
