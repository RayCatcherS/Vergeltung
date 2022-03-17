using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class DoorInteractable : Interactable {

    [SerializeField] private bool doorLocked = true;
    [SerializeField] private bool doorClosed = true;

    [SerializeField] string lockPickingEventName = "LOCKPICK DOOR";
    UnityEventCharacter lockPickingEvent = new UnityEventCharacter();

    [SerializeField] string openDoorEventName = "OPEN DOOR";
    UnityEventCharacter openDoorEvent = new UnityEventCharacter();

    [SerializeField] string closeDoorEventName = "CLOSE DOOR";
    UnityEventCharacter closeDoorEvent = new UnityEventCharacter();

    public void Start() {
        


        lockPickingEvent.AddListener(lockPicking);
        openDoorEvent.AddListener(openDoor);
        closeDoorEvent.AddListener(closeDoor);
    }

    public void openDoor(CharacterInteraction p) {

        Debug.Log("Apertura porta");
    }

    public void closeDoor(CharacterInteraction p) {

        Debug.Log("Chiusura porta");
    }

    public void lockPicking(CharacterInteraction p) {

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
