using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Animations.Rigging;

public class InventoryManager : Interactable {
    // const
    private const int ALL_LAYERS = -1;
    private const int CHARACTER_LAYERS = 7;
    private const int RAY_DISTANCE = 100;

    private const int INTERACTABLE_LAYER = 3;

    private const string BASE_MELEE_ID = "base_melee";



    [Header("GamePad vibration Reference")]
    GamePadVibrationController gamePadVibration;

    [Header("Character References")]
    [SerializeField] private CharacterManager _characterManager;
    public CharacterManager characterManager {
        get { return _characterManager; }
    }
    [SerializeField] private CharacterMovement characterMovement;
    [SerializeField] public SphereCollider interactableInventoryColliderTrigger;

    [Header("Weapon References")]
    [SerializeField] private Transform headViewTransform; // serve a controllare tramite ray cast se tra la testa è l'arma c'è un collider
    [SerializeField] private Transform weaponTransform;

    [Header("Character Rig References")]
    [SerializeField] private RigBuilder rigBuilder;
    [SerializeField] private TwoBoneIKConstraint _rightHandRig;
    public TwoBoneIKConstraint rightHandRig {
        get { return _rightHandRig; }
    }
    [SerializeField] private TwoBoneIKConstraint _leftHandRig;
    public TwoBoneIKConstraint leftHandRig {
        get { return _leftHandRig; }
    }


    [Header("Inventory Objects")]
    [SerializeField] private List<WeaponItem> _weaponItems = new List<WeaponItem>();
    [SerializeField] private List<ActionObjectItem> actionObjectItems = new List<ActionObjectItem>();
    [SerializeField] private bool _isFlashlightTaken = false;
    public bool isFlashlightTaken {
        get { return _isFlashlightTaken; }
        set {
            _isFlashlightTaken = value;
        }
    }
    [SerializeField] private CharacterFlashLight _characterFlashLight;
    public CharacterFlashLight characterFlashLight {
        get { return _characterFlashLight; }
    }
    [Header("Inventory Ammunitions and config")]
    [SerializeField] private Dictionary<WeaponType, Ammunition> _inventoryAmmunitions = new Dictionary<WeaponType, Ammunition>();
    public Dictionary<WeaponType, Ammunition> inventoryAmmunitions {
        get { return _inventoryAmmunitions; }
    }
    [SerializeField]
    private Dictionary<WeaponType, Ammunition> _maxInventoryAmmunitions = new Dictionary<WeaponType, Ammunition>() {
        { WeaponType.melee, new Ammunition(WeaponType.melee, 1) },
        { WeaponType.pistol, new Ammunition(WeaponType.pistol, 12) },
        { WeaponType.rifle, new Ammunition(WeaponType.rifle, 28) },
        { WeaponType.controlWeapon, new Ammunition(WeaponType.controlWeapon, 14) },
    };
    public Dictionary<WeaponType, Ammunition> maxInventoryAmmunitions {
        get { return _maxInventoryAmmunitions; }
    }

    [Header("Inventory state")]
    [SerializeField] private bool _weaponPuttedAway = true;
    public bool weaponPuttedAway {
        get { return _weaponPuttedAway; }
    }
    [SerializeField] private int _selectedWeapon = 0; // -1 significa non selezionato
    //[SerializeField] private int selectedActionObject = -1;// -1 significa non selezionato

    [Header("Weapon aim render configuration")]
    [SerializeField] private LineRenderer _weaponLineRenderer;
    public LineRenderer weaponLineRenderer {
        get { return _weaponLineRenderer; }
    }
    [SerializeField] private Material weaponLineRendererMaterial;
    [SerializeField] public Gradient extractedweaponLineRendererGradient;
    [SerializeField] public Gradient puttedAwayweaponLineRendererGradient;
    [SerializeField] public Gradient controlWeaponLineRendererGradient;

    [Header("Aim UI target")]
    private AimUIManager _aimTargetImage;
    public AimUIManager aimTargetImage {
        set { _aimTargetImage = value; }
    }

    // getter - setter
    public int selectedWeapon {
        get { return _selectedWeapon; }
    }
    public List<WeaponItem> weaponItems {
        get { return _weaponItems; }
    }    

