using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class DoorInteractable : Interactable {

    [SerializeField] DoorState doorState;

    [SerializeField] string lockPickingEventName = "LOCKPICK DOOR";
    [SerializeField] UnityEventCharacter lockPickingEvent = new UnityEventCharacter();

    [SerializeField] string openDoorEventName = "OPEN DOOR";
    [SerializeField] UnityEventCharacter openDoorEvent = new UnityEventCharacter();

    [SerializeField] string closeDoorEventName = "CLOSE DOOR";
    [SerializeField] UnityEventCharacter closeDoorEvent = new UnityEventCharacter();


    public void Start() {

        lockPickingEvent.AddListener(lockPicking);
        openDoorEvent.AddListener(openDoor);
        closeDoorEvent.AddListener(closeDoor);
    }

    public void openDoor(CharacterInteraction characterInteraction) {

        doorState.setDoorClosed(false);


        // avvia chiusura timeout
        StartCoroutine(doorClosingTimeOut(characterInteraction));
    }

    public void closeDoor(CharacterInteraction characterInteraction) {

        doorState.setDoorClosed(true);

    }

    public void lockPicking(CharacterInteraction characterInteraction) {
        doorState.setDoorLocked(false);
    }


    public override List<Interaction> getInteractable() {

        List<Interaction> eventRes = new List<Interaction>();

        if (doorState.isDoorLocked()) {
            eventRes.Add(
                new Interaction(lockPickingEvent, lockPickingEventName, this)
            );
        } else {

            if (doorState.isDoorClosed()) {
                eventRes.Add(
                    new Interaction(openDoorEvent, openDoorEventName, this)
                );
            } else {
                eventRes.Add(
                    new Interaction(closeDoorEvent, closeDoorEventName, this)
                );
            }
        }

        return eventRes;
    }

    IEnumerator doorClosingTimeOut(CharacterInteraction characterInteraction) {
        yield return new WaitForSeconds(doorState.getDoorTimeOut());

        if (!doorState.getDoorClosed()) {
            doorState.setDoorClosed(true); // chiudi porta

            characterInteraction.buildListOfInteraction();
        }


    }


}
