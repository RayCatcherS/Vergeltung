using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// implementazione suspiciousAlertBehaviour process
/// </summary>
public class GenericSuspiciousProcess : BehaviourProcess {
    
    public GenericSuspiciousProcess(
        Vector3 lastSeenFocusAlarmPosition,
        NavMeshAgent navMeshAgent,
        BaseNPCBehaviourManager baseNPCBehaviour
    ) {
        _lastSeenFocusAlarmPosition = lastSeenFocusAlarmPosition;

        _behaviourAgent = navMeshAgent;
        _baseNPCBehaviour = baseNPCBehaviour;

        processIdName = "generic_suspicious";
    }


    public override async Task runBehaviourAsyncProcess() {
        await base.runBehaviourAsyncProcess();

        _baseNPCBehaviour.rotateAndAimSuspiciousAndHostility(_lastSeenFocusAlarmPosition);

        if (!_baseNPCBehaviour.isAgentReachedDestination(_lastSeenFocusAlarmPosition)) {


            _behaviourAgent.SetDestination(_lastSeenFocusAlarmPosition);

            _behaviourAgent.isStopped = false;
            _baseNPCBehaviour.animateAndSpeedMovingAgent();


        } else {

            _baseNPCBehaviour.stopAgent();

            _processTaskFinished = true;
        }
    }



        
}
