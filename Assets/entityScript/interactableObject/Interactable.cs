using System.Collections.Generic;
using UnityEngine.Events;

public class Interactable : InteractableAbstract {
    override public List<Interaction> getInteractable() {
        return new List<Interaction>();
    }
}
