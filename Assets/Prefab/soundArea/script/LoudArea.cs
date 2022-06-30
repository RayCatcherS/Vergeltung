using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public enum LoudAreaIntensity {
    nothing = 0,
    low = 5,
    medium = 45,
    high = 70
}

public class LoudArea : MonoBehaviour
{


    [Header("References")]
    [SerializeField] private AudioSource _audioSource; // audio sorce suono generato

    [Header("Config")]
    [SerializeField] private LayerMask targetCharacterMask;
    [SerializeField] private float _nearLoudArea = 4; // area in cui viene generato casualmente un punto da raggiungere
    [SerializeField] private int _numberOfCharactersToCall = 2;

    // variabili non configurabili
    private float _loudAreaRadius = 13;
    private LoudAreaIntensity _intensity;

    public void initLoudArea(LoudAreaIntensity intensity, AudioClip clip = null) {


        if(clip != null) {
            _audioSource.clip = clip;
        }

        _loudAreaRadius = (float)intensity;
        _intensity = intensity;
    }

    public void startLoudArea() {

        if(_audioSource.clip != null) {
            _audioSource.Play();
        }

        if(_intensity != LoudAreaIntensity.nothing) {

            manageCharacterAlarms();
        }
    }

    private async void manageCharacterAlarms() {
        Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, _loudAreaRadius, targetCharacterMask);

        if (hitColliders.Length != 0) {

            int cCount = 0;
            foreach (Collider collider in hitColliders) {


                // ottenimento character manager
                CharacterManager character = collider.gameObject.GetComponent<CharacterManager>();

                if (!character.isPlayer && !character.isDead) { // solo characters che non sono il player e che non sono morti


                    // ottenimento ruolo character
                    Role characterRole = collider.gameObject.GetComponent<CharacterRole>().role;

                    // ottenimento behaviour
                    BaseNPCBehaviourManager characterBehaviour = null;
                    if (characterRole == Role.Civilian) {
                        characterBehaviour = collider.gameObject.GetComponent<CivilianNPCBehaviourManager>();
                    } else if (characterRole == Role.EnemyGuard) {

                        characterBehaviour = collider.gameObject.GetComponent<EnemyNPCBehaviourManager>();
                    }
                     

                    if(characterBehaviour != null) {
                        if (_intensity == LoudAreaIntensity.low) {
                            characterBehaviour.instantOnCurrentPositionWarnOfSouspiciousCheck();

                        } else if (_intensity == LoudAreaIntensity.medium || _intensity == LoudAreaIntensity.high) {

                            // get bheaviour agent
                            NavMeshAgent agent = characterBehaviour.agent;

                            Vector3 loudTargetSourcePoint = await getNearLoudAreaSource(agent);
                            characterBehaviour.warnOfSouspiciousCheck(loudTargetSourcePoint);
                        }
                    }
                }

                cCount++;
                if(cCount == _numberOfCharactersToCall) {
                    break;
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
    private async Task<Vector3> getNearLoudAreaSource(NavMeshAgent agent) {
        Vector3 nearSourceValue = Vector3.zero;


        

        
        NavMeshPath navMeshPath = new NavMeshPath();
        NavMeshHit hit;

        int attempts = 0;
        while (nearSourceValue == Vector3.zero) {

            Vector3 randomPos = new Vector3(Random.insideUnitCircle.x, 0, Random.insideUnitCircle.y);
            Vector3 randomPoint = gameObject.transform.position + randomPos * _nearLoudArea;

            await Task.Yield();

            if (NavMesh.SamplePosition(randomPoint, out hit, 5, NavMesh.AllAreas)) {

                if (agent.CalculatePath(hit.position, navMeshPath) && navMeshPath.status == NavMeshPathStatus.PathComplete) {
                    nearSourceValue = hit.position;


#if UNITY_EDITOR

                    loudPositionsToReach.Add(nearSourceValue);
#endif
                    
                } else {
                    Debug.Log("attemp fail");
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
