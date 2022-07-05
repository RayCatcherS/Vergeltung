using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponUIController : MonoBehaviour {

    [Header("UI ref")]
    [SerializeField] private Text ammunition;
    [SerializeField] private Text magazineCapacity;
    [SerializeField] private Image selectedWeaponPreview;
    [SerializeField] private Image previousWeaponPreview;
    [SerializeField] private Image nextWeaponPreview;


    private void Awake() {
        disableAmmoUI();
    }

    public void disableAmmoUI() {
        ammunition.enabled = false;
        magazineCapacity.enabled = false;
    }

    public void enableAmmoUI() {
        ammunition.enabled = true;
        magazineCapacity.enabled = true;
    }

    /// <summary>
    /// build an inventory manager UI
    /// </summary>
    /// <param name="inventoryManager"></param>
    public void buildUI(InventoryManager inventoryManager) {


        if(inventoryManager.weaponItems[inventoryManager.selectedWeapon].getWeaponType != WeaponType.melee) {
            enableAmmoUI();
        } else {
            disableAmmoUI();
        }


        // setta Sprite arma selezionata
        if(inventoryManager.weaponPuttedAway) {
            selectedWeaponPreview.sprite = inventoryManager.weaponItems[inventoryManager.selectedWeapon].puttedAwayWeaponPreview;
        } else {
            selectedWeaponPreview.sprite = inventoryManager.weaponItems[inventoryManager.selectedWeapon].extractedWeaponPreview;
        }


        // setta sprite arma precedente a quella selezionata
        if(inventoryManager.selectedWeapon > 0) {
            previousWeaponPreview.enabled = true;
            previousWeaponPreview.sprite = inventoryManager.weaponItems[inventoryManager.selectedWeapon - 1].puttedAwayWeaponPreview;
        } else {
            previousWeaponPreview.enabled = false;
        }

        // setta sprite arma successiva a quella selezionata
        if (inventoryManager.selectedWeapon < inventoryManager.weaponItems.Count - 1) {
            nextWeaponPreview.enabled = true;
            nextWeaponPreview.sprite = inventoryManager.weaponItems[inventoryManager.selectedWeapon + 1].puttedAwayWeaponPreview;
        } else {
            nextWeaponPreview.enabled = false;
        }


        // setta UI munizioni

        ammunition.text = inventoryManager.inventoryAmmunitions[inventoryManager.weaponItems[inventoryManager.selectedWeapon].getWeaponType].ammunitionQuantity.ToString();

            
        magazineCapacity.text = "/" + inventoryManager.weaponItems[inventoryManager.selectedWeapon].magazineCapacity.ToString();
    }
}