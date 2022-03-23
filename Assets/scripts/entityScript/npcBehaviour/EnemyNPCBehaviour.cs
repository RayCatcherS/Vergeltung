using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyNPCBehaviour : BaseNPCBehaviour {

    static public GameObject addToGOEnemyNPComponent(GameObject gameObject, CharacterSpawnPoint spwanPoint) {
        gameObject.AddComponent<EnemyNPCBehaviour>();

        EnemyNPCBehaviour enemyNPCNewComponent = gameObject.GetComponent<EnemyNPCBehaviour>();
        enemyNPCNewComponent.initEnemyNPCComponent(spwanPoint);

        return gameObject;
    }

    public void initEnemyNPCComponent(CharacterSpawnPoint spawnPoint) {
        this.spawnPoint = spawnPoint;
    }


    /// <summary>
    /// implementazione comportamento di allerta 1 
    /// </summary>
    public override void alertBehaviour1() {

    }
    /// <summary>
    /// implementazione comportamento di allerta 2
    /// </summary>
    public override void alertBehaviour2() {

    }
    /// <summary>
    /// implementazione comportamento di allerta 3
    /// </summary>
    public override void alertBehaviour3() {

    }
}
