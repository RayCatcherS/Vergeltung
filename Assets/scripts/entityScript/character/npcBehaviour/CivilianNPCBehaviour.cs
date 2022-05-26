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
