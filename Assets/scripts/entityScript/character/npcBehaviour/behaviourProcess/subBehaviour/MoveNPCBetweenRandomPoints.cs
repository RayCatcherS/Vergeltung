using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


/// Questa classe gestisce il subBehaviour generando punti casuali che il 
/// character deve raggiungere simulando la ricerca della causa dello stato di allarme
public class MoveNPCBetweenRandomPoints {
    private BaseNPCBehaviour _baseNPCBehaviour;
    private Vector3 _originPosition;
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

    // global states
    private bool _isFinished = false;
    public bool isFinished {
        get { return _isFinished; }
    }
    public bool _isStarted = false;
    public bool isStarted {
        get { return _isStarted; }
    }

    public MoveNPCBetweenRandomPoints(BaseNPCBehaviour baseNPCBehaviour, Vector3 originPosition, float areaRadius, int sampleToReach, float waitingOnPointTime) {

        _baseNPCBehaviour = baseNPCBehaviour;
        _originPosition = originPosition;
        _areaRadius = areaRadius;
        _sampleToReach = sampleToReach;
        _waitingOnPointTime = waitingOnPointTime;

        generateTargetNavMeshPoints();
        _isStarted = true;
    }

    private void generateTargetNavMeshPoints() {
        _randomNavMeshPositions = new List<Vector3>();

        // aggiungi prima posizione == a quello dell'origin
        /*UnityEngine.AI.NavMeshHit firstHit;
        if (UnityEngine.AI.NavMesh.SamplePosition(_originPosition, out firstHit, _areaRadius, UnityEngine.AI.NavMesh.AllAreas)) {
            _randomNavMeshPositions.Add(firstHit.position);
        }*/
        

        for (int i = 0; i < _sampleToReach; i++) {

            Vector3 randomPos = new Vector3(Random.insideUnitCircle.x, 0, Random.insideUnitCircle.y);
            Vector3 randomPoint = _originPosition + randomPos * _areaRadius;

            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(randomPoint, out hit, _areaRadius, UnityEngine.AI.NavMesh.AllAreas)) {

                _randomNavMeshPositions.Add(hit.position);

            }
        }
    }


    public void runBehaviour() {
        
        
        _baseNPCBehaviour.agent.updateRotation = true;

        if (agentDestinationSetted == false) {

            updateUnalertAgentTarget();

            agentDestinationSetted = true;
        } else {


            // fin quando non è raggiunta la posizione 
            if(!_baseNPCBehaviour.isAgentReachedDestination(_randomNavMeshPositions[_selectedPosition])) {

                _baseNPCBehaviour.agent.isStopped = false;
                _baseNPCBehaviour.animateAndSpeedMovingAgent();
                _isPointReached = false;

                updateUnalertAgentTarget();

                Debug.Log(1);
            } else {

                if(_isPointReached == false) {

                    // start waiting loop
                    waitingOnANavMeshPointTimerLoop();
                    _isPointReached = true;
                } else {

                    // wait waiting loop
                    if(isWaitingOnANavMeshPoint) {
                        Debug.Log(2);
                        _baseNPCBehaviour.stopAgent();
                    } else {
                        Debug.Log(3);
                        _isPointReached = false;
                        setNextPosition();
                        agentDestinationSetted = false;
                    }
                }
            }
        }

        if(_selectedPosition == _randomNavMeshPositions.Count -1) {
            _isFinished = true;
        }

    }

    private void updateUnalertAgentTarget() {

        
        if (!_baseNPCBehaviour.characterManager.isDead) {

            _baseNPCBehaviour.agent.SetDestination(
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
        

        if (_selectedPosition <= _randomNavMeshPositions.Count - 1) {

            _selectedPosition = _selectedPosition + 1;
        } else {
            _isFinished = true;
        }
    }
}
