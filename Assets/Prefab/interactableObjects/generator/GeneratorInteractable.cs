using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class GeneratorInteractable : Interactable {

    [Header("References")]
    [SerializeField] private ScenePowerController scenePowerController; // game state per accedere ai metodi dello stato di gioco
    [SerializeField] private AudioSource audioSource;

    [Header("State")]
    [SerializeField] string sabotageGeneratorEventName = "SABOTAGE GENERATOR";
    [SerializeField] UnityEventCharacter sabotageGenerator = new UnityEventCharacter();
    [SerializeField] private GeneratorState generatorState = GeneratorState.GeneratorOn;
    private bool isSabotage = false;

    [Header("generator config")]
    [SerializeField] private float sabotageTime = 2f; // tempo per sabotare il generatore

    [Header("Asset Refs")]
    [SerializeField] private AudioClip interactAudioClip;



    public override void Start() {
        initInteractable();

        sabotageGenerator.AddListener(switchOffGenerator);

        if(generatorState == GeneratorState.GeneratorOn) {
            interactableMeshEffectSetEnebled(true);
        }
    }

    private async void switchOffGenerator(CharacterManager characterWhoIsInteracting) {

        isSabotage = true;
        characterWhoIsInteracting.isSuspiciousGenericAction = true; // permette al player di diventare sospetto/ostile
        characterWhoIsInteracting.alarmAlertUIController.potentialSuspiciousGenericActionAlarmOn(); // avvia potenziale stato alert

        // sound 
        playSounds();

        // avvia task sul character che ha avviato il task
        bool playerTaskResultDone = await characterWhoIsInteracting.startTimedInteraction(sabotageTime, "Sabotage");

        characterWhoIsInteracting.isSuspiciousGenericAction = false;

        isSabotage = false;
        if (playerTaskResultDone) {
            generatorState = GeneratorState.GeneratorOff;
            scenePowerController.turnOffPower();
            interactableMeshEffectSetEnebled(false);
        }

        characterWhoIsInteracting.alarmAlertUIController.potentialSuspiciousGenericActionAlarmOff();
        characterWhoIsInteracting.buildListOfInteraction(); // rebuilda UI

    }

    /// <summary>
    /// Metodo per rendere generatore disattivabile(sabotaggio)
    /// </summary>
    public void switchOnGenerator() {
        generatorState = GeneratorState.GeneratorOn;
        interactableMeshEffectSetEnebled(true);
    }

    public override Interaction getMainInteraction() {
        return new Interaction(sabotageGenerator, sabotageGeneratorEventName, this);
    }

    public override List<Interaction> getInteractions(CharacterManager character = null) {

        List<Interaction> eventRes = new List<Interaction>();

        if(generatorState == GeneratorState.GeneratorOn && scenePowerController.getPowerOn() && !isSabotage) {
            eventRes.Add(new Interaction(sabotageGenerator, sabotageGeneratorEventName, this));
        }

        return eventRes;
    }

    private void playSounds() {
        audioSource.clip = interactAudioClip;
        audioSource.Play();
    }
}

public enum GeneratorState {
    GeneratorOn,
    GeneratorOff,
}