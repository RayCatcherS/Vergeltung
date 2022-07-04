using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class WarnOfSospiciousEnemyProcess : BehaviourProcess {

    public WarnOfSospiciousEnemyProcess(
        Vector3 lastSeenFocusAlarmPosition,
        NavMeshAgent behaviourAgent,
        BaseNPCBehaviourManager baseNPCBehaviour
    ) {
        _lastSeenFocusAlarmPosition = lastSeenFocusAlarmPosition;

        _baseNPCBehaviour = baseNPCBehaviour;
        _behaviourAgent = behaviourAgent;

        processIdName = "warn_of_sospicious_enemy_process";
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
