using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private Transform selectedActiveWeaponTransform;

    [SerializeField] private List<WeaponItem> weaponItems = new List<WeaponItem>();
    [SerializeField] private List<ActionObjectItem> actionObjectItems = new List<ActionObjectItem>();

    [SerializeField] private int selectedWeapon = -1; // -1 significa non selezionato
    [SerializeField] private int selectedActionObject = -1;// -1 significa non selezionato


    [SerializeField] private CharacterManager characterManager;
    [SerializeField] private CharacterMovement characterMovement;

    public void Start() {
        initInventoryManager();
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

        // cambia arma selezionata
        selectWeapon(weaponItems.Count - 1);
    }

    public void selectWeapon(int weaponPos) {

        // disattiva l'arma precedente
        if(selectedWeapon != -1) {
            weaponItems[selectedWeapon].gameObject.SetActive(false);
        }
        

        selectedWeapon = weaponPos;
        weaponItems[selectedWeapon].gameObject.SetActive(true);

        characterMovement.updateAnimatorStateByInventoryWeaponType(weaponItems[selectedWeapon].getWeaponType);
    }

    public void addActionObjectItem(ActionObjectItem actionObjectItem) {
        actionObjectItems.Add(actionObjectItem);

        // disabilita gameobject
        actionObjectItem.gameObject.SetActive(false);

    }

    public void useSelectedWeapon() {
        if(selectedWeapon != -1) {
            weaponItems[selectedWeapon].useItem(characterManager);
        }
    }

    public WeaponType getSelectedWeaponType {
        get { 

            if(isSelectedWeapon) {
                return weaponItems[selectedWeapon].getWeaponType;
            } else {
                return WeaponType.melee;
            }
            
        }
    }

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
}
