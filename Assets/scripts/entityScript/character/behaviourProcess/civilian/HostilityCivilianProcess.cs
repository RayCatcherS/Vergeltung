using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class HostilityCivilianProcess : BehaviourProcess {

    protected bool _checkedByHimselfHostility = false;
    private CharacterFOV _characterFOV;
    private CharacterManager _characterManager;

    public HostilityCivilianProcess(
        Vector3 lastSeenFocusAlarmPosition,
        NavMeshAgent navMeshAgent,
        BaseNPCBehaviourManager baseNPCBehaviour,
        CharacterFOV characterFOV,
        CharacterManager characterManager,

        bool checkedByHimselfHostility
    ) {
        _lastSeenFocusAlarmPosition = lastSeenFocusAlarmPosition;

        _behaviourAgent = navMeshAgent;
        _baseNPCBehaviour = baseNPCBehaviour;
        _characterFOV = characterFOV;
        _characterManager = characterManager;


        _checkedByHimselfHostility = checkedByHimselfHostility;
        processIdName = "hostility_civilian";
        initBehaviourProcess();
    }

    private EnemyNPCBehaviourManager closerEnemyCharacterToWarn = null;
    private bool closerEnemyCharacterToWarnSelected {
        get {
            if (closerEnemyCharacterToWarn == null) {
                return false;
            } else {
                return true;
            }
        }
    }
    private bool isEnemyCharacterToWarnCalled = false; // se è già stata avvisata una guardia dell'hostilità

    public override async Task runBehaviourAsyncProcess() {
        await base.runBehaviourAsyncProcess();

        if (closerEnemyCharacterToWarnSelected && !isEnemyCharacterToWarnCalled) {


            if (!_baseNPCBehaviour.isAgentReachedEnemyCharacterToWarnDestination(closerEnemyCharacterToWarn.transform.position)) {

                _behaviourAgent.updateRotation = true;
                _behaviourAgent.SetDestination(
                    closerEnemyCharacterToWarn.transform.position
                );

                _behaviourAgent.isStopped = false;
                _baseNPCBehaviour.animateAndSpeedMovingAgent(agentSpeed: AgentSpeed.Run);
            } else {

                if (!isEnemyCharacterToWarnCalled) {



                    bool isCharacterToNotifyPossibleToSee = _characterFOV.canCharacterReachableBy(
                        closerEnemyCharacterToWarn.characterFOV
                    );


                    if (isCharacterToNotifyPossibleToSee) {


                        closerEnemyCharacterToWarn.warnOfSouspiciousCheck(_lastSeenFocusAlarmPosition);
                        isEnemyCharacterToWarnCalled = true;
                        _processTaskFinished = true;

                    } else { // impossibile raggiungere il closer enemy character

                        throw new System.ApplicationException("enemyCharacterImpossibleToReach");
                    }


                }

            }
        } else {
            _processTaskFinished = true;

            _baseNPCBehaviour.rotateAndAimSuspiciousAndHostility(_lastSeenFocusAlarmPosition);
            _baseNPCBehaviour.stopAgent();

        }
    }

    /// <summary>
    /// Inizializza behaviour
    /// </summary>
    public override void initBehaviourProcess() {

        // se ha scoperto da solo il character hostile (tramite il suo stesso fov)
        if (_checkedByHimselfHostility) {
            isEnemyCharacterToWarnCalled = false;
            // get the closer character
            closerEnemyCharacterToWarn =
                SceneEntitiesController.getCloserEnemyCharacterFromPosition(
                    _characterManager.transform.position,
                    _characterManager.sceneEntitiesController.enemyNpcList
                );

        }
    }
}
