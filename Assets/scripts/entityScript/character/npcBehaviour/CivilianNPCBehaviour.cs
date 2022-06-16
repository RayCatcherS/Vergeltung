using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CivilianNPCBehaviour : BaseNPCBehaviour {
    static public GameObject initCivilianNPCComponent(GameObject gameObject, CharacterSpawnPoint spawnPoint) {

        CivilianNPCBehaviour enemyNPCNewComponent = gameObject.GetComponent<CivilianNPCBehaviour>();
        enemyNPCNewComponent.initNPCComponent(spawnPoint, gameObject.GetComponent<CharacterMovement>());

        return gameObject;
    }

    [SerializeField] private EnemyNPCBehaviour closerEnemyCharacterToWarn = null;
    private bool closerEnemyCharacterToWarnSelected {
        get { 
            if(closerEnemyCharacterToWarn == null) {
                return false;
            } else {
                return true;
            }
        }
    }

    private bool enemyCharacterImpossibleToReach = false;


    private bool isEnemyCharacterToWarnCalled = false; // se è già stata avvisata una guardia dell'hostilità

    /// <summary>
    /// implementazione suspiciousAlertBehaviour
    /// </summary>
    public override void suspiciousAlertBehaviour() {

        if (!isAgentReachedDestination(lastSeenFocusAlarmCharacterPosition)) {

            _agent.SetDestination(lastSeenFocusAlarmCharacterPosition);

            _agent.isStopped = false;
            animateAndSpeedMovingAgent();
        } else {
            rotateAndAimSubBehaviour();
            stopAgent();
        }

    }
    /// <summary>
    /// implementazione hostilityAlertBehaviour
    /// </summary>
    public override void hostilityAlertBehaviour() {


        if (closerEnemyCharacterToWarnSelected) {

            if (!isAgentReachedEnemyCharacterToWarnDestination(closerEnemyCharacterToWarn.transform.position)) {


                _agent.SetDestination(closerEnemyCharacterToWarn.transform.position);

                _agent.isStopped = false;
                animateAndSpeedMovingAgent(agentSpeed: AgentSpeed.RunWalk);
            } else {

                if (!isEnemyCharacterToWarnCalled) {



                    bool isCharacterToNotifyPossibleToSee = _characterFOV.isCharacterReachableBy(
                        closerEnemyCharacterToWarn.characterFOV
                    );


                    if (isCharacterToNotifyPossibleToSee) {
                        closerEnemyCharacterToWarn.receiveWarnOfSouspiciousCheck(lastSeenFocusAlarmCharacterPosition);
                        isEnemyCharacterToWarnCalled = true;
                    } else { // impossibile raggiungere il closer enemy character

                        Debug.Log("enemyCharacterImpossibleToReach ");
                        enemyCharacterImpossibleToReach = true;
                    }


                } else {
                    rotateAndAimSubBehaviour();
                    stopAgent();
                }
                
            }
        } else {

            
            rotateAndAimSubBehaviour();
            stopAgent();

        }
        
        
    }
    /// <summary>
    /// implementazione soundAlert1Behaviour
    /// </summary>
    public override void soundAlert1Behaviour() {

    }

    /// <summary>
    /// Avvisa tutti gli npc nell'area AlertAreaCharacters
    /// </summary>
    public override void onHostilityAlert() {

        
    }

    protected override void startHostilityTimer() {
        
        // se ha scoperto da solo il character hostile (tramite il suo stesso fov)
        if (checkedByHimselfHostility) {
            isEnemyCharacterToWarnCalled = false;

            // get the closer character
            closerEnemyCharacterToWarn = characterManager.sceneEntitiesController.getCloserEnemyCharacterFromPosition(gameObject.transform.position);

            if (closerEnemyCharacterToWarnSelected) {
                _agent.updateRotation = true;
                _agent.SetDestination(closerEnemyCharacterToWarn.transform.position);
                _agent.isStopped = false;
                animateAndSpeedMovingAgent(agentSpeed: AgentSpeed.RunWalk);
            } else {

                rotateAndAimSubBehaviour();
                stopAgent();
            }
        } else {
            rotateAndAimSubBehaviour();
            stopAgent();
        }
        

        base.startHostilityTimer();
    }

    protected override async void hostilityTimerLoop() {

        /// continua a ciclare fino a quando il character civile 
        /// non ha raggiunto il character nemico da avvisare
        if (closerEnemyCharacterToWarnSelected) {
            while (!isAgentReachedEnemyCharacterToWarnDestination(closerEnemyCharacterToWarn.transform.position)) {
                await Task.Yield();

                if (characterBehaviourStopped) {
                    break;
                }

                if(enemyCharacterImpossibleToReach) {
                    break;
                }
            }
        }

        closerEnemyCharacterToWarn = null;

        hostilityTimerEndStateValue = Time.time + hostilityTimerValue; // setta
        // una volta raggiunta la posizione esaurisci l'[hostilityTimerEndStateValue]
        while (Time.time < hostilityTimerEndStateValue) { 
            await Task.Yield();

            if (characterBehaviourStopped) {
                break;
            }
        }

        stopAgent();

        
        

        if (characterAlertState == CharacterAlertState.HostilityAlert) {
            setAlert(CharacterAlertState.Unalert);
        }
    }
}
