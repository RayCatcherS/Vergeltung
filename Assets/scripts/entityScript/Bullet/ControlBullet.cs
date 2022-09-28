using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ControlBullet : Bullet {

    private InventoryManager _sourceInventoryCharacter;

    private Dictionary<Role, int> controlRoleCost = new Dictionary<Role, int>() {

        { Role.Civilian, 1},
        { Role.EnemyGuard, 3}
    };

    public void setupBullet(Vector3 bulletDirection, InventoryManager sourceInventoryCharacter) {
        _bulletDirection = bulletDirection;
        _sourceInventoryCharacter = sourceInventoryCharacter;
    }

    protected override void characterCollision(CharacterManager character, Vector3 collisionPoint, Vector3 damageVelocity) {
        
        //Instantiate(particleBloodImpact, collisionPoint, Quaternion.identity);

        

        // manage character control
        manageCharacterControlAsync(character);

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

    private async Task manageCharacterControlAsync(CharacterManager characterToControl) {
        int _sourceCharacterControlAmmunitions 
            = _sourceInventoryCharacter.inventoryAmmunitions[WeaponType.controlWeapon].ammunitionQuantity;

        // le munizioni sono abbastanza per il ruolo del character che si vuole controllare
        if(_sourceCharacterControlAmmunitions >= controlRoleCost[characterToControl.chracterRole]) {

            // controlla se il character non è già controllato
            if(!characterToControl.isStackControlled) {


                // rimuovi munizioni controllo
                _sourceInventoryCharacter.inventoryAmmunitions[WeaponType.controlWeapon].ammunitionQuantity
                    = _sourceInventoryCharacter.inventoryAmmunitions[WeaponType.controlWeapon].ammunitionQuantity
                    - controlRoleCost[characterToControl.chracterRole];


                // controllo character
                characterToControl.playerWarpController.addCharacterToWarpStack(characterToControl);
                await characterToControl.playerWarpController.warpToCharacter(characterToControl);

                characterToControl.sceneEntitiesController.gameObject.GetComponent<PlayerWarpController>().startSwitchCharacterMode();
            }

        }


    }
}
