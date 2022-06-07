using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CivilianNPCBehaviour : BaseNPCBehaviour {
    static public GameObject initCivilianNPCComponent(GameObject gameObject, CharacterSpawnPoint spawnPoint) {

        CivilianNPCBehaviour enemyNPCNewComponent = gameObject.GetComponent<CivilianNPCBehaviour>();
        enemyNPCNewComponent.initNPCComponent(spawnPoint, gameObject.GetComponent<CharacterMovement>());

        return gameObject;
    }




    /// <summary>
    /// implementazione suspiciousAlertBehaviour
    /// </summary>
    public override void suspiciousAlertBehaviour() {
        if (isFocusAlarmCharacterVisible) {
            Vector3 targetDirection = focusAlarmCharacter.transform.position - gameObject.transform.position;
            targetDirection.y = 0;
            characterMovement.rotateCharacter(new Vector2(targetDirection.x, targetDirection.z), true);


            lastSeenFocusAlarmCharacterPosition = focusAlarmCharacter.transform.position; // setta ultima posizione in cui è stato visto l'alarm character
        } else {
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
            Vector3 targetDirection = focusAlarmCharacter.transform.position - gameObject.transform.position;
            targetDirection.y = 0;
            characterMovement.rotateCharacter(new Vector2(targetDirection.x, targetDirection.z), true);


            lastSeenFocusAlarmCharacterPosition = focusAlarmCharacter.transform.position; // setta ultima posizione in cui è stato visto l'alarm character
        } else {
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
    /// Avvisa tutti gli npc nell'area AlertAreaCharacters
    /// </summary>
    public override void onHostilityAlert() {


        base.onHostilityAlert();
    }
}
