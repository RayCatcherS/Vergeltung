using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponUIController : MonoBehaviour {
    [SerializeField] private Text ammunition;
    [SerializeField] private Text magazineCapacity;


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


        ammunition.text = inventoryManager.weaponItems[inventoryManager.selectedWeapon].ammunition.ToString();
        magazineCapacity.text = "/" + inventoryManager.weaponItems[inventoryManager.selectedWeapon].magazineCapacity.ToString();
    }
}