using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType{
    actionObject,
    weapon
}

public class InventoryItem : Interactable {
    [SerializeField] protected string itemName = "item";
    [SerializeField] protected ItemType itemType;


    [SerializeField] protected string getItemEventName = "GET ";
    [SerializeField] protected UnityEventCharacter getItemEvent = new UnityEventCharacter();

    public override void Start() {
        getItemEventName = getItemEventName + itemName;
        initInteractable();
        getItemEvent.AddListener(getItem);
    }

    public virtual void getItem(CharacterManager p) {
        
    }

    public virtual void useItem(CharacterManager p) {

    }
}