using UnityEngine.Events;

public class Interaction
{
    private UnityEventPlayer unityEvent;
    private string eventName;

    public Interaction(UnityEventPlayer e, string eName) {
        this.unityEvent = e;
        this.eventName = eName;
    }

    public UnityEventPlayer getUnityEvent() {
        return unityEvent;
    }

    public string getUnityEventName() {
        return eventName;
    }
}
