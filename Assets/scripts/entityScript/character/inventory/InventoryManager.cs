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
        weaponItem.gameObject.transform.eulerAngles = new Vector3(0, gameObject.GetComponent<CharacterMovement>().aimTransform.gameObject.transform.eulerAngles.y, 0);

        // cambia layer oggetto interattivo in default
        weaponItem.gameObject.layer = 0;
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

}
