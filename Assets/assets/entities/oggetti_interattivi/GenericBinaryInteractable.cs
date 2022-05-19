using System.Collections;
using System.Collections.Generic;
using UnityEngine;


enum BinaryInteractableState {
    state1,
    state2
}
public class GenericBinaryInteractable : Interactable {

    [SerializeField] string genericEventName = "ACTION";
    [SerializeField] UnityEventCharacter genericEvent1 = new UnityEventCharacter();
    [SerializeField] UnityEventCharacter genericEvent2 = new UnityEventCharacter();
    [SerializeField] private bool repetableInteraction = false;
    [SerializeField] private bool unRepetableInteractionStateActive = true;
    [SerializeField] private BinaryInteractableState binaryInteractableState = BinaryInteractableState.state1;

    public override void Start() {
        initInteractable();

        genericEvent1.AddListener((CharacterManager c) => { changeBinaryState(BinaryInteractableState.state2); });
        genericEvent2.AddListener((CharacterManager c) => { changeBinaryState(BinaryInteractableState.state1); });
    }
    
    public override Interaction getMainInteraction() {
        if(!repetableInteraction) {

            if(unRepetableInteractionStateActive) {
                unRepetableInteractionStateActive = false;
                return new Interaction(genericEvent1, genericEventName, this);
            } else {
                return new Interaction(new UnityEventCharacter(), "", this);
            }
            
        } else {

            if(binaryInteractableState == BinaryInteractableState.state1) {
                return new Interaction(genericEvent1, genericEventName, this);
            } else {
                return new Interaction(genericEvent2, genericEventName, this);
            }
        }
        
    }

    public override List<Interaction> getInteractions() {

        List<Interaction> eventRes = new List<Interaction>();

        if (!repetableInteraction) {

            if (unRepetableInteractionStateActive) {
                unRepetableInteractionStateActive = false;
                eventRes.Add(new Interaction(genericEvent1, genericEventName, this));
            }

        } else {

            if (binaryInteractableState == BinaryInteractableState.state1) {
                eventRes.Add(new Interaction(genericEvent1, genericEventName, this));
            } else {
                eventRes.Add(new Interaction(genericEvent2, genericEventName, this));
            }
        }
        

        return eventRes;
    }

    private void changeBinaryState(BinaryInteractableState state) {
        binaryInteractableState = state;
    }

}
