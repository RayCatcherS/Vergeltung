using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SceneEntitiesController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameSoundtrackController gameSoundtrackController;

    [Header("Scene character entities")]
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

    [SerializeField] private GameObject _player;
    public GameObject player {
        get { return _player; }
    }

    /// <summary>
    /// Dizionario characters hostility characters
    /// Contiene i characters attualmente in stato di Hostility e Suspicious
    /// </summary>
    private Dictionary<CharacterManager, CharacterBehaviourSoundtrackState> _charactersHostilitySouspiciousD = new Dictionary<CharacterManager, CharacterBehaviourSoundtrackState>();


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
        _player = playerCharacterManager.gameObject;
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


    /// <summary>
    /// Aggiungi CharacterManagerInstanceID e stato di allerta
    /// </summary>
    public void addCharacterInstanceAndAlertStateToDictionary(CharacterManager character, CharacterBehaviourSoundtrackState state) {

        _charactersHostilitySouspiciousD.TryAdd(character, state);


        updateGlobalAlarmState();
    }

    public void removeCharacterInstanceAndAlertStateToDictionary(CharacterManager character) {


        if(_charactersHostilitySouspiciousD.ContainsKey(character)) {
            _charactersHostilitySouspiciousD.Remove(character);

            updateGlobalAlarmState();
        }

        
    }

    public void updateGlobalAlarmState() {

        int civilianSuspicious = 0;
        int civilianHostility = 0;
        int enemySuspicious = 0;
        int enemyHostility = 0;

        foreach (var character in _charactersHostilitySouspiciousD) {


            if(character.Key.chracterRole == Role.Civilian) {

                if(character.Value == CharacterBehaviourSoundtrackState.Suspicious) {
                    civilianSuspicious++;
                }

                if(character.Value == CharacterBehaviourSoundtrackState.Hostility) {
                    civilianHostility++;
                }
            }

            if(character.Key.chracterRole == Role.EnemyGuard) {
                if(character.Value == CharacterBehaviourSoundtrackState.Suspicious) {
                    enemySuspicious++;
                }

                if(character.Value == CharacterBehaviourSoundtrackState.Hostility) {
                    enemyHostility++;
                }
            }
        }

        if(civilianSuspicious == 0 &&
               civilianHostility == 0 &&
               enemySuspicious == 0 &&
               enemyHostility == 0
            ) {

            gameSoundtrackController
                .setSoundTrackState(CharacterBehaviourSoundtrackState.Unalert);
        } else {

            // Se nel dizionario esistono solo character civili
            // in stato di suspicious oppure hostility e nessun character
            // nemico(in suspicious o hostility), il trigger dell'animator
            // verrà settato su suspicious
            if(enemySuspicious == 0 && enemyHostility == 0) {
                if(civilianSuspicious != 0 || civilianHostility != 0) {
                    gameSoundtrackController
                        .setSoundTrackState(CharacterBehaviourSoundtrackState.Suspicious);
                }

            } else {
                if(enemyHostility != 0) {
                    gameSoundtrackController
                            .setSoundTrackState(CharacterBehaviourSoundtrackState.Hostility);
                } else if(enemySuspicious != 0) {
                    gameSoundtrackController
                            .setSoundTrackState(CharacterBehaviourSoundtrackState.Suspicious);
                }

                
            }

        }

#if UNITY_EDITOR
        _civilianSuspicious = civilianSuspicious;
        _civilianHostility = civilianHostility;
        _enemySuspicious = enemySuspicious;
        _enemyHostility = enemyHostility;
#endif
    }

    public async Task stopAllCharacterTargetIcon() {

        _player.GetComponent<TargetIconManager>().disableTargetUI();

        foreach(var character in allNpcList) {
            character.gameObject.GetComponent<TargetIconManager>().disableTargetUI();
        }
        return;
    }
#if UNITY_EDITOR
    
    private int _civilianSuspicious = 0;
    private int _civilianHostility = 0;
    private int _enemySuspicious = 0;
    private int _enemyHostility = 0;


    void OnGUI() {
        GUI.TextArea(new Rect(0, Screen.height - 100, 200, 100), "GLOBAL ALERT: \n"
            + "C Civili suspicious: " + _civilianSuspicious.ToString() + "\n"
            + "C Civili hostility: " + _civilianHostility.ToString() + "\n"
            + "C Nemici suspicious: " + _enemySuspicious.ToString() + "\n"
            + "C Nemici hostility: " + _enemyHostility.ToString() + "\n"
            + "GSoundTrackState: " + gameSoundtrackController.soundTrackState.ToString()
            , 200);
        /*
        GUI.TextArea(new Rect(0, 100, 200, 40), "rotazione character \n" + characterMovement.getCharacterModelRotation.ToString(), 200);
        GUI.TextArea(new Rect(0, 140, 200, 40), "input is run: \n" + isRunPressed.ToString(), 200);*/
    }
#endif
}


