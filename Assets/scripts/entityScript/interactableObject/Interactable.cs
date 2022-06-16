using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour, InteractableInterface {
    [SerializeField] private Outline outlineScript;
    [SerializeField] private MeshRenderer interactableMeshEffect;

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

    public virtual List<Interaction> getInteractions() {
        return new List<Interaction>();
    }

    public virtual Interaction getMainInteraction() {
        Debug.LogError("il metodo getMainInteracion non è stato implementato nella classe figlia");
        return null;
    }

    public void focusInteractableOutline() {
        outlineScript.changeOutlineColor(GameConstant.outlineInteractableColor);
        outlineScript.setEnableOutline(true);
    }
    public void unFocusInteractableOutline() {
        outlineScript.setEnableOutline(false);
    }

    public void interactableMeshEffectSetEnebled(bool value) {

        if(interactableMeshEffect != null) {

            interactableMeshEffect.gameObject.SetActive(value);
        }
    }
}
