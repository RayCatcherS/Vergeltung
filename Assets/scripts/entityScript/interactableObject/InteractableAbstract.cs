using System.Collections.Generic;
using UnityEngine;

public abstract class InteractableAbstract : MonoBehaviour
{
    public abstract List<Interaction> getInteractable();
    public abstract Interaction getMainInteracion();
    protected bool busyInteractable = false;
}