    /// <summary>
    /// Verifica se l'arma selezionata sta sparando
    /// </summary>
    public bool isWeaponFiring {
        get {

            return weaponItems[selectedWeapon].isWeaponFiring;
        }
    }

    public bool isSelectedWeaponEmpty {
        get { return weaponItems[_selectedWeapon].isWeaponAmmunitionEmpty; }
    }
    public bool isAllWeaponEmpty {
        get {
            bool result = true;

            foreach(WeaponItem weapon in weaponItems) {
                if(!weapon.isWeaponAmmunitionEmpty) {
                    result = false;
                    break;
                }
            }

            return result;
        }
    }

    public void Awake() {
        initDrawPlayerWeaponLineRendered();

        gamePadVibration = GameObject.Find("GameController").GetComponent<GamePadVibrationController>();
    }

    public void Update() {

        AimInformation aimInfo = getAimInformation();


        // update aimed character
        if(_characterManager.isPlayer) {

            if(!isGunThroughWall()) {
                characterManager.aimedCharacter = aimInfo.aimedCharacter;
            }
        }


        if(_characterManager.isPlayer) {


            if(weaponItems[selectedWeapon].itemNameID != BASE_MELEE_ID) {


                if(!isGunThroughWall()) {
                    _aimTargetImage.setAimTargetEnabled(true);

                    if(_weaponPuttedAway) {
                        _aimTargetImage.aimLowOpacity();

                    } else {

                        if(aimInfo.isAimedCharacter) {
                            _aimTargetImage.aimHighOpacity();
                        } else {
                            _aimTargetImage.aimMediumOpacity();
                        }

                    }

                    _aimTargetImage.updateUIWorldPosition(aimInfo.aimedHitPosition);
                } else {

                    _aimTargetImage.setAimTargetEnabled(false);
                }

                
                
            } else {

                _aimTargetImage.setAimTargetEnabled(false);
            }
            


            drawAimWeaponLineRendered(aimInfo.aimedHitPosition);
        }
        

    }

    /// <summary>
    /// Ottieni tipo di arma attualemente selezionata
    /// </summary>
    public WeaponType getSelectedWeaponType {
        get {

            if (isSelectedWeapon) {
                return _weaponItems[_selectedWeapon].getWeaponType;
            } else {
                return WeaponType.melee;
            }

        }
    }

    /// <summary>
    /// Se l'arma è selezionata
    /// </summary>
    public bool isSelectedWeapon {
        get {
            bool res = false;

            if (_selectedWeapon == -1) {
                res = false;
            } else {
                res = true;
            }

            return res;
        }
    }

    public WeaponItem getSelectWeapon() {
        WeaponItem weapon = null;

        if (isSelectedWeapon) {
            weapon = _weaponItems[_selectedWeapon];
        }

        return weapon;
    }


    /// <summary>
    /// Aggiungi arma all'inventario
    /// </summary>
    /// <param name="weaponItem"></param>
    public void addWeapon(WeaponItem weaponItem) {

        // disabilita effetto interactable
        weaponItem.interactableMeshEffectSetEnebled(false);
        weaponItem.unFocusInteractableOutline();


        //cerca se la weapon è già presente
        int weaponInInventary = isWeaponInInventory(weaponItem.itemNameID);
        if (weaponInInventary == -1) {
            // aggiungi istanza alla lista weapon dell'inventory manager
            _weaponItems.Add(weaponItem);

            // associa l'inventario all'arma
            weaponItem.inventoryManager = this;

            // disabilita gameobject
            weaponItem.gameObject.SetActive(false);

            //setta il gameobject weapon come figlio del selectedActiveWeaponTransform
            weaponItem.gameObject.transform.SetParent(weaponTransform);

            // setta coordinate
            weaponItem.gameObject.transform.localPosition = Vector3.zero;

            // setta rotazione
            weaponItem.gameObject.transform.localEulerAngles = weaponItem.weaponOffsetRotation;

            // cambia layer oggetto interattivo in default
            weaponItem.gameObject.layer = 0;

            // disattiva collider trigger interactable
            weaponItem.gameObject.GetComponent<SphereCollider>().enabled = false;
        } else {
            
            Destroy(weaponItem.gameObject);
        }

        // aggiungi al dizionario dell'inventario le munizioni
        if(_inventoryAmmunitions.ContainsKey(weaponItem.ammunition.ammunitionType)) {
            _inventoryAmmunitions[weaponItem.ammunition.ammunitionType].ammunitionQuantity 
                = _inventoryAmmunitions[weaponItem.ammunition.ammunitionType].ammunitionQuantity + weaponItem.ammunition.ammunitionQuantity;
        } else {
            _inventoryAmmunitions.Add(weaponItem.ammunition.ammunitionType, weaponItem.ammunition);
        }
        

        // max ammunition reached
        if(_inventoryAmmunitions[weaponItem.ammunition.ammunitionType].ammunitionQuantity > _maxInventoryAmmunitions[weaponItem.ammunition.ammunitionType].ammunitionQuantity) {
            _inventoryAmmunitions[weaponItem.ammunition.ammunitionType].ammunitionQuantity = _maxInventoryAmmunitions[weaponItem.ammunition.ammunitionType].ammunitionQuantity;
        }


        // builda UI solo se player
        if (_characterManager.isPlayer) {
            _characterManager.weaponUIController.buildUI(this);
        }
    }

