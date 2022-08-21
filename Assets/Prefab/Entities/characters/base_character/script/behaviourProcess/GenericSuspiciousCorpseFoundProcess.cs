using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Implementazione del suspiciousCorpseFoundAlertBehaviour
/// Cerca di raggiungere la lastSeenFocusAlarmPosition
/// </summary>
public class GenericSuspiciousCorpseFoundProcess : BehaviourProcess {

    public GenericSuspiciousCorpseFoundProcess(
        Vector3 lastSeenFocusAlarmPosition,
        NavMeshAgent navMeshAgent,
        BaseNPCBehaviourManager baseNPCBehaviour
    ) {
        _lastSeenFocusAlarmPosition = lastSeenFocusAlarmPosition;


        _behaviourAgent = navMeshAgent;
        _baseNPCBehaviour = baseNPCBehaviour;

        processIdName = "generic_suspicious_corpse_found";
    }
    public override async Task runBehaviourAsyncProcess() {

        await base.runBehaviourAsyncProcess();


        _behaviourAgent.updateRotation = true; // ruota il character in base alla direzione da raggiungere

        if (!_baseNPCBehaviour.isAgentReachedDestination(_lastSeenFocusAlarmPosition)) {

            _behaviourAgent.SetDestination(_lastSeenFocusAlarmPosition);

            _behaviourAgent.isStopped = false;
            _baseNPCBehaviour.animateAndSpeedMovingAgent(agentSpeed: AgentSpeed.Run);

        } else {

            
            _baseNPCBehaviour.stopAgent();
            _processTaskFinished = true;
        }
    }
}
