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

        
        rotateAndAimSubBehaviour();


        if (!agentReachedDestination(lastSeenFocusAlarmCharacterPosition)) {

            agent.SetDestination(lastSeenFocusAlarmCharacterPosition);

            agent.isStopped = false;
            animateMovingAgent();
        } else {
            stopAgent();
        }



    }
    /// <summary>
    /// implementazione hostilityAlertBehaviour
    /// </summary>
    public override void hostilityAlertBehaviour() {

        rotateAndAimSubBehaviour();

        if (isFocusAlarmCharacterVisible) {
            characterInventoryManager.useSelectedWeapon();
        }


        if (!agentReachedDestination(lastSeenFocusAlarmCharacterPosition)) {

            agent.SetDestination(lastSeenFocusAlarmCharacterPosition);

            agent.isStopped = false;
            animateMovingAgent();
        } else {
            stopAgent();
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
