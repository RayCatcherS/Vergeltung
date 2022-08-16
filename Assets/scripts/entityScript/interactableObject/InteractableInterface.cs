using System.Collections.Generic;
using UnityEngine;

public interface InteractableInterface
{
    /// <summary>
    /// Ottieni lista di interazioni che l'oggetto interattivo offre
    /// </summary>
    /// <returns></returns>
    public List<Interaction> getInteractions(CharacterManager character);
    public Interaction getMainInteraction();
}
