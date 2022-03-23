using System.Collections.Generic;
using UnityEngine;

public abstract class InteractableAbstract : MonoBehaviour
{
    public abstract List<Interaction> getInteractable();
    protected bool busyInteractable = false;
}
