using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlBullet : Bullet {

    private InventoryManager _sourceInventoryCharacter;

    private Dictionary<Role, int> controlRoleCost = new Dictionary<Role, int>() {

        { Role.Civilian, 1},
        { Role.EnemyGuard, 2}
    };

    public void setupBullet(Vector3 bulletDirection, InventoryManager sourceInventoryCharacter) {
        _bulletDirection = bulletDirection;
        _sourceInventoryCharacter = sourceInventoryCharacter;
    }

    protected override void characterCollision(CharacterManager character, Vector3 collisionPoint) {
        
        //Instantiate(particleBloodImpact, collisionPoint, Quaternion.identity);

        

        // manage character control
        manageCharacterControl(character);

        // rebuild UI
        _sourceInventoryCharacter.characterManager.weaponUIController.buildUI(_sourceInventoryCharacter);
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

    private void manageCharacterControl(CharacterManager characterToControl) {
        int _sourceCharacterControlAmmunitions 
            = _sourceInventoryCharacter.inventoryAmmunitions[WeaponType.controlWeapon].ammunitionQuantity;

        // le munizioni sono 
        if(_sourceCharacterControlAmmunitions >= controlRoleCost[characterToControl.chracterRole]) {

            // rimuovi munizioni controllo
            _sourceInventoryCharacter.inventoryAmmunitions[WeaponType.controlWeapon].ammunitionQuantity
                = _sourceInventoryCharacter.inventoryAmmunitions[WeaponType.controlWeapon].ammunitionQuantity - controlRoleCost[characterToControl.chracterRole];



            // controllo character
            characterToControl.playerWarpController.addCharacterToWarpStack(characterToControl);
            characterToControl.playerWarpController.warpToCharacter(characterToControl);

        }


    }
}
