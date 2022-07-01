using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType{
    melee,
    pistol,
    rifle,
}
public class WeaponItem : InventoryItem
{

    private const string BASE_MELEE_ID = "base_melee";

    [Header("Weapon ref")]
    [SerializeField] private Transform _shootingTransform;
    [SerializeField] private Vector3 _weaponOffsetRotation;
    [SerializeField] private Transform _rightHandTransformRef;
    [SerializeField] private Transform _leftHandTransformRef;

    [Header("Weapon preview ref")]
    [SerializeField] Sprite _extractedWeaponPreview;
    [SerializeField] Sprite _puttedAwayWeaponPreview;


    [Header("Weapon configuration")]
    [SerializeField] private GameObject damageObject; // può essere un proiettile trigger in movimento che applica del danno o solo una sfera trigger che applica del danno
    [SerializeField] private WeaponType weaponType;
    [SerializeField] private int _magazineCapacity = 28;
    [SerializeField] private int _ammunition = 5;

    
    [SerializeField] private float shootFrequency = 0.15f;
    private float busyWeaponDurationTimeEnd = 0f;
    [SerializeField] private bool _automaticWeapon = false;
    [SerializeField] private bool _silencedWeapon = false;

    [Header("Weapon gamepad vibration config")]
    [SerializeField] private bool _vibrationOnUseWeapon = false;
    [SerializeField] private float _impulseTime = 0.15f;
    [SerializeField] private float _impulseForce = 0.60f;

    [Header("Weapon state")]
    public bool weaponUsed = false;
    /// <summary>
    /// Verifica se l'arma sta sparando
    /// </summary>
    public bool isWeaponFiring {
        get {

            bool result = false;
            if(Time.time > busyWeaponDurationTimeEnd) {
                result = false;
            } else {

                if(itemNameID != BASE_MELEE_ID) {
                    result = true;
                }
            }


            return result; 
       }
    }
    

    [Header("Weapon effects")]
    [SerializeField] private Transform spawnDamageObjectParticleTransform;
    [SerializeField] private GameObject spawnDamageObjectParticle; 

    [Header("Weapon sounds")]
    [SerializeField] private AudioClip gunShootSound;

    [Header("Weapon audio loud object")]
    /// questo oggetto emette suoni e scatenare eventi all'interno della sua area e viene generato all'utilizzo dell'item
    [SerializeField] private GameObject loudArea;
    [SerializeField] private LoudAreaType loudIntensity;

    // getters 

    public WeaponType getWeaponType {
        get { return weaponType; }
    }
    
    public float impulseTime {
        get { return _impulseTime; }
    }
    public float impulseForce {
        get { return _impulseForce; }
    }

    public bool automaticWeapon {
        get { return _automaticWeapon; }
    }
    public int ammunition {
        get { return _ammunition; }
    }
    public int magazineCapacity {
        get { return _magazineCapacity; }
    }

    // ref getters 
    public Transform shootingTransform {
        get { return _shootingTransform; }
    }
    public Vector3 weaponOffsetRotation {
        get { return _weaponOffsetRotation; }
    }
    public Transform rightHandTransformRef {
        get { return _rightHandTransformRef; }
    }
    public Transform leftHandTransformRef {
        get { return _leftHandTransformRef; }
    }
    public Sprite extractedWeaponPreview {
        get { return _extractedWeaponPreview; }
    }
    public Sprite puttedAwayWeaponPreview {
        get { return _puttedAwayWeaponPreview; }
    }



    /// <summary>
    /// Metodo richiamato dall'evento getWeaponEvent
    /// </summary>
    /// <param name="p">Istanza CharacterManager del 
    /// character che avvia il metodo tramite evento</param>
    public override void getItem(CharacterManager p) {

        // rimuovi arma dal precedente inventory
        if(_inventoryManager != null) {
            _inventoryManager.removeWeapon(itemNameID);
        }

        p.inventoryManager.addWeapon(this); // aggiungi arma al nuovo inventario

        InteractableObject interactableObject = gameObject.GetComponent<InteractableObject>();
        p.removeCharacterInteractableObject(interactableObject);
    }

    public override List<Interaction> getInteractions() {
        List<Interaction> eventRes = new List<Interaction>();
        eventRes.Add(
            new Interaction(getItemEvent, _getItemEventName, this)
        );

        return eventRes;
    }



