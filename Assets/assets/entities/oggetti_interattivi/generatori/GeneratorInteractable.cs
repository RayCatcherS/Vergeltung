using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class GeneratorInteractable : Interactable {

    [SerializeField] private GameState gameState; // game state per accedere ai metodi dello stato di gioco

    [SerializeField] string lockPickingEventName = "SABOTAGE GENERATOR";
    [SerializeField] UnityEventCharacter sabotageGenerator = new UnityEventCharacter();
    [SerializeField] private GeneratorState generatorState = GeneratorState.GeneratorOn;
    

    public void Start() {
        sabotageGenerator.AddListener(switchOffGenerator);
    }

    void switchOffGenerator(CharacterInteraction p) {
        generatorState = GeneratorState.GeneratorOff;
        gameState.turnOffPower();

    }

    public override List<Interaction> getInteractable() {

        List<Interaction> eventRes = new List<Interaction>();

        if(generatorState == GeneratorState.GeneratorOn && gameState.getPowerOn()) {
            eventRes.Add(new Interaction(sabotageGenerator, lockPickingEventName, this));
        }

        return eventRes;
    }
}

public enum GeneratorState {
    GeneratorOn,
    GeneratorOff,
}