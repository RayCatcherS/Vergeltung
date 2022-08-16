using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour, InteractableInterface {
    [SerializeField] private Outline outlineScript;
    [SerializeField] private Renderer interactableMeshEffect;

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

    public virtual List<Interaction> getInteractions(CharacterManager character) {
        return new List<Interaction>();
    }

    public virtual Interaction getMainInteraction() {
        Debug.LogError("il metodo getMainInteracion non è stato implementato nella classe figlia");
        return null;
    }

    public void focusInteractableOutline() {

        if(outlineScript != null) {
            outlineScript.changeOutlineColor(GameConstant.outlineInteractableColor);
            outlineScript.setEnableOutline(true);
        }
        
    }
    public void unFocusInteractableOutline() {

        if(outlineScript != null) {
            outlineScript.setEnableOutline(false);
        }

    }

    public void interactableMeshEffectSetEnebled(bool value) {
        
        if (interactableMeshEffect != null) {
            interactableMeshEffect.gameObject.SetActive(value);
        }
    }

    /// <summary>
    /// attiva l'effetto interactable se ci sono interazioni dispobili
    /// altrimenti no
    /// </summary>
    protected void rebuildInteractableMeshEffect(List<Interaction> interactions) {
        // set effetto oggetto con interazioni non vuote
        int interactionCount = interactions.Count;
        if (interactionCount == 0) {
            interactableMeshEffectSetEnebled(false);
        } else {
            interactableMeshEffectSetEnebled(true);
        }
    }
}
