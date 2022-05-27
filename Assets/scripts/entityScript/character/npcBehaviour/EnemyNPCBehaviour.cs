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
        stopAgent();
    }
    /// <summary>
    /// implementazione hostilityAlertBehaviour
    /// </summary>
    public override void hostilityAlertBehaviour() {
        stopAgent();
    }
    /// <summary>
    /// implementazione soundAlert1Behaviour
    /// </summary>
    public override void soundAlert1Behaviour() {

    }
}
