using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType{
    melee,
    pistol,
    rifle,
}
public class WeaponItem : InventoryItem
{

    [SerializeField] private Transform _shootingTransform;
    [SerializeField] private Vector3 _weaponOffsetRotation; // 30.826  131.512  -3.875


    [Header("Gun configuration")]
    [SerializeField] private GameObject damageSpawnObject; // può essere un proiettile trigger in movimento che applica del danno o solo una sfera trigger che applica del danno
    [SerializeField] private int magazineCapacity = 10;
    [SerializeField] private int currentMagazineCapacity = 10;
    [SerializeField] private int ammunition = 100;

    [SerializeField] private WeaponType weaponType;
    [SerializeField] private bool automaticWeapon = false;
    public bool weaponUsed = false;
    [SerializeField] private bool silencedWeapon = false;

    


    // getters 
    public Vector3 weaponOffsetRotation {
        get { return _weaponOffsetRotation; }
    }
    public WeaponType getWeaponType {
        get { return weaponType; }
    }
    public Transform shootingTransform {
        get { return _shootingTransform; }
    }

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

    

    public override void useItem(CharacterManager p) {
        Instantiate(damageSpawnObject, _shootingTransform.position, _shootingTransform.rotation);
    }

    public void useItem(CharacterManager p, Vector3 destinationPosition) {

        Vector3 posA = _shootingTransform.position;
        Vector3 posB = destinationPosition;
        Vector3 bulletDirection = (posB - posA).normalized;

        GameObject damageGO = Instantiate(damageSpawnObject, posA, _shootingTransform.rotation);


        if(damageGO.GetComponent<Bullet>() != null) {
            damageGO.GetComponent<Bullet>().setupBullet(bulletDirection);
        }
    }
}
