using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType{
    pistol,
    rifle,
    granade
}
public class WeaponItem : InventoryItem
{

    [SerializeField] private int magazineCapacity = 10;
    [SerializeField] private int currentMagazineCapacity = 10;
    [SerializeField] private int ammunition = 100;
    [SerializeField] private WeaponType weaponType;
    [SerializeField] private bool automaticWeapon = false;
    [SerializeField] private bool silencedWeapon = false;


    /// <summary>
    /// Metodo richiamato dall'evento getWeaponEvent
    /// </summary>
    /// <param name="p">Istanza CharacterManager del 
    /// character che avvia il metodo tramite evento</param>
    public override void getItem(CharacterManager p) {
        p.inventoryManager.addWeapon(this);

        InteractableObject interactableObject = gameObject.GetComponent<InteractableObject>();
        p.removeCharacterInteractableObject(interactableObject);
    }

    public override List<Interaction> getInteractable() {
        List<Interaction> eventRes = new List<Interaction>();
        eventRes.Add(
            new Interaction(getItemEvent, getItemEventName, this)
        );

        return eventRes;
    }
}