    /// <summary>
    /// seleziona arma character by int
    /// </summary>
    /// <param name="weaponPos"></param>
    public void selectWeapon(int weaponPos) {

        if (_selectedWeapon != -1) {

            _weaponItems[_selectedWeapon].gameObject.SetActive(false);
        }

        if (weaponPos > _weaponItems.Count - 1) {
            Debug.LogError("L'arma selezionata è fuori dall'index dell'equipagiamento selezionato");
            _selectedWeapon = 0;
        } else {
            // disattiva l'arma precedente
            
            _selectedWeapon = weaponPos;
        }

        configSelectedWeapon();
    }

    /// <summary>
    /// seleziona arma character by string
    /// </summary>
    /// <param name="weaponPos"></param>
    public void selectWeapon(string weaponId) {

        // disattiva l'arma precedente
        
        if (_selectedWeapon != -1) {

            _weaponItems[_selectedWeapon].gameObject.SetActive(false);
        }

        for (int i = 0; i < _weaponItems.Count; i++) {

            if (_weaponItems[i].itemNameID == weaponId) {
                _selectedWeapon = i;
            }
        }


        configSelectedWeapon();
    }

    /// <summary>
    /// Configura arma selezionata
    /// 
    /// Imposta animazione in base all'arma selezionata e in base a
    /// _weaponPuttedAway che indica se l'arma è estratta o meno
    /// 
    /// Imposta anche il rig del melee in base a _weaponPuttedAway
    /// </summary>
    private void configSelectedWeapon() {

        characterMovement.updateAnimatorStateByInventoryWeaponType(_weaponItems[_selectedWeapon].getWeaponType, this); // configura animazione in base all'arma selezionata
        _characterManager.aimedCharacter = null; // rimuovi character mirato


        // configurazione rig
        _rightHandRig.data.target = _weaponItems[_selectedWeapon].rightHandTransformRef;
        _leftHandRig.data.target = _weaponItems[_selectedWeapon].leftHandTransformRef;

        rigBuilder.Build();

        // imposta rig
        if(_weaponItems[_selectedWeapon].getWeaponType == WeaponType.melee || _weaponItems[_selectedWeapon].getWeaponType == WeaponType.controlWeapon) {
            if (weaponPuttedAway) {
                _rightHandRig.weight = 0;
                _leftHandRig.weight = 0;
            } else {
                _rightHandRig.weight = 1;
                _leftHandRig.weight = 0;
            }
            
        } else {

            if(!weaponPuttedAway) {
                _rightHandRig.weight = 1;
                _leftHandRig.weight = 1;
                
            } else {
                _rightHandRig.weight = 0;
                _leftHandRig.weight = 0;
            }
            
        }

        configPutAwayExtractWeapon();

        // builda UI solo se player
        if (_characterManager.isPlayer) {
            _characterManager.weaponUIController.buildUI(this);
        }
        
    }
    
