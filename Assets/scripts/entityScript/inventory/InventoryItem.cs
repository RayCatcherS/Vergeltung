using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryItem : Interactable {

    [Header("Item config")]
    [SerializeField] protected string _itemNameID = "item";
    [SerializeField] protected bool _prohibitedItem = true;
    public bool prohibitedItem {
        get { return _prohibitedItem; }
    }

    [SerializeField] protected string _getItemEventName = "GET ";
    [SerializeField] protected UnityEventCharacter getItemEvent = new UnityEventCharacter();
    

    [Header("Item ref")]
    [SerializeField] protected InventoryManager _inventoryManager; //inventory manager del character che possiede l'oggetto

    // getter
    public string itemNameID {
        get { return _itemNameID; }
    }
    public string getItemEventName {
        get { return _getItemEventName; }
    }
    public InventoryManager inventoryManager {
        get { return _inventoryManager; }
        set { _inventoryManager = value; }
    }
    public void Awake() {
        _getItemEventName = _getItemEventName + " " + _itemNameID;
        initInteractable();
        getItemEvent.AddListener(getItem);
    }

    public virtual void getItem(CharacterManager p) {
        
    }

    public virtual void useItem(CharacterManager p) {

    }
}