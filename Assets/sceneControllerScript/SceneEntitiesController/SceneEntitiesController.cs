using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneEntitiesController : MonoBehaviour
{
    [SerializeField] private List<BaseNPCBehaviour> _allNpcList = new List<BaseNPCBehaviour>();
    public List<BaseNPCBehaviour> allNpcList {
        get { return _allNpcList; }
    }
    [SerializeField] private List<EnemyNPCBehaviour> enemyNpcList = new List<EnemyNPCBehaviour>();
    [SerializeField] private List<EnemyNPCBehaviour> civilianNpcList = new List<EnemyNPCBehaviour>();

    [SerializeField] private GameObject player;

    public void addNPCEnemyIstance(EnemyNPCBehaviour enemyNPCBehaviour) {
        _allNpcList.Add(enemyNPCBehaviour);
        enemyNpcList.Add(enemyNPCBehaviour);
    }

    public void addNPCCivilianIstance(EnemyNPCBehaviour enemyNPCBehaviour) {
        _allNpcList.Add(enemyNPCBehaviour);
        civilianNpcList.Add(enemyNPCBehaviour);
    }

    public void setPlayerEntity(CharacterManager playerCharacterManager) {
        player = playerCharacterManager.gameObject;
    }
}
