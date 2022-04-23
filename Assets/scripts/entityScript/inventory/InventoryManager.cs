using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    // raycast const
    private const int ALL_LAYERS = -1;
    private const int CHARACTER_LAYERS = 7;
    private const int RAY_DISTANCE = 100;

    [SerializeField] private Transform headViewTransform; // serve a controllare tramite ray cast se tra la testa è l'arma c'è un collider
    [SerializeField] private Transform selectedActiveWeaponTransform;

    [SerializeField] private List<WeaponItem> weaponItems = new List<WeaponItem>();
    [SerializeField] private List<ActionObjectItem> actionObjectItems = new List<ActionObjectItem>();

    [SerializeField] private int selectedWeapon = 0; // -1 significa non selezionato
    [SerializeField] private int selectedActionObject = -1;// -1 significa non selezionato

    [SerializeField] private LineRenderer weaponLineRenderer;
    [SerializeField] private Material weaponLineRendererMaterial;
    [SerializeField] public Gradient weaponLineRendererGradient;


    [SerializeField] private CharacterManager characterManager;
    [SerializeField] private CharacterMovement characterMovement;

    

    public void Start() {
        initInventoryManager();
        initDrawPlayerWeaponLineRendered();
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
                return weaponItems[selectedWeapon].getWeaponType;
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

            if (selectedWeapon == -1) {
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
            weapon = weaponItems[selectedWeapon];
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
        weaponItems.Add(weaponItem);

        // disabilita gameobject
        weaponItem.gameObject.SetActive(false);

        //setta il gameobject weapon come figlio del selectedActiveWeaponTransform
        weaponItem.gameObject.transform.SetParent(selectedActiveWeaponTransform);

        // setta coordinate
        weaponItem.gameObject.transform.localPosition = Vector3.zero;

        // setta rotazione
        weaponItem.gameObject.transform.localEulerAngles = weaponItem.weaponOffsetRotation;

        // cambia layer oggetto interattivo in default
        weaponItem.gameObject.layer = 0;

        // disattiva collider trigger interactable
        weaponItem.gameObject.GetComponent<SphereCollider>().enabled = false;

        // cambia arma selezionata
        selectWeapon(weaponItems.Count - 1);
    }

    /// <summary>
    /// seleziona arma character
    /// </summary>
    /// <param name="weaponPos"></param>
    public void selectWeapon(int weaponPos) {

        // disattiva l'arma precedente
        if(selectedWeapon != -1) {

            weaponItems[selectedWeapon].gameObject.SetActive(false);
        }
        selectedWeapon = weaponPos;
        weaponItems[selectedWeapon].gameObject.SetActive(true);

        characterMovement.updateAnimatorStateByInventoryWeaponType(weaponItems[selectedWeapon].getWeaponType);
        characterManager.aimedCharacter = null; // rimuovi character mirato
    }


    public void selectWeapon(string weaponId) {

        // disattiva l'arma precedente
        
        if (selectedWeapon != -1) {

            weaponItems[selectedWeapon].gameObject.SetActive(false);
        }

        for (int i = 0; i < weaponItems.Count; i++) {

            if (weaponItems[i].itemNameID == weaponId) {
                selectedWeapon = i;
            }
        }


        weaponItems[selectedWeapon].gameObject.SetActive(true);
        characterMovement.updateAnimatorStateByInventoryWeaponType(weaponItems[selectedWeapon].getWeaponType);

        
    }
    public void nextWeapon() {
        int value;
        if (selectedWeapon != -1) {
            if (weaponItems.Count - 1 == selectedWeapon) {
                value = 0;
            } else {
                value = selectedWeapon + 1;
            }

            selectWeapon(value);
        }
    }


    public void previewWeapon() {
        int value;
        if (selectedWeapon != -1) {
            if (selectedWeapon == 0) {
                value = weaponItems.Count - 1;
            } else {
                value = selectedWeapon - 1;
            }

            selectWeapon(value);
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
        if(selectedWeapon != -1) {

            weaponItems[selectedWeapon].useItem(characterManager, drawAimWeaponLineRendered());
        }
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
        weaponLineRenderer.colorGradient = weaponLineRendererGradient;
    }

    ////// <summary>
    /// Disegna tramite il componente line rendere una 
    /// retta che parte dall'arma al punto mirato dal character
    /// 
    /// </summary>
    /// <returns>Restituisce la posizione puntata</returns>
    public Vector3 drawAimWeaponLineRendered() {

        Vector3 aimedPosition = Vector3.zero;

        if (isSelectedWeapon && weaponItems[selectedWeapon].getWeaponType != WeaponType.melee && characterManager.isPlayer && !gunThroughWall()) {
            weaponLineRenderer.enabled = true;


            weaponLineRenderer.SetPosition(0, weaponItems[selectedWeapon].shootingTransform.position);

            
            RaycastHit hit;
            Ray ray = new Ray(weaponItems[selectedWeapon].shootingTransform.position, new Vector3(
                Mathf.Sin((characterMovement.characterModel.transform.eulerAngles.y) * (Mathf.PI / 180)),
                0,
                Mathf.Cos((characterMovement.characterModel.transform.eulerAngles.y) * (Mathf.PI / 180))
            ));


            if (Physics.Raycast(ray, out hit, RAY_DISTANCE, ALL_LAYERS, QueryTriggerInteraction.Ignore)) { // se il raycast hitta


               
                // setta il character mirato nel character manager come character aimato
                if(hit.transform.gameObject.layer == CHARACTER_LAYERS ) {

                    if(!hit.transform.gameObject.GetComponent<CharacterManager>().isPlayer) {
                        characterManager.aimedCharacter = hit.transform.gameObject.GetComponent<CharacterManager>();

                        Debug.DrawLine(weaponItems[selectedWeapon].shootingTransform.position, hit.point);
                        weaponLineRenderer.SetPosition(1, hit.point);
                    }

                } else {
                    characterManager.aimedCharacter = null;

                    Debug.DrawLine(weaponItems[selectedWeapon].shootingTransform.position, hit.point);
                    weaponLineRenderer.SetPosition(1, hit.point);
                }
                aimedPosition = hit.point;



            } else { // se il ray cast non hitta

                characterManager.aimedCharacter = null;


                Debug.DrawLine(weaponItems[selectedWeapon].shootingTransform.position, weaponItems[selectedWeapon].shootingTransform.position + new Vector3(
                    Mathf.Sin((characterMovement.characterModel.transform.eulerAngles.y) * (Mathf.PI / 180)),
                    0,
                    Mathf.Cos((characterMovement.characterModel.transform.eulerAngles.y) * (Mathf.PI / 180))
                ) * RAY_DISTANCE);


                weaponLineRenderer.SetPosition(1, weaponItems[selectedWeapon].shootingTransform.position + new Vector3(
                    Mathf.Sin((characterMovement.characterModel.transform.eulerAngles.y) * (Mathf.PI / 180)),
                    0,
                    Mathf.Cos((characterMovement.characterModel.transform.eulerAngles.y) * (Mathf.PI / 180))
                ) * RAY_DISTANCE);


                aimedPosition = weaponItems[selectedWeapon].shootingTransform.position + new Vector3(
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
    public bool gunThroughWall() {
        bool res = false;
        RaycastHit hit;

        if(getSelectedWeaponType != WeaponType.melee) {
            if (Physics.Linecast(headViewTransform.position, weaponItems[selectedWeapon].shootingTransform.position, out hit, ALL_LAYERS, QueryTriggerInteraction.Ignore)) {

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
}
