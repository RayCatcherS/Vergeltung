using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlBullet : Bullet {

    private InventoryManager _sourceInventoryCharacter;

    public void setupBullet(Vector3 bulletDirection, InventoryManager sourceInventoryCharacter) {
        _bulletDirection = bulletDirection;
        _sourceInventoryCharacter = sourceInventoryCharacter;
    }

    protected override void characterCollision(CharacterManager character, Vector3 collisionPoint) {
        
        //Instantiate(particleBloodImpact, collisionPoint, Quaternion.identity);


        _sourceInventoryCharacter.inventoryAmmunitions[WeaponType.controlWeapon].ammunitionQuantity
                = _sourceInventoryCharacter.inventoryAmmunitions[WeaponType.controlWeapon].ammunitionQuantity - 1;

        // rebuild UI
        _sourceInventoryCharacter.characterManager.weaponUIController.buildUI(_sourceInventoryCharacter);

        // manage character control
        manageCharacterControl(character);
    }

    protected override void wallCollision(Vector3 collisionPoint, Vector3 collisionNormal) {
        //Instantiate(collisionWallImpact, collisionPoint, Quaternion.LookRotation(collisionNormal));

    }

    protected override void ragdollBoneCollision(Vector3 collisionPoint) {
        //Instantiate(particleBloodImpact, collisionPoint, Quaternion.identity);

        
    }

    protected override void glassCollision(RaycastHit hit) {
        //hit.transform.gameObject.GetComponent<ShatterableGlass>().shatterGlass();

    }

    private void manageCharacterControl(CharacterManager character) {

    }
}
