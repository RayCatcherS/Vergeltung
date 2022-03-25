using System.Collections.Generic;
using UnityEngine;

public class Interactable : InteractableAbstract {
    [SerializeField] Outline outlineScript;

    override public List<Interaction> getInteractable() {
        return new List<Interaction>();
    }

    override public Interaction getMainInteracion() {
        Debug.LogError("il metodo getMainInteracion non è stato implementato nella classe figlia");
        return null;
    }

    public void focusInteractable() {
        outlineScript.enabled = true;
    }
    public void unFocusInteractable() {
        outlineScript.enabled = false;
    }
}
