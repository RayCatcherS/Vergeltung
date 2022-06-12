using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class GeneratorInteractable : Interactable {

    [Header("References")]
    [SerializeField] private GameState gameState; // game state per accedere ai metodi dello stato di gioco

    
    [SerializeField] string sabotageGeneratorEventName = "SABOTAGE GENERATOR";
    [SerializeField] UnityEventCharacter sabotageGenerator = new UnityEventCharacter();
    [SerializeField] private GeneratorState generatorState = GeneratorState.GeneratorOn;

    [Header("generator config")]
    [SerializeField] private float sabotageTime = 2f; // tempo per sabotare il generatore


    public override void Start() {
        initInteractable();

        sabotageGenerator.AddListener(switchOffGenerator);
    }

    private async void switchOffGenerator(CharacterManager characterWhoIsInteracting) {

        characterWhoIsInteracting.alarmAlertUIController.potentialLockPickingAlarmOn(); // avvia potenziale stato alert
        // avvia task sul character che ha avviato il task
        bool playerTaskResultDone = await characterWhoIsInteracting.startTimedInteraction(sabotageTime, "Sabotage");

        if(playerTaskResultDone) {
            generatorState = GeneratorState.GeneratorOff;
            gameState.turnOffPower();
        }

        characterWhoIsInteracting.alarmAlertUIController.potentialLockPickingAlarmOff();
        characterWhoIsInteracting.buildListOfInteraction(); // rebuilda UI

    }

    public override Interaction getMainInteraction() {
        return new Interaction(sabotageGenerator, sabotageGeneratorEventName, this);
    }

    public override List<Interaction> getInteractions() {

        List<Interaction> eventRes = new List<Interaction>();

        if(generatorState == GeneratorState.GeneratorOn && gameState.getPowerOn()) {
            eventRes.Add(new Interaction(sabotageGenerator, sabotageGeneratorEventName, this));
        }

        return eventRes;
    }
}

public enum GeneratorState {
    GeneratorOn,
    GeneratorOff,
}