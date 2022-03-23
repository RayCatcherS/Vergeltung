using UnityEngine.Events;

public class Interaction
{
    private UnityEventCharacter unityEvent;
    private string eventName;

    public Interaction(UnityEventCharacter e, string eName) {
        this.unityEvent = e;
        this.eventName = eName;
    }

    public UnityEventCharacter getUnityEvent() {
        return unityEvent;
    }

    public string getUnityEventName() {
        return eventName;
    }
}
