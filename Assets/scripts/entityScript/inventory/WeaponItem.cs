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


    [Header("Weapon configuration")]
    [SerializeField] private GameObject damageObject; // può essere un proiettile trigger in movimento che applica del danno o solo una sfera trigger che applica del danno
    [SerializeField] private int magazineCapacity = 10;
    [SerializeField] private int currentMagazineCapacity = 10;
    [SerializeField] private int ammunition = 100;

    [SerializeField] private WeaponType weaponType;
    [SerializeField] private bool automaticWeapon = false;
    [SerializeField] private bool silencedWeapon = false;

    [Header("Weapon state")]
    public bool weaponUsed = false;

    [Header("Weapon effects")]
    [SerializeField] private Transform spawnDamageItemParticleTransform;
    [SerializeField] private GameObject spawnDamageItemParticle;

    [Header("Weapon gamepad vibration config")]
    [SerializeField] private float _impulseTime = 0.15f;
    [SerializeField] private float _impulseForce = 0.60f;

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
    public float impulseTime {
        get { return _impulseTime; }
    }
    public float impulseForce {
        get { return _impulseForce; }
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

    

    /// <summary>
    /// Utilizza weapon
    /// Questo metodo spawna sul luogo un damageObject
    /// </summary>
    /// <param name="p"></param>
    public override void useItem(CharacterManager p) {
        Instantiate(damageObject, _shootingTransform.position, _shootingTransform.rotation);
    }


    /// <summary>
    /// Utilizza weapon
    /// Questo metodo prevede il damageObject debba raggiungere una destinazione
    /// </summary>
    /// <param name="p">CharacterManager del character che esegue l'azione</param>
    /// <param name="destinationPosition">Destinazione che il damageObject deve raggiungere</param>
    public void useItem(CharacterManager p, Vector3 destinationPosition) {

        Vector3 posA = _shootingTransform.position;
        Vector3 posB = destinationPosition;
        Vector3 bulletDirection = (posB - posA).normalized;

        GameObject damageGO = Instantiate(damageObject, posA, _shootingTransform.rotation);


        if(weaponType == WeaponType.pistol || weaponType == WeaponType.rifle) {

            damageGO.GetComponent<Bullet>().setupBullet(bulletDirection);

            if(spawnDamageItemParticle != null) {
                GameObject particleGO = Instantiate(spawnDamageItemParticle, spawnDamageItemParticleTransform.position, spawnDamageItemParticleTransform.rotation);
                particleGO.transform.parent = gameObject.transform;
            }
        }
    }
}
