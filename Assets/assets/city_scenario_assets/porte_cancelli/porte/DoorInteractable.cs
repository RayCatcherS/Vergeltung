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

    [SerializeField] private Boolean internalOpeningSide = new Boolean(false);


    public override void Start() {
        initInteractable();

        lockPickingEvent.AddListener(lockPick);
        openDoorEvent.AddListener(openDoor);
        closeDoorEvent.AddListener(closeDoor);

        doorState.isDoorPickLocking.value = false;
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

    private async void lockPick(CharacterManager characterWhoIsInteracting) {

        doorState.isDoorPickLocking.value = true; // setta lo stato della porta in "PickLocking"


        

        characterWhoIsInteracting.isSuspiciousGenericAction = true; // permette al player di diventare sospetto/ostile
        characterWhoIsInteracting.alarmAlertUIController.potentialSuspiciousGenericActionAlarmOn(); // avvia potenziale stato alert 

        // avvia task sul character che ha avviato il task
        bool playerTaskResultDone = await characterWhoIsInteracting.startTimedInteraction(doorState.doorLockPickTime, "Lock-Picking", doorState.isDoorClosed(), false);
        characterWhoIsInteracting.isSuspiciousGenericAction = false;

        if (playerTaskResultDone) { // sblocca la porta se il task è stato portato a termine
            doorState.setDoorLocked(false);
        }



        characterWhoIsInteracting.alarmAlertUIController.potentialSuspiciousGenericActionAlarmOff();
        doorState.isDoorPickLocking.value = false; // disattiva stato della porta in "PickLocking"
        characterWhoIsInteracting.buildListOfInteraction(); // rebuilda UI
    }


    public override List<Interaction> getInteractions(CharacterManager character) {

        List<Interaction> eventRes = new List<Interaction>();


        if(internalOpeningSide.value || character.chracterRole == Role.EnemyGuard) { // se la direzione della porta interna è apribile

            if (doorState.isDoorClosed().value) {
                eventRes.Add(
                    new Interaction(openDoorEvent, openDoorEventName, this)
                );
            } else {
                eventRes.Add(
                    new Interaction(closeDoorEvent, closeDoorEventName, this)
                );
            }

        } else {
            if (doorState.isDoorLockedByKey().value) {

            } else {

                if (!doorState.isDoorPickLocking.value) {
                    if (doorState.isDoorLocked().value) {

                        if (!doorState.isDoorClosed().value) {
                            eventRes.Add(
                                new Interaction(closeDoorEvent, closeDoorEventName, this)
                            );
                        } else {
                            eventRes.Add(
                                new Interaction(lockPickingEvent, lockPickingEventName, this)
                            );
                        }

                    } else {

                        if (doorState.isDoorClosed().value) {
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
            }
        }

        

        
        

        return eventRes;
    }

    IEnumerator doorClosingTimeOut(CharacterManager characterInteraction) {
        yield return new WaitForSeconds(doorState.getDoorTimeOut());

        if (!doorState.getDoorClosed().value) {
            doorState.closeDoor(true); // chiudi porta

            characterInteraction.buildListOfInteraction();
        }


    }


}