    /// <summary>
    /// Utilizza weapon
    /// Questo metodo spawna sul luogo un damageObject
    /// </summary>
    /// <param name="p"></param>
    public override void useItem(CharacterManager p = null) {
        if (_ammunition > 0) {

            if (Time.time > busyWeaponDurationTimeEnd) {

                Vector3 posA = _shootingTransform.position;
                Vector3 posB = _shootingTransform.forward;


                // loud area
                if ((itemNameID != BASE_MELEE_ID)) {

                    if (inventoryManager != null) {

                        GameObject loudGameObject = Instantiate(loudArea, posA, shootingTransform.rotation);
                        loudGameObject.GetComponent<LoudArea>().initLoudArea(inventoryManager.characterManager.isPlayer ? loudIntensity : LoudAreaType.nothing, gunShootSound);
                        loudGameObject.GetComponent<LoudArea>().startLoudArea();
                    }
                }



                // damage object
                GameObject damageGO = Instantiate(damageObject, _shootingTransform.position, _shootingTransform.rotation);
                if (weaponType == WeaponType.pistol || weaponType == WeaponType.rifle) {

                    damageGO.GetComponent<Bullet>().setupBullet(posB);

                    if (spawnDamageObjectParticle != null) {

                        GameObject particleGO = Instantiate(spawnDamageObjectParticle, spawnDamageObjectParticleTransform.position, spawnDamageObjectParticleTransform.rotation);
                        
                    }
                }

                busyWeaponDurationTimeEnd = Time.time + shootFrequency;

                if (inventoryManager != null) {
                    if (inventoryManager.characterManager.chracterRole == Role.Player) {
                        _ammunition = _ammunition - 1;
                    }
                }
            }
            
        }
            
    }


    /// <summary>
    /// Utilizza weapon
    /// Questo metodo prevede il damageObject debba raggiungere una destinazione
    /// </summary>
    /// <param name="p">CharacterManager del character che esegue l'azione</param>
    /// <param name="destinationPosition">Destinazione che il damageObject deve raggiungere</param>
    public void useItem(CharacterManager p, Vector3 destinationPosition, GamePadVibrationController gamePadVibrationController) {

        if(_ammunition > 0) {
            if (Time.time > busyWeaponDurationTimeEnd) {
                Vector3 posA = _shootingTransform.position;
                Vector3 posB = destinationPosition;
                Vector3 bulletDirection = (posB - posA).normalized;



                // loud area
                if ((itemNameID != BASE_MELEE_ID)) {

                    if (inventoryManager != null) {

                        GameObject loudGameObject = Instantiate(loudArea, posA, shootingTransform.rotation);
                        loudGameObject.GetComponent<LoudArea>().initLoudArea(inventoryManager.characterManager.isPlayer ? loudIntensity : LoudAreaType.nothing, gunShootSound);
                        loudGameObject.GetComponent<LoudArea>().startLoudArea();
                    }
                }



                if (_vibrationOnUseWeapon) {
                    gamePadVibrationController.sendImpulse(
                        impulseTime, impulseForce
                    );
                }


                // damage object
                GameObject damageGO = Instantiate(damageObject, posA, _shootingTransform.rotation);
                if (weaponType == WeaponType.pistol || weaponType == WeaponType.rifle) {
                    damageGO.GetComponent<Bullet>().setupBullet(bulletDirection);

                    if (spawnDamageObjectParticle != null) {
                        GameObject particleGO = Instantiate(spawnDamageObjectParticle, spawnDamageObjectParticleTransform.position, spawnDamageObjectParticleTransform.rotation);
                        particleGO.transform.parent = p.gameObject.GetComponent<CharacterMovement>().characterModel.gameObject.transform;
                    }
                }


                busyWeaponDurationTimeEnd = Time.time + shootFrequency;

                if (inventoryManager != null) {
                    if (inventoryManager.characterManager.chracterRole == Role.Player) {
                        _ammunition = _ammunition - 1;
                    }
                }
                
                    
            }
        }
        
        
    }
    
    /// <summary>
    /// Aggiungi munizioni alla Weapon fino alla sua capacità massima
    /// </summary>
    public void addAmmunition(int ammo) {
        _ammunition = _ammunition + ammo;

        if(_ammunition > _magazineCapacity) {
            _ammunition = _magazineCapacity;
        }
    }
}
