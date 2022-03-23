using System.Collections.Generic;
using UnityEngine;

public class Interactable : InteractableAbstract {
    [SerializeField] Outline outlineScript;
    override public List<Interaction> getInteractable() {
        return new List<Interaction>();
    }


    public void focusInteractable() {
        outlineScript.enabled = true;
    }
    public void unFocusInteractable() {
        outlineScript.enabled = false;
    }
}
