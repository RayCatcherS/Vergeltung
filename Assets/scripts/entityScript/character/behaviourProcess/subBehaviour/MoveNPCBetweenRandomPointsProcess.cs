using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

/// Questa classe gestisce il subBehaviour generando punti casuali che il 
/// character deve raggiungere simulando la ricerca della causa dello stato di allarme
public class MoveNPCBetweenRandomPointsProcess : BehaviourProcess {
    
    private float _areaRadius;
    private int _sampleToReach;
    private float _waitingOnPointTime = 1.5f;

    private bool _isPointReached = false;
    private int _selectedPosition = 0;
    private List<Vector3> _randomNavMeshPositions = new List<Vector3>();
    public List<Vector3> randomNavMeshPositions {
        get { return _randomNavMeshPositions; }
    }

    private bool isWaitingOnANavMeshPoint = false;


    // navigation states
    private bool agentDestinationSetted = false;

    // ref
    CharacterManager _characterManager;
    

    public MoveNPCBetweenRandomPointsProcess(
        UnityEngine.AI.NavMeshAgent behaviourAgent,
        BaseNPCBehaviourManager baseNPCBehaviour,
        CharacterManager CharacterManager,
        float areaRadius = 7,
        int sampleToReach = 4,
        float waitingOnPointTime = 1
    ) {
        _behaviourAgent = behaviourAgent;
        _baseNPCBehaviour = baseNPCBehaviour;
        _characterManager = CharacterManager;

        _areaRadius = areaRadius;
        _sampleToReach = sampleToReach;
        _waitingOnPointTime = waitingOnPointTime;
    }

    public override void initBehaviourProcess() {
        _randomNavMeshPositions = new List<Vector3>();

        // aggiungi prima posizione == a quello dell'origin
        /*UnityEngine.AI.NavMeshHit firstHit;
        if (UnityEngine.AI.NavMesh.SamplePosition(_originPosition, out firstHit, _areaRadius, UnityEngine.AI.NavMesh.AllAreas)) {
            _randomNavMeshPositions.Add(firstHit.position);
        }*/


        while (_randomNavMeshPositions.Count < _sampleToReach) {
            Vector3 randomPos = new Vector3(Random.insideUnitCircle.x, 0, Random.insideUnitCircle.y);
            Vector3 randomPoint = _characterManager.getCharacterPositionReachebleByAgents() + randomPos * _areaRadius;

            NavMeshPath navMeshPath = new NavMeshPath();
            UnityEngine.AI.NavMeshHit hit;

            if (UnityEngine.AI.NavMesh.SamplePosition(randomPoint, out hit, _areaRadius, UnityEngine.AI.NavMesh.AllAreas)) {

                if (_behaviourAgent.CalculatePath(hit.position, navMeshPath) && navMeshPath.status == NavMeshPathStatus.PathComplete) {
                    _randomNavMeshPositions.Add(hit.position);
                }

            }
        }

        Debug.Log(_randomNavMeshPositions.Count);
    }


    public override async Task runBehaviourAsyncProcess() {

        if(!_processTaskFinished) {
            _behaviourAgent.updateRotation = true;

            if (agentDestinationSetted == false) {

                updateUnalertAgentTarget();

                agentDestinationSetted = true;
            } else {


                // fin quando non è raggiunta la posizione 
                if (!_baseNPCBehaviour.isAgentReachedDestination(_randomNavMeshPositions[_selectedPosition])) {

                    _behaviourAgent.isStopped = false;
                    _baseNPCBehaviour.animateAndSpeedMovingAgent();
                    _isPointReached = false;

                    updateUnalertAgentTarget();

                } else {

                    if (_isPointReached == false) {

                        // start waiting loop
                        waitingOnANavMeshPointTimerLoop();
                        _isPointReached = true;
                    } else {

                        // wait waiting loop
                        if (isWaitingOnANavMeshPoint) {

                            _baseNPCBehaviour.stopAgent();
                        } else {
                            setNextPosition();
                            _isPointReached = false;
                            agentDestinationSetted = false;
                        }
                    }
                }
            }
        } else {
            _baseNPCBehaviour.stopAgent();
        }
    }

    private void updateUnalertAgentTarget() {

        
        if (!_baseNPCBehaviour.characterManager.isDead) {

            _behaviourAgent.SetDestination(
                _randomNavMeshPositions[_selectedPosition]

            );
        }

    }

    private async void waitingOnANavMeshPointTimerLoop() {

        
        float endTime = Time.time + _waitingOnPointTime;

        isWaitingOnANavMeshPoint = true;

        while (Time.time < endTime) {

            await Task.Yield();
        }

        isWaitingOnANavMeshPoint = false;
    }
    

    void setNextPosition() {
        

        if (_selectedPosition < _randomNavMeshPositions.Count - 1) {

            _selectedPosition = _selectedPosition + 1;
        } else {
            _processTaskFinished = true;
        }
    }
}