    /// <summary>
    /// Seleziona arma successiva
    /// </summary>
    public void selectNextWeapon() {
        int value;

        if(_selectedWeapon != _weaponItems.Count - 1) {
            if (_weaponItems.Count != 1) {
                if (_selectedWeapon != -1) {
                    if (_weaponItems.Count - 1 == _selectedWeapon) {
                        value = 0;
                    } else {
                        value = _selectedWeapon + 1;
                    }

                    selectWeapon(value);
                }
            }
        }

        if (_characterManager.isPlayer) {
            prohibitedWeaponAlarmUICheck();
        }
    }


    public void selectPreviousWeapon() {
        int value;

        if(_selectedWeapon != 0) {
            if (_weaponItems.Count != 1) {
                if (_selectedWeapon != -1) {
                    if (_selectedWeapon == 0) {
                        value = _weaponItems.Count - 1;
                    } else {
                        value = _selectedWeapon - 1;
                    }

                    selectWeapon(value);
                }
            }
        }

        if (_characterManager.isPlayer) {
            prohibitedWeaponAlarmUICheck();
        }
    }

    /// <summary>
    /// rimuove weapon dall'istanza dell'inventario partendo dal ItemId
    /// </summary>
    /// <param name="weaponId"></param>
    public void removeWeapon(string weaponId) {
        // disattiva l'arma precedente

        for (int i = 0; i < _weaponItems.Count; i++) {

            if (_weaponItems[i].itemNameID == weaponId) {
                _weaponItems.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Aggiungi action object all'inventario
    /// </summary>
    /// <param name="actionObjectItem"></param>
    public void addActionObjectItem(ActionObjectItem actionObjectItem) {
        actionObjectItems.Add(actionObjectItem);

        // disabilita gameobject
        actionObjectItem.gameObject.SetActive(false);

    }

    /// <summary>
    /// Usa arma attualmente selezionata
    /// </summary>
    public void useSelectedWeapon() {
        
        if(!characterManager.isBusy) {
            if(_selectedWeapon != -1 && !isGunThroughWall() && !_weaponPuttedAway) {

                Vector3 destinationPosition = getAimInformation().aimedHitPosition;


                // builda UI solo se player
                if(_characterManager.isPlayer) {
                    _weaponItems[_selectedWeapon].useItem(_characterManager, destinationPosition, gamePadVibration);
                    _characterManager.weaponUIController.buildUI(this);
                } else {
                    _weaponItems[_selectedWeapon].useItem(_characterManager);
                }
            }
        }
        
    }

    /// <summary>
    /// Rende disponibile le interaction delle armi di tutto l'inventario del character
    /// Offre agli altri character la possibilità di interagire con l'inventario del character
    /// Usato quando il character muore
    /// </summary>
    public void setInventoryAsInteractable() {
        if(_selectedWeapon != -1) {
            _weaponItems[_selectedWeapon].gameObject.SetActive(false);
        }
        

        gameObject.layer = INTERACTABLE_LAYER; // cambia layer in interactable
        interactableInventoryColliderTrigger.enabled = true;

        
        rebuildInteractableMeshEffect(getInteractions());
    }

    public override List<Interaction> getInteractions() {
        List<Interaction> allWeaponsInteractions = new List<Interaction>();

        for(int i = 0; i < _weaponItems.Count; i++) {

            if(_weaponItems[i].itemNameID != BASE_MELEE_ID) {


                if(_weaponItems[i].ammunition.ammunitionQuantity != 0) {
                    UnityEventCharacter eventWeapon = new UnityEventCharacter();
                    eventWeapon.AddListener(_weaponItems[i].getItem);

                    allWeaponsInteractions.Add(
                        new Interaction(
                            eventWeapon,
                            _weaponItems[i].getItemEventName,
                            this
                        )
                    );
                }
                
            }
        }
        rebuildInteractableMeshEffect(allWeaponsInteractions);


        return allWeaponsInteractions;
    }


    private void initDrawPlayerWeaponLineRendered() {
        LineRenderer lineRenderer = gameObject.GetComponent<LineRenderer>();
        if (lineRenderer == null) {
            gameObject.AddComponent<LineRenderer>();
            lineRenderer = gameObject.GetComponent<LineRenderer>();
        }
        _weaponLineRenderer = lineRenderer;

        _weaponLineRenderer.material = weaponLineRendererMaterial;
        _weaponLineRenderer.startWidth = 0.1f;
        _weaponLineRenderer.endWidth = 0.1f;

        if (_weaponPuttedAway) {
            _weaponLineRenderer.colorGradient = puttedAwayweaponLineRendererGradient;
        } else {
            _weaponLineRenderer.colorGradient = extractedweaponLineRendererGradient;
        }
            
    }

    /// <summary>
    /// Disegna tramite il componente line rendere una 
    /// retta che parte dall'arma al punto mirato dal character
    public void drawAimWeaponLineRendered(Vector3 hitPosition) {

        if (isSelectedWeapon && !isGunThroughWall()) {
            _weaponLineRenderer.enabled = true;


            _weaponLineRenderer.SetPosition(0, _weaponItems[_selectedWeapon].shootingTransform.position);
            _weaponLineRenderer.SetPosition(1, hitPosition);


        } else {
            _weaponLineRenderer.enabled = false;
        }
    }


    /// <summary>
    /// Ottieni informazioni sulla mira del character
    /// </summary>
    /// <returns>Restituisce un oggetto con tutte le info sulla mira del character</returns>
    AimInformation getAimInformation() {
        AimInformation aimedPosition;


        RaycastHit hit;
        Ray ray = new Ray(_weaponItems[_selectedWeapon].shootingTransform.position, new Vector3(
            Mathf.Sin((characterMovement.characterModel.transform.eulerAngles.y) * (Mathf.PI / 180)),
            0,
            Mathf.Cos((characterMovement.characterModel.transform.eulerAngles.y) * (Mathf.PI / 180))
        ));


        if(Physics.Raycast(ray, out hit, RAY_DISTANCE, ALL_LAYERS, QueryTriggerInteraction.Ignore)) { // se il raycast hitta



            // setta il character mirato nel character manager come character aimato
            if(hit.transform.gameObject.layer == CHARACTER_LAYERS) {

                if(!hit.transform.gameObject.GetComponent<CharacterManager>().isPlayer) {

                    aimedPosition = new AimInformation(hit.point, hit.transform.gameObject.GetComponent<CharacterManager>());


                } else {
                    aimedPosition = new AimInformation(Vector3.zero, null);
                }

            } else {
                aimedPosition = new AimInformation(hit.point, null);
            }




        } else { // se il ray cast non hitta


            Vector3 noHitPos;
            noHitPos = _weaponItems[_selectedWeapon].shootingTransform.position + new Vector3(
                    Mathf.Sin((characterMovement.characterModel.transform.eulerAngles.y) * (Mathf.PI / 180)),
                    0,
                    Mathf.Cos((characterMovement.characterModel.transform.eulerAngles.y) * (Mathf.PI / 180))
                ) * RAY_DISTANCE;

            aimedPosition = new AimInformation(noHitPos, null);
        }
        return aimedPosition;
    }

    /// <summary>
    /// Il metodo controlla se l'arma attraversa un muro.
    /// Un raycast parte dalla testa del character e raggiunge il weaponShootTransform
    /// se questo viene ostruito allora l'arma sta attraversando un collider
    /// </summary>
    /// <returns></returns>
    public bool isGunThroughWall() {
        bool res = false;
        RaycastHit hit;

        if (Physics.Linecast(headViewTransform.position, _weaponItems[_selectedWeapon].shootingTransform.position, out hit, ALL_LAYERS, QueryTriggerInteraction.Ignore)) {

            if (hit.collider != null) {
                res = true;

                //Debug.Log("gun Throug hWall");
            } else {
                res = false;
            }
        }


        return res;
    }


    /// <summary>
    /// Verifica se la weapon è nell'inventario partendo dall'id
    /// se l'arma è nell'inventario ritorna la posizione in cui si trova
    /// altrimenti torna -1
    /// </summary>
    /// <param name="weaponItemID"></param>
    /// <returns></returns>
    public int isWeaponInInventory(string weaponItemID) {
        int result = -1;

        for(int i = 0; i < _weaponItems.Count; i++) {

            if(_weaponItems[i].itemNameID == weaponItemID) {
                result = i;
            }
            
        }

        return result;
    }

    /// <summary>
    /// Riponi l'arma selezionata
    /// </summary>
    public void putAwayWeapon() {
        _weaponPuttedAway = true;
        configPutAwayExtractWeapon();

        // aggiorna ui solo se sei il player
        if (_characterManager.isPlayer) {
            prohibitedWeaponAlarmUICheck();
        }
        configSelectedWeapon();
    }

    /// <summary>
    /// estrai l'arma selezionata
    /// </summary>
    public void extractWeapon() {
        _weaponPuttedAway = false;
        configPutAwayExtractWeapon();

        // aggiorna ui solo se sei il player
        if (_characterManager.isPlayer) {
            prohibitedWeaponAlarmUICheck();
        }
        configSelectedWeapon();
    }

    /// <summary>
    /// usare questo check per verificare e abilitare icona arma proibita
    /// </summary>
    private void prohibitedWeaponAlarmUICheck() {
        if(isUsedItemProhibitedCheck()) {
            _characterManager.alarmAlertUIController.potentialVisiblyArmedAlarmOn();
        } else {
            _characterManager.alarmAlertUIController.potentialVisiblyArmedAlarmOff();
        }
    }

    /// <summary>
    /// richiama putAwayWeapon o extractWeapon in base
    /// allo stato di _weaponPuttedAway
    /// </summary>
    public void switchPutAwayExtractWeapon() {
        if(_weaponPuttedAway) {
            extractWeapon();
        } else {
            putAwayWeapon();
        }
    }

    /// <summary>
    /// Builda laser puntatore e arma scoperta o meno
    /// </summary>
    private void configPutAwayExtractWeapon() {
        if (_weaponPuttedAway) {

            _weaponLineRenderer.colorGradient = puttedAwayweaponLineRendererGradient;
            _weaponItems[_selectedWeapon].gameObject.SetActive(false); // attiva arma selezionata
        } else {


            if(_weaponItems[_selectedWeapon].getWeaponType == WeaponType.melee) {
                _weaponLineRenderer.colorGradient = puttedAwayweaponLineRendererGradient;

            } else if(_weaponItems[_selectedWeapon].getWeaponType == WeaponType.controlWeapon) {
                _weaponLineRenderer.colorGradient = controlWeaponLineRendererGradient;

            } else {
                _weaponLineRenderer.colorGradient = extractedweaponLineRendererGradient;

            }
            _weaponItems[_selectedWeapon].gameObject.SetActive(true); // attiva arma selezionata
        }
    }


    /// <summary>
    /// Verifica se il character sta impugnando un item proibito
    /// Il risultato dipenderà dal ruolo.
    /// Il ruolo EnemyGuard avrà la possibilita di impugnare qualsiasi tipo di item
    /// </summary>
    /// <returns></returns>
    public bool isUsedItemProhibitedCheck() {
        bool result = false;


        if(_characterManager.gameObject.GetComponent<CharacterRole>().role == Role.EnemyGuard) {
            result = false;
        } else {


            if (!weaponPuttedAway) {

                result = weaponItems[selectedWeapon].prohibitedItem;
                
            } else {
                result = false;
            }
        }
        

        return result;
    }


    /// <summary>
    /// Seleziona la prima arma con munizioni
    /// </summary>
    public void selectFirstWeaponWithAmmunition() {


        if(!isAllWeaponEmpty) {



            for(int i = 0; i < weaponItems.Count; i++) {

                if(!weaponItems[i].isWeaponAmmunitionEmpty) {


                    selectWeapon(i);
                }
            }
        }
        
    }
}
public class AimInformation {
    private Vector3 _aimedHitPosition = Vector3.zero;
    public Vector3 aimedHitPosition {
        get { return _aimedHitPosition; }
    }

    public bool isAimedCharacter {
        get { return (_aimedCharacter != null); }
    }
    private CharacterManager _aimedCharacter;
    public CharacterManager aimedCharacter {
        get { return _aimedCharacter; }
    }




    public AimInformation(Vector3 hitPos, CharacterManager aimedC) {
        _aimedHitPosition = hitPos;
        _aimedCharacter = aimedC;
    }
}