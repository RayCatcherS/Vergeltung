using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class CorpseFoundConfirmedEnemyProcess : BehaviourProcess {

    private CharacterMovement _characterMovement;

    public CorpseFoundConfirmedEnemyProcess(
        Vector3 lastSeenFocusAlarmPosition,
        NavMeshAgent behaviourAgent,
        BaseNPCBehaviourManager baseNPCBehaviour,
        CharacterMovement characterMovement
    ) {
        _lastSeenFocusAlarmPosition = lastSeenFocusAlarmPosition;

        _behaviourAgent = behaviourAgent;
        _baseNPCBehaviour = baseNPCBehaviour;
        _characterMovement = characterMovement;

        processIdName = "corpse_found_confirmed_enemy_process";
    }



    public override async Task runBehaviourAsyncProcess() {
        await base .runBehaviourAsyncProcess();

        _behaviourAgent.updateRotation = true; // ruota il character in base alla direzione da raggiungere

        if (!_baseNPCBehaviour.isAgentReachedDestination(_lastSeenFocusAlarmPosition)) {

            _behaviourAgent.updateRotation = true;
            _behaviourAgent.SetDestination(_lastSeenFocusAlarmPosition);

            _behaviourAgent.isStopped = false;
            _baseNPCBehaviour.animateAndSpeedMovingAgent(agentSpeed: AgentSpeed.Run);
        } else {


            Vector3 targetDirection = _lastSeenFocusAlarmPosition - _baseNPCBehaviour.transform.position;
            _characterMovement.rotateCharacter(targetDirection, false, rotationLerpSpeedValue: RotationLerpSpeedValue.fast);
            _baseNPCBehaviour.stopAgent();

            _processTaskFinished = true;
        }
    }
}
