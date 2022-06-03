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
