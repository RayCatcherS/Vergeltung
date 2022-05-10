using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
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
    [SerializeField] private CharacterManager characterManager;
    [SerializeField] private CharacterMovement characterMovement;
    [SerializeField] public SphereCollider interactableInventoryColliderTrigger;

    [Header("Weapon References")]
    [SerializeField] private Transform headViewTransform; // serve a controllare tramite ray cast se tra la testa è l'arma c'è un collider
    [SerializeField] private Transform weaponTransform;

    [Header("Character Rig References")]
    [SerializeField] private RigBuilder rigBuilder;
    [SerializeField] private TwoBoneIKConstraint rightHandRig;
    [SerializeField] private TwoBoneIKConstraint leftHandRig;


    [Header("Inventory Objects")]
    [SerializeField] private List<WeaponItem> _weaponItems = new List<WeaponItem>();
    [SerializeField] private List<ActionObjectItem> actionObjectItems = new List<ActionObjectItem>();

    [Header("Inventory state")]
    [SerializeField] private bool _weaponPuttedAway = true;
    [SerializeField] private int _selectedWeapon = 0; // -1 significa non selezionato
    //[SerializeField] private int selectedActionObject = -1;// -1 significa non selezionato

    [Header("Weapon aim render configuration")]
    [SerializeField] private LineRenderer weaponLineRenderer;
    [SerializeField] private Material weaponLineRendererMaterial;
    [SerializeField] public Gradient extractedweaponLineRendererGradient;
    [SerializeField] public Gradient puttedAwayweaponLineRendererGradient;


    // getter - setter
    public int selectedWeapon {
        get { return _selectedWeapon; }
    }
    public List<WeaponItem> weaponItems {
        get { return _weaponItems; }
    }
    public bool weaponPuttedAway {
        get { return _weaponPuttedAway; }
    }
    


    public void Awake() {
        initInventoryManager();
        initDrawPlayerWeaponLineRendered();

        gamePadVibration = GameObject.Find("GameController").GetComponent<GamePadVibrationController>();
    }

    public void Update() {
        drawAimWeaponLineRendered();

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

    void initInventoryManager() {

        CharacterManager chM = gameObject.GetComponent<CharacterManager>();
        if(chM == null) {
            gameObject.AddComponent<CharacterManager>();
            chM = gameObject.GetComponent<CharacterManager>();
        }
        characterManager = chM;




        CharacterMovement chMov = gameObject.GetComponent<CharacterMovement>();
        if (chMov == null) {
            gameObject.AddComponent<CharacterMovement>();
            chMov = gameObject.GetComponent<CharacterMovement>();
        }
        characterMovement = chMov;
        
    }

    /// <summary>
    /// Aggiungi arma all'inventario
    /// </summary>
    /// <param name="weaponItem"></param>
    public void addWeapon(WeaponItem weaponItem) {

        int weaponInInventary = isWeaponInInventory(weaponItem.itemNameID);

        //cerca se la weapon è già presente
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

            _weaponItems[weaponInInventary].addAmmunition(weaponItem.ammunition);
            Destroy(weaponItem.gameObject);
        }


        // builda UI solo se player
        if (gameObject.GetComponent<CharacterManager>().isPlayer) {
            gameObject.GetComponent<CharacterManager>().weaponUIController.buildUI(this);
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
        characterManager.aimedCharacter = null; // rimuovi character mirato


        // configurazione rig
        rightHandRig.data.target = _weaponItems[_selectedWeapon].rightHandTransformRef;
        leftHandRig.data.target = _weaponItems[_selectedWeapon].leftHandTransformRef;

        rigBuilder.Build();

        // imposta rig
        if(_weaponItems[_selectedWeapon].getWeaponType == WeaponType.melee) {
            if (weaponPuttedAway) {
                rightHandRig.weight = 0;
                leftHandRig.weight = 0;
            } else {
                rightHandRig.weight = 1;
                leftHandRig.weight = 0;
            }
            
        } else {

            if(!weaponPuttedAway) {
                rightHandRig.weight = 1;
                leftHandRig.weight = 1;
                
            } else {
                rightHandRig.weight = 0;
                leftHandRig.weight = 0;
            }
            
        }

        configPutAwayExtractWeapon();

        // builda UI solo se player
        if (gameObject.GetComponent<CharacterManager>().isPlayer) {
            gameObject.GetComponent<CharacterManager>().weaponUIController.buildUI(this);
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


        if (_selectedWeapon != -1 && !isGunThroughWall() && !_weaponPuttedAway) {

            Vector3 destinationPosition = drawAimWeaponLineRendered();
            _weaponItems[_selectedWeapon].useItem(characterManager, destinationPosition, gamePadVibration);

            // builda UI solo se player
            if (gameObject.GetComponent<CharacterManager>().isPlayer) {
                gameObject.GetComponent<CharacterManager>().weaponUIController.buildUI(this);
            }
        }
    }

    /// <summary>
    /// Rende disponibile le interaction delle armi di tutto l'inventario del character
    /// Offre agli altri character la possibilità di interagire con l'inventario del character
    /// Usato quando il character muore
    /// </summary>
    public void setInventoryAsInteractable() {
        _weaponItems[_selectedWeapon].gameObject.SetActive(false);

        gameObject.layer = INTERACTABLE_LAYER; // cambia layer in interactable
        interactableInventoryColliderTrigger.enabled = true;
    }

    public override List<Interaction> getInteractions() {
        List<Interaction> allWeaponsInteractions = new List<Interaction>();

        for(int i = 0; i < _weaponItems.Count; i++) {

            if(_weaponItems[i].itemNameID != BASE_MELEE_ID) {
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

        return allWeaponsInteractions;
    }


    public void initDrawPlayerWeaponLineRendered() {
        LineRenderer lineRenderer = gameObject.GetComponent<LineRenderer>();
        if (lineRenderer == null) {
            gameObject.AddComponent<LineRenderer>();
            lineRenderer = gameObject.GetComponent<LineRenderer>();
        }
        weaponLineRenderer = lineRenderer;

        weaponLineRenderer.material = weaponLineRendererMaterial;
        weaponLineRenderer.startWidth = 0.1f;
        weaponLineRenderer.endWidth = 0.1f;

        if (_weaponPuttedAway) {
            weaponLineRenderer.colorGradient = puttedAwayweaponLineRendererGradient;
        } else {
            weaponLineRenderer.colorGradient = extractedweaponLineRendererGradient;
        }
            
    }

    ////// <summary>
    /// Disegna tramite il componente line rendere una 
    /// retta che parte dall'arma al punto mirato dal character
    /// 
    /// </summary>
    /// <returns>Restituisce la posizione puntata</returns>
    public Vector3 drawAimWeaponLineRendered() {

        Vector3 aimedPosition = Vector3.zero;

        if (isSelectedWeapon && /*_weaponItems[_selectedWeapon].getWeaponType != WeaponType.melee &&*/ characterManager.isPlayer && !isGunThroughWall()) {
            weaponLineRenderer.enabled = true;


            weaponLineRenderer.SetPosition(0, _weaponItems[_selectedWeapon].shootingTransform.position);

            
            RaycastHit hit;
            Ray ray = new Ray(_weaponItems[_selectedWeapon].shootingTransform.position, new Vector3(
                Mathf.Sin((characterMovement.characterModel.transform.eulerAngles.y) * (Mathf.PI / 180)),
                0,
                Mathf.Cos((characterMovement.characterModel.transform.eulerAngles.y) * (Mathf.PI / 180))
            ));


            if (Physics.Raycast(ray, out hit, RAY_DISTANCE, ALL_LAYERS, QueryTriggerInteraction.Ignore)) { // se il raycast hitta


               
                // setta il character mirato nel character manager come character aimato
                if(hit.transform.gameObject.layer == CHARACTER_LAYERS ) {

                    if(!hit.transform.gameObject.GetComponent<CharacterManager>().isPlayer) {
                        characterManager.aimedCharacter = hit.transform.gameObject.GetComponent<CharacterManager>();

                        Debug.DrawLine(_weaponItems[_selectedWeapon].shootingTransform.position, hit.point);
                        weaponLineRenderer.SetPosition(1, hit.point);
                    }

                } else {
                    characterManager.aimedCharacter = null;

                    Debug.DrawLine(_weaponItems[_selectedWeapon].shootingTransform.position, hit.point);
                    weaponLineRenderer.SetPosition(1, hit.point);
                }
                aimedPosition = hit.point;



            } else { // se il ray cast non hitta

                characterManager.aimedCharacter = null;


                Debug.DrawLine(_weaponItems[_selectedWeapon].shootingTransform.position, _weaponItems[_selectedWeapon].shootingTransform.position + new Vector3(
                    Mathf.Sin((characterMovement.characterModel.transform.eulerAngles.y) * (Mathf.PI / 180)),
                    0,
                    Mathf.Cos((characterMovement.characterModel.transform.eulerAngles.y) * (Mathf.PI / 180))
                ) * RAY_DISTANCE);


                weaponLineRenderer.SetPosition(1, _weaponItems[_selectedWeapon].shootingTransform.position + new Vector3(
                    Mathf.Sin((characterMovement.characterModel.transform.eulerAngles.y) * (Mathf.PI / 180)),
                    0,
                    Mathf.Cos((characterMovement.characterModel.transform.eulerAngles.y) * (Mathf.PI / 180))
                ) * RAY_DISTANCE);


                aimedPosition = _weaponItems[_selectedWeapon].shootingTransform.position + new Vector3(
                    Mathf.Sin((characterMovement.characterModel.transform.eulerAngles.y) * (Mathf.PI / 180)),
                    0,
                    Mathf.Cos((characterMovement.characterModel.transform.eulerAngles.y) * (Mathf.PI / 180))
                ) * RAY_DISTANCE;
            }


            

            
        } else {
            weaponLineRenderer.enabled = false;
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

        if(getSelectedWeaponType != WeaponType.melee) {
            if (Physics.Linecast(headViewTransform.position, _weaponItems[_selectedWeapon].shootingTransform.position, out hit, ALL_LAYERS, QueryTriggerInteraction.Ignore)) {

                if (hit.collider != null) {
                    res = true;

                    //Debug.Log("gun Throug hWall");
                } else {
                    res = false;
                }
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
    private void putAwayWeapon() {
        _weaponPuttedAway = true;
        configPutAwayExtractWeapon();
    }

    /// <summary>
    /// estrai l'arma selezionata
    /// </summary>
    private void extractWeapon() {
        _weaponPuttedAway = false;
        configPutAwayExtractWeapon();
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

        configSelectedWeapon();
    }

    /// <summary>
    /// configura gli elementi del character in base 
    /// </summary>
    public void configPutAwayExtractWeapon() {
        if (_weaponPuttedAway) {

            weaponLineRenderer.colorGradient = puttedAwayweaponLineRendererGradient;
            _weaponItems[_selectedWeapon].gameObject.SetActive(false); // attiva arma selezionata
        } else {


            if (_weaponItems[_selectedWeapon].getWeaponType == WeaponType.melee) {
                weaponLineRenderer.colorGradient = puttedAwayweaponLineRendererGradient;
                
            } else {
                weaponLineRenderer.colorGradient = extractedweaponLineRendererGradient;
                
            }
            _weaponItems[_selectedWeapon].gameObject.SetActive(true); // attiva arma selezionata
        }
    }
}
