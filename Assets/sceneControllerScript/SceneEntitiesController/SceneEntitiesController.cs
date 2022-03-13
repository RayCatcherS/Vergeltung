using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneEntitiesController : MonoBehaviour
{
    public List<BaseNPCBehaviour> allNpcList = new List<BaseNPCBehaviour>();
    public List<EnemyNPCBehaviour> enemyNpcList = new List<EnemyNPCBehaviour>();


    public void addNPCEnemyIstance(EnemyNPCBehaviour enemyNPCBehaviour) {
        allNpcList.Add(enemyNPCBehaviour);
        enemyNpcList.Add(enemyNPCBehaviour);
    }
}
