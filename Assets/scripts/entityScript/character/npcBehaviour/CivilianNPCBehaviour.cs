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


    }
}
