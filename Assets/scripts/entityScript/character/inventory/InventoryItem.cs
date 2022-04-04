using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryItem : Interactable {
    [SerializeField] protected string _itemNameID = "item";

    [SerializeField] protected string getItemEventName = "GET ";
    [SerializeField] protected UnityEventCharacter getItemEvent = new UnityEventCharacter();

    // getter
    public string itemNameID {
        get { return _itemNameID; }
    }

    public override void Start() {
        getItemEventName = getItemEventName + _itemNameID;
        initInteractable();
        getItemEvent.AddListener(getItem);
    }

    public virtual void getItem(CharacterManager p) {
        
    }

    public virtual void useItem(CharacterManager p) {

    }
}