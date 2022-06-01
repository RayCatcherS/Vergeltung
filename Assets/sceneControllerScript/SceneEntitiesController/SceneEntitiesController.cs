using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneEntitiesController : MonoBehaviour
{
    [SerializeField] private List<BaseNPCBehaviour> _allNpcList = new List<BaseNPCBehaviour>();
    public List<BaseNPCBehaviour> allNpcList {
        get { return _allNpcList; }
    }
    public List<CharacterManager> getAllNPC() {
        List<CharacterManager> characterManagers = new List<CharacterManager>();

        for(int i = 0; i < _allNpcList.Count; i++) {

            characterManagers.Add(
                _allNpcList[i].gameObject.GetComponent<CharacterManager>()
            );
        }

        return characterManagers;
    }

    [SerializeField] private List<EnemyNPCBehaviour> enemyNpcList = new List<EnemyNPCBehaviour>();
    [SerializeField] private List<CivilianNPCBehaviour> civilianNpcList = new List<CivilianNPCBehaviour>();

    [SerializeField] private GameObject player;

    public void addNPCEnemyIstance(EnemyNPCBehaviour enemyNPCBehaviour) {
        _allNpcList.Add(enemyNPCBehaviour);
        enemyNpcList.Add(enemyNPCBehaviour);
    }

    public void addNPCCivilianIstance(CivilianNPCBehaviour enemyNPCBehaviour) {
        _allNpcList.Add(enemyNPCBehaviour);
        civilianNpcList.Add(enemyNPCBehaviour);
    }

    public void setPlayerEntity(CharacterManager playerCharacterManager) {
        player = playerCharacterManager.gameObject;
    }
}
