using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionObjectItem : InventoryItem {


    /// <summary>
    /// Metodo richiamato dall'evento getActionObjectEvent
    /// </summary>
    /// <param name="p">Istanza CharacterManager del 
    /// character che avvia il metodo tramite evento</param>
    public override void getItem(CharacterManager p) {
        p.inventoryManager.addActionObjectItem(this);

        InteractableObject interactableObject = gameObject.GetComponent<InteractableObject>();
        p.removeCharacterInteractableObject(interactableObject);
    }

    public override List<Interaction> getInteractable() {
        List<Interaction> eventRes = new List<Interaction>();
        eventRes.Add(
            new Interaction(getItemEvent, getItemEventName, this)
        );

        return eventRes;
    }
}
