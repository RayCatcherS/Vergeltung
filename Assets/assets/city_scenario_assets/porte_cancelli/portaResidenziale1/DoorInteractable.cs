using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class DoorInteractable : Interactable {

    [SerializeField] public DoorState doorState;

    [SerializeField] string lockPickingEventName = "LOCKPICK DOOR";
    [SerializeField] UnityEventCharacter lockPickingEvent = new UnityEventCharacter();

    [SerializeField] string openDoorEventName = "OPEN DOOR";
    [SerializeField] public UnityEventCharacter openDoorEvent = new UnityEventCharacter();

    [SerializeField] string closeDoorEventName = "CLOSE DOOR";
    [SerializeField] UnityEventCharacter closeDoorEvent = new UnityEventCharacter();


    public override void Start() {
        initInteractable();

        lockPickingEvent.AddListener(lockPick);
        openDoorEvent.AddListener(openDoor);
        closeDoorEvent.AddListener(closeDoor);

        doorState.isDoorPickLocking = false;
    }

    public void openDoor(CharacterManager characterInteraction) {
        StopAllCoroutines(); // rimuovi coroutine "penzolanti" (dangling)
        doorState.closeDoor(false);


        // avvia chiusura timeout
        StartCoroutine(doorClosingTimeOut(characterInteraction));
    }

    public void closeDoor(CharacterManager characterInteraction) {

        doorState.closeDoor(true);

    }

    public async void lockPick(CharacterManager characterWhoIsInteracting) {

        doorState.isDoorPickLocking = true; // setta lo stato della porta in "PickLocking"
        characterWhoIsInteracting.buildListOfInteraction(); // rebuilda UI


        

        characterWhoIsInteracting.isPickLocking = true; // permette al player di diventare sospetto/ostile
        characterWhoIsInteracting.alarmAlertUIController.potentialLockPickingAlarmOn(); // avvia alert 

        // avvia task sul character che ha avviato il task
        bool playerTaskResultDone = await characterWhoIsInteracting.startTimedInteraction(doorState.doorLockPickTime, "Lock-Picking");
        characterWhoIsInteracting.isPickLocking = false;

        if (playerTaskResultDone) { // sblocca la porta se il task è stato portato a termine
            doorState.setDoorLocked(false);
        }



        characterWhoIsInteracting.alarmAlertUIController.potentialLockPickingAlarmOff();
        doorState.isDoorPickLocking = false; // disattiva stato della porta in "PickLocking"
        characterWhoIsInteracting.buildListOfInteraction(); // rebuilda UI
    }


    public override List<Interaction> getInteractions() {

        List<Interaction> eventRes = new List<Interaction>();


        if(!doorState.isDoorPickLocking) {
            if (doorState.isDoorLocked()) {

                if(!doorState.isDoorClosed()) {
                    eventRes.Add(
                        new Interaction(closeDoorEvent, closeDoorEventName, this)
                    );
                } else {
                    eventRes.Add(
                        new Interaction(lockPickingEvent, lockPickingEventName, this)
                    );
                }
                
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
        }
        

        return eventRes;
    }

    IEnumerator doorClosingTimeOut(CharacterManager characterInteraction) {
        yield return new WaitForSeconds(doorState.getDoorTimeOut());

        if (!doorState.getDoorClosed()) {
            doorState.closeDoor(true); // chiudi porta

            characterInteraction.buildListOfInteraction();
        }


    }


}
