using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public enum LoudAreaType {
    nothing = 0,
    low = 5,
    medium = 40,
    high = 60,
    characterLoudRun = 4
}

public class LoudArea : MonoBehaviour
{


    [Header("References")]
    [SerializeField] private AudioSource _audioSource; // audio sorce suono generato
    CharacterManager _characterThatGenerateLA;

    [Header("Config")]
    [SerializeField] private LayerMask targetCharacterMask;
    [SerializeField] private float _nearLoudAreaRange = 13; // area in cui viene generato casualmente un punto da raggiungere
    [SerializeField] private int _numberOfCharactersToCall = 2;

    // variabili non configurabili
    private float _loudAreaRadius = 13;
    private LoudAreaType _intensity;

    public void initLoudArea(LoudAreaType intensity, AudioClip clip = null, CharacterManager characterThatGenerateLA = null) {


        if(clip != null) {
            _audioSource.clip = clip;
        }

        _loudAreaRadius = (float)intensity;
        _intensity = intensity;

        if(characterThatGenerateLA != null) {
            _characterThatGenerateLA = characterThatGenerateLA;
        }
        
    }

    public void startLoudArea() {

        if(_audioSource.clip != null) {
            _audioSource.Play();
        }

        if(_intensity != LoudAreaType.nothing) {

            manageCharacterAlarms();
        }
    }

    private async void manageCharacterAlarms() {
        Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, _loudAreaRadius, targetCharacterMask);
        List<EnemyNPCBehaviourManager> enemyCharacters = new List<EnemyNPCBehaviourManager>();
        List<CivilianNPCBehaviourManager> civilianCharacters = new List<CivilianNPCBehaviourManager>();
        List<BaseNPCBehaviourManager> allCharacters = new List<BaseNPCBehaviourManager>();

        if (hitColliders.Length != 0) {

            
            foreach (Collider collider in hitColliders) {


                // ottenimento character manager
                CharacterManager character = collider.gameObject.GetComponent<CharacterManager>();

                if (!character.isPlayer && !character.isDead && !character.isStackControlled) { // solo characters che non sono il player e che non sono morti e non controllati


                    // ottenimento ruolo character
                    Role characterRole = collider.gameObject.GetComponent<CharacterRole>().role;

                    // ottenimento behaviour
                    
                    if (characterRole == Role.EnemyGuard) {
                        EnemyNPCBehaviourManager characterBehaviour = null;
                        characterBehaviour = collider.gameObject.GetComponent<EnemyNPCBehaviourManager>();
                        if (characterBehaviour != null) {


                            enemyCharacters.Add(characterBehaviour);
                            allCharacters.Add(characterBehaviour);
                        }
                        
                    } else if (characterRole == Role.Civilian) {
                        CivilianNPCBehaviourManager characterBehaviour = null;
                        characterBehaviour = collider.gameObject.GetComponent<CivilianNPCBehaviourManager>();
                        if (characterBehaviour != null) {

                            civilianCharacters.Add(characterBehaviour);
                            allCharacters.Add(characterBehaviour);
                        }
                    }


                    
                }

                
            }

            // se l'intensità del rumore è bassa vengono allertati tutti i character che sono nell'area
            if (_intensity == LoudAreaType.low) {

                foreach(BaseNPCBehaviourManager characters in allCharacters) {
                    characters.instantOnCurrentPositionWarnOfSouspiciousCheck(gameObject.transform.position);
                }


            } else if(_intensity == LoudAreaType.characterLoudRun) { // loud area generata dalla corsa di un character

                if(_characterThatGenerateLA != null) {
                    foreach(BaseNPCBehaviourManager character in allCharacters) {
                        if(character.characterFOV.canCharacterReachableBy(positionToReach: gameObject.transform.position)) {
                                character.playerLoudRunSuspiciousCheck(
                                _characterThatGenerateLA,
                                gameObject.transform.position
                            );
                        }
                        
                    }
                }

            } else if(_intensity == LoudAreaType.medium || _intensity == LoudAreaType.high) {


                // per il numero di guardie da chiamare
                for (int i = 0; i < _numberOfCharactersToCall; i++) {
                    
                    // get bheaviour agent
                    NavMeshAgent agent;

                    if(enemyCharacters.Count != 0) {

                        // seleziona la guardia più vicina alla loud zone
                        EnemyNPCBehaviourManager closerEnemyCharacters = SceneEntitiesController.
                        getCloserEnemyCharacterFromPosition(
                            gameObject.transform.position,
                            enemyCharacters
                        );


                        // manda in warn of souspicious il character verso la loudTargetSourcePoint
                        if (closerEnemyCharacters != null) {

                            agent = closerEnemyCharacters.agent;

                            // genera punto casuale vicino alla fonte della loud area
                            Vector3 loudTargetSourcePoint = await getNearPositionLoudAreaSource(agent);

                            closerEnemyCharacters.warnOfSouspiciousCheck(loudTargetSourcePoint);
                        }
                    }
                }
            }
        }


        // distruggi solo quando è terminata la riproduzione del suono
        while(_audioSource.isPlaying) {

            await Task.Yield();
        }

        destroyLoudArea();
    }


    /// <summary>
    /// Genera una posizione casuale vicino alla loud area
    /// in modo da simulare il raggiungere una posizione vicina ad un forte rumore.
    /// 
    /// La posizione viene generata in modo asincrono perchè potrebbe richiedere più iterazioni
    /// </summary>
    /// <param name="agent"> agente che deve raggiungere la loud area</param>
    /// <returns></returns>
    private async Task<Vector3> getNearPositionLoudAreaSource(NavMeshAgent agent) {
        Vector3 nearSourceValue = Vector3.zero;


        

        
        NavMeshPath navMeshPath = new NavMeshPath();
        NavMeshHit hit;

        int attempts = 0;
        while (nearSourceValue == Vector3.zero) {

            Vector3 randomPos = new Vector3(Random.insideUnitCircle.x, 0, Random.insideUnitCircle.y);
            Vector3 randomPoint = gameObject.transform.position + randomPos * _nearLoudAreaRange;

            await Task.Yield();

            if (NavMesh.SamplePosition(randomPoint, out hit, 5, NavMesh.AllAreas)) {

                if (agent.CalculatePath(hit.position, navMeshPath) && navMeshPath.status == NavMeshPathStatus.PathComplete) {
                    nearSourceValue = hit.position;


#if UNITY_EDITOR

                    loudPositionsToReach.Add(nearSourceValue);
#endif
                    
                } else {
                    // Debug.Log("attemp fail");
                }
            }

            attempts++;
            if (attempts > 10) {
                Debug.LogError("Loud area CalculatePath ATTEMPS OUT");
            }
        }

        return nearSourceValue;
    }

    private void destroyLoudArea() {
        Destroy(gameObject);
    }



#if UNITY_EDITOR

    List<Vector3> loudPositionsToReach = new List<Vector3>();
    void OnDrawGizmos() {
        Handles.color = Color.red;
        Handles.DrawWireDisc(gameObject.transform.position, Vector3.up, (float)_intensity); //debug radius

        if(loudPositionsToReach.Count != 0) {

            foreach(Vector3 position in loudPositionsToReach) {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(position, 0.25f);
            }

            
        }
        
    }
#endif
}
