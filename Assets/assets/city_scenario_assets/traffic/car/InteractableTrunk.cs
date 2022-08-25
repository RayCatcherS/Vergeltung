using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableTrunk : Interactable {
    [SerializeField] private bool trunkOpened = false;
    private bool isPickLock = false;

    [Header("References")]
    [SerializeField] private AudioSource trunkAudioSource;
    [SerializeField] private AudioClip openTrunkClip;
    [SerializeField] private Animator trunkAnimator;

    [Header("Event conf")]
    [SerializeField] string trunkEventName = "LOCKPICK TRUNK";
    [SerializeField] UnityEventCharacter trunkLockPick = new UnityEventCharacter();

    [Header("trunk lockpick config")]
    [SerializeField] private float lockpickTime = 2f; // tempo per scassinare baule auto

    [Header("Spawn on open trunk")]
    [SerializeField] Transform spawnTransform;
    [SerializeField] private GameObject goToSpawn;

    public override void Start() {
        base.Start();

        trunkLockPick.AddListener(lockpickTruck);

    }

    private async void lockpickTruck(CharacterManager characterWhoIsInteracting) {
        isPickLock = true;

        characterWhoIsInteracting.isSuspiciousGenericAction = true; // permette al player di diventare sospetto/ostile
        characterWhoIsInteracting.alarmAlertUIController.potentialSuspiciousGenericActionAlarmOn(); // avvia UI potenziale stato alert


        // avvia task sul character che ha avviato il task
        bool playerTaskResultDone = await characterWhoIsInteracting.startTimedInteraction(lockpickTime, "lockpicking");


        characterWhoIsInteracting.isSuspiciousGenericAction = false;
        characterWhoIsInteracting.alarmAlertUIController.potentialSuspiciousGenericActionAlarmOff();


        isPickLock = false;

        if(playerTaskResultDone) {
            trunkOpened = true;
            interactableMeshEffectSetEnebled(false);
            playSound();
            openTrunk();
            spawnOBJ();
            unFocusInteractableOutline();
        }

        characterWhoIsInteracting.buildListOfInteraction(); // rebuilda UI
    }

    private void playSound() {

    }

    private void openTrunk() {
        trunkAnimator.SetTrigger("open");
    }

    private void spawnOBJ() {
        GameObject obj;
        obj = Instantiate(goToSpawn, spawnTransform.position, spawnTransform.rotation);
    }

    public override List<Interaction> getInteractions(CharacterManager character = null) {

        List<Interaction> eventRes = new List<Interaction>();

        if(!trunkOpened) {
            eventRes.Add(new Interaction(trunkLockPick, trunkEventName, this));
        }

        return eventRes;
    }
}

