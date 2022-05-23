using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericUnaryInteractable : Interactable {

    [SerializeField] string genericEventName = "ACTION";
    [SerializeField] UnityEventCharacter genericEvent = new UnityEventCharacter();
    [SerializeField] private bool repetableInteraction = false;
    [SerializeField] private bool unRepetableInteractionStateActive = true;

    public override void Start() {
        initInteractable();

    }

    public override Interaction getMainInteraction() {
        if (!repetableInteraction) {

            if (unRepetableInteractionStateActive) {
                unRepetableInteractionStateActive = false;
                return new Interaction(genericEvent, genericEventName, this);
            } else {
                return new Interaction(new UnityEventCharacter(), "", this);
            }

        } else {

            return new Interaction(genericEvent, genericEventName, this);
        }

    }

    public override List<Interaction> getInteractions() {

        List<Interaction> eventRes = new List<Interaction>();

        if (!repetableInteraction) {

            if (unRepetableInteractionStateActive) {
                unRepetableInteractionStateActive = false;
                eventRes.Add(new Interaction(genericEvent, genericEventName, this));
            }

        } else {

            eventRes.Add(new Interaction(genericEvent, genericEventName, this));
        }


        return eventRes;
    }
}
