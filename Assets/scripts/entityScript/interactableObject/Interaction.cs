using UnityEngine.Events;

public class Interaction
{
    private UnityEventCharacter unityEvent;
    private string eventName;
    private Interactable interactable; // oggetto dell'interazione

    public Interaction(UnityEventCharacter e, string eName, Interactable interactableObject) {
        this.unityEvent = e;
        this.eventName = eName;
        this.interactable = interactableObject;
    }

    public UnityEventCharacter getUnityEvent() {
        return unityEvent;
    }

    public string getUnityEventName() {
        return eventName;
    }

    public Interactable getInteractable() {
        return interactable;
    }
}
