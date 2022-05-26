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
    /// implementazione comportamento di allerta 1 
    /// </summary>
    public override void alertBehaviour1() {
        stopAgent();
    }
    /// <summary>
    /// implementazione comportamento di allerta 2
    /// </summary>
    public override void alertBehaviour2() {
        stopAgent();
    }
    /// <summary>
    /// implementazione comportamento di allerta 3
    /// </summary>
    public override void alertBehaviour3() {

    }
}
