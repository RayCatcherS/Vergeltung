using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CivilianNPCBehaviour : BaseNPCBehaviour {
    static public GameObject addToGOCivilianNPCComponent(GameObject gameObject, CharacterSpawnPoint spwanPoint) {
        gameObject.AddComponent<CivilianNPCBehaviour>();

        CivilianNPCBehaviour enemyNPCNewComponent = gameObject.GetComponent<CivilianNPCBehaviour>();
        enemyNPCNewComponent.initCivilianNPCComponent(spwanPoint);

        return gameObject;
    }

    public void initCivilianNPCComponent(CharacterSpawnPoint spawnPoint) {
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
