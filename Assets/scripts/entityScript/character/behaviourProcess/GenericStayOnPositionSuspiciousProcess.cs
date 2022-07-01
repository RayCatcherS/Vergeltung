using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class GenericStayOnPositionSuspiciousProcess : BehaviourProcess {
    public GenericStayOnPositionSuspiciousProcess(
        NavMeshAgent navMeshAgent,
        BaseNPCBehaviourManager baseNPCBehaviour
    ) {
        _behaviourAgent = navMeshAgent;
        _baseNPCBehaviour = baseNPCBehaviour;

        processIdName = "Generic_stay_on_position_suspicious";
    }

    public override async Task runBehaviourAsyncProcess() {
        await base.runBehaviourAsyncProcess();

        _baseNPCBehaviour.stopAgent();

        if(_baseNPCBehaviour.isFocusedAlarmCharacter) {

            Vector3 targetDirection;

            if(_baseNPCBehaviour.isFocusAlarmCharacterVisible) {
                
                
                targetDirection = _baseNPCBehaviour.focusAlarmCharacter.transform.position - _baseNPCBehaviour.gameObject.transform.position;

                _baseNPCBehaviour.lastSeenFocusAlarmPosition = _baseNPCBehaviour.focusAlarmCharacter.transform.position; // update lastSeen

            } else {
                targetDirection = _baseNPCBehaviour.lastSeenFocusAlarmPosition - _baseNPCBehaviour.gameObject.transform.position;
                
            }
            _baseNPCBehaviour.characterMovement.rotateCharacter(new Vector2(targetDirection.x, targetDirection.z), false, rotationLerpSpeedValue: RotationLerpSpeedValue.fast);

            
        }

        _processTaskFinished = true;



    }
}
