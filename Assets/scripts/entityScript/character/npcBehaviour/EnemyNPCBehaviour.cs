using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyNPCBehaviour : BaseNPCBehaviour {

    static public GameObject initEnemyNPComponent(GameObject gameObject, CharacterSpawnPoint spwanPoint) {

        EnemyNPCBehaviour enemyNPCNewComponent = gameObject.GetComponent<EnemyNPCBehaviour>();
        enemyNPCNewComponent.initNPCComponent(spwanPoint, gameObject.GetComponent<CharacterMovement>());

        return gameObject;
    }


    /// <summary>
    /// implementazione suspiciousAlertBehaviour
    /// </summary>
    public override void suspiciousAlertBehaviour() {

        if (isFocusAlarmCharacterVisible) {
            lastSeenFocusAlarmCharacterPosition = focusAlarmCharacter.transform.position; // setta ultima posizione in cui è stato visto l'alarm character


            Vector3 targetDirection = focusAlarmCharacter.transform.position - gameObject.transform.position;
            targetDirection.y = 0;
            characterMovement.rotateCharacter(new Vector2(targetDirection.x, targetDirection.z), true);
            
        } else {

            if (lastSeenFocusAlarmCharacterPosition == Vector3.zero) { // solo se il character non è riuscito a prendere la vecchia posizione del character/player
                lastSeenFocusAlarmCharacterPosition = focusAlarmCharacter.transform.position; // setta ultima posizione in cui è stato visto l'alarm character
            }
            Vector3 targetDirection = lastSeenFocusAlarmCharacterPosition - gameObject.transform.position;
            targetDirection.y = 0;
            characterMovement.rotateCharacter(new Vector2(targetDirection.x, targetDirection.z), true);

        }

    }
    /// <summary>
    /// implementazione hostilityAlertBehaviour
    /// </summary>
    public override void hostilityAlertBehaviour() {
        if (isFocusAlarmCharacterVisible) {
            lastSeenFocusAlarmCharacterPosition = focusAlarmCharacter.transform.position; // setta ultima posizione in cui è stato visto l'alarm character


            Vector3 targetDirection = focusAlarmCharacter.transform.position - gameObject.transform.position;
            targetDirection.y = 0;
            characterMovement.rotateCharacter(new Vector2(targetDirection.x, targetDirection.z), true);


            characterInventoryManager.useSelectedWeapon();
        } else {

            if(lastSeenFocusAlarmCharacterPosition == Vector3.zero) { // solo se il character non è riuscito a prendere la vecchia posizione del character/player
                lastSeenFocusAlarmCharacterPosition = focusAlarmCharacter.transform.position; // setta ultima posizione in cui è stato visto l'alarm character
            }
            Vector3 targetDirection = lastSeenFocusAlarmCharacterPosition - gameObject.transform.position;
            targetDirection.y = 0;
            characterMovement.rotateCharacter(new Vector2(targetDirection.x, targetDirection.z), true);

        }
    }
    /// <summary>
    /// implementazione soundAlert1Behaviour
    /// </summary>
    public override void soundAlert1Behaviour() {

    }

    /// <summary>
    /// Per le guardie nemiche quando termina l'HostilityTimer viene aggiornato il dizionario a livello globale
    /// passando il dizionario del character (viene fatta l'unione)
    /// Inoltre tutti gli altri character nemici avranno il dizionario hostility aggiornato
    /// </summary>
    public override void onHostilityAlertTimerEnd() {
        gameObject.GetComponent<CharacterManager>().globalGameState.updateGlobalWantedHostileCharacters(this._wantedHostileCharacters);
    }

    /// <summary>
    /// Avvisa tutti gli npc nell'area AlertAreaCharacters
    /// </summary>
    public override void onHostilityAlert() {


        base.onHostilityAlert();
    }
}
