using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour, InteractableInterface {
    [SerializeField] Outline outlineScript;

    public virtual void Start() {
        initInteractable();
    }

    protected void initInteractable() {


        InteractableObject interactableObject = gameObject.GetComponent<InteractableObject>();
        if(interactableObject == null) {
            gameObject.AddComponent<InteractableObject>();

            interactableObject = gameObject.GetComponent<InteractableObject>();
        }

        interactableObject.interactable = this;


    }

    public virtual List<Interaction> getInteractable() {
        return new List<Interaction>();
    }

    public virtual Interaction getMainInteraction() {
        Debug.LogError("il metodo getMainInteracion non è stato implementato nella classe figlia");
        return null;
    }

    public void focusInteractable() {
        outlineScript.changeOutlineColor(GameConstant.outlineInteractableColor);
        outlineScript.setEnableOutline(true);
    }
    public void unFocusInteractable() {
        outlineScript.setEnableOutline(false);
    }
}
