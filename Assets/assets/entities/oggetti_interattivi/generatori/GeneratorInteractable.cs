using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class GeneratorInteractable : Interactable {

    [SerializeField] private GameState gameState; // game state per accedere ai metodi dello stato di gioco

    [SerializeField] string sabotageGeneratorEventName = "SABOTAGE GENERATOR";
    [SerializeField] UnityEventCharacter sabotageGenerator = new UnityEventCharacter();
    [SerializeField] private GeneratorState generatorState = GeneratorState.GeneratorOn;
    

    public override void Start() {
        initInteractable();

        sabotageGenerator.AddListener(switchOffGenerator);
    }

    private void switchOffGenerator(CharacterManager p) {
        generatorState = GeneratorState.GeneratorOff;
        gameState.turnOffPower();

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