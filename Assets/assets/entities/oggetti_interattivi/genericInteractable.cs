using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class genericInteractable : Interactable {

    [SerializeField] string genericEventName = "SABOTAGE";
    [SerializeField] UnityEventCharacter genericEvent = new UnityEventCharacter();
    [SerializeField] private GeneratorState generatorState = GeneratorState.GeneratorOn;

    public override void Start() {
        initInteractable();
    }

    public override Interaction getMainInteraction() {
        return new Interaction(genericEvent, genericEventName, this);
    }

    public override List<Interaction> getInteractions() {

        List<Interaction> eventRes = new List<Interaction>();

        eventRes.Add(new Interaction(genericEvent, genericEventName, this));

        return eventRes;
    }

}
