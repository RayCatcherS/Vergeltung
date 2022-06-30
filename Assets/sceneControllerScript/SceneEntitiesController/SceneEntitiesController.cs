using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SceneEntitiesController : MonoBehaviour
{
    [SerializeField] private List<BaseNPCBehaviourManager> _allNpcList = new List<BaseNPCBehaviourManager>();
    public List<BaseNPCBehaviourManager> allNpcList {
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

    [SerializeField] private List<EnemyNPCBehaviourManager> _enemyNpcList = new List<EnemyNPCBehaviourManager>();
    public List<EnemyNPCBehaviourManager> enemyNpcList {
        get { return _enemyNpcList; }
    }
    [SerializeField] private List<CivilianNPCBehaviourManager> civilianNpcList = new List<CivilianNPCBehaviourManager>();

    [SerializeField] private GameObject player;

    public void addNPCEnemyIstance(EnemyNPCBehaviourManager enemyNPCBehaviour) {
        _allNpcList.Add(enemyNPCBehaviour);
        _enemyNpcList.Add(enemyNPCBehaviour);
    }

    public void addNPCCivilianIstance(CivilianNPCBehaviourManager enemyNPCBehaviour) {
        _allNpcList.Add(enemyNPCBehaviour);
        civilianNpcList.Add(enemyNPCBehaviour);
    }

    /// <summary>
    /// Setta il primo character giocabile, il player
    /// </summary>
    /// <param name="playerCharacterManager"></param>
    public void setPlayerEntity(CharacterManager playerCharacterManager) {
        player = playerCharacterManager.gameObject;
    }

    /// <summary>
    /// Questo metodo disattiva i behaviour di tutti i characters
    /// </summary>
    /// <returns></returns>
    public async Task stopAllCharacterBehaviourInSceneAsync() {
        for(int i = 0; i < allNpcList.Count; i++) {

            if(!allNpcList[i].gameObject.GetComponent<CharacterManager>().isDead) {
                await allNpcList[i].forceStopCharacterAndAwaitStopProcess();
                allNpcList[i].stopAgent();
            }
        }
    }

    /// <summary>
    /// Ottieni il character nemico più vicino a [fromPosition]
    /// </summary>
    /// <param name="fromPosition">posizione da cui trovare il character nemico più vicino</param>
    /// <returns>
    /// Restituisce una istanza di CharacterManager se esiste un character nemico vicino che non è morto
    /// Restituisce null se non è disponibile alcun character nemico (esempio: sono tutti morti)
    /// </returns>
    public static EnemyNPCBehaviourManager getCloserEnemyCharacterFromPosition(Vector3 fromPosition, List<EnemyNPCBehaviourManager> enemiesBehv) {
        EnemyNPCBehaviourManager closerEnenemyCharacter = null;

        float closerDisance = 0;
        
        foreach (EnemyNPCBehaviourManager enemy in enemiesBehv) {


            // character non dead o che non si stanno stoppando
            if (!enemy.characterManager.isDead && !enemy.stopCharacterBehaviour && enemy.characterAlertState == CharacterAlertState.Unalert) {
                closerDisance = Vector3.Distance(fromPosition, enemiesBehv[0].gameObject.transform.position);
                closerEnenemyCharacter = enemy;
                break;
            }

        }
       

        foreach (EnemyNPCBehaviourManager enemy in enemiesBehv) {

            // character non dead o che non si stanno stoppando
            if (!enemy.characterManager.isDead && !enemy.stopCharacterBehaviour && enemy.characterAlertState == CharacterAlertState.Unalert) {
                float tDistance = Vector3.Distance(fromPosition, enemy.gameObject.transform.position);
                if (tDistance < closerDisance) {
                    closerDisance = tDistance;
                    closerEnenemyCharacter = enemy;
                }
            }

        }
        

        if(closerEnenemyCharacter == null) {
            return null;
        } else {
            return closerEnenemyCharacter;
        }
    }
    /// <summary>
    /// Ottieni il character più vicino a [fromPosition]
    /// </summary>
    /// <param name="fromPosition">posizione da cui trovare il character più vicino</param>
    /// <returns>
    /// Restituisce una istanza di CharacterManager se esiste un character vicino che non è morto
    /// Restituisce null se non è disponibile alcun character (esempio: sono tutti morti)
    /// </returns>
    public static BaseNPCBehaviourManager getCloserCharacterFromPosition(Vector3 fromPosition, List<BaseNPCBehaviourManager> enemiesBehv) {
        BaseNPCBehaviourManager closerCharacter = null;

        float closerDisance = 0;

        foreach (BaseNPCBehaviourManager character in enemiesBehv) {


            // character non dead o che non si stanno stoppando
            if (!character.characterManager.isDead && !character.stopCharacterBehaviour && character.characterAlertState == CharacterAlertState.Unalert) {
                closerDisance = Vector3.Distance(fromPosition, enemiesBehv[0].gameObject.transform.position);
                closerCharacter = character;
                break;
            }

        }


        foreach (BaseNPCBehaviourManager character in enemiesBehv) {

            // character non dead o che non si stanno stoppando
            if (!character.characterManager.isDead && !character.stopCharacterBehaviour && character.characterAlertState == CharacterAlertState.Unalert) {
                float tDistance = Vector3.Distance(fromPosition, character.gameObject.transform.position);
                if (tDistance < closerDisance) {
                    closerDisance = tDistance;
                    closerCharacter = character;
                }
            }

        }


        if (closerCharacter == null) {
            return null;
        } else {
            return closerCharacter;
        }
    }
}
