using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CivilianNPCBehaviourManager : BaseNPCBehaviourManager {
    static public GameObject initCivilianNPCComponent(GameObject gameObject, CharacterSpawnPoint spawnPoint) {

        CivilianNPCBehaviourManager enemyNPCNewComponent = gameObject.GetComponent<CivilianNPCBehaviourManager>();
        enemyNPCNewComponent.initNPCComponent(spawnPoint);

        return gameObject;
    }


    protected override void startHostilityTimer(bool checkedByHimself) {
        stopAgent(); // stop task agent

        // start behaviour process
        mainBehaviourProcess = new CivilianHostilityAlertProcess(_agent, this, _characterFOV, _characterManager, checkedByHimself);

        hostilityTimerLoop();
    }


    /// <summary>
    /// implementazione suspiciousAlertBehaviour
    /// </summary>
    public override void suspiciousAlertBehaviour() {

        if (!isAgentReachedDestination(lastSeenFocusAlarmPosition)) {

            _agent.updateRotation = true;
            _agent.SetDestination(lastSeenFocusAlarmPosition);

            _agent.isStopped = false;
            animateAndSpeedMovingAgent();
        } else {
            rotateAndAimSuspiciousAndHostility();
            stopAgent();
        }

    }

    /// <summary>
    /// implementazione hostilityAlertBehaviour
    /// </summary>
    public override void corpseFoundConfirmedAlertBehaviour() {
        /*

        if (closerEnemyCharacterToWarnSelected) {

            if (!isAgentReachedEnemyCharacterToWarnDestination(closerEnemyCharacterToWarn.transform.position)) {
                

                _agent.SetDestination(closerEnemyCharacterToWarn.transform.position);

                _agent.isStopped = false;
                animateAndSpeedMovingAgent(agentSpeed: AgentSpeed.Run);
            } else {

                if (!isEnemyCharacterToWarnCalled) {



                    bool isCharacterToNotifyPossibleToSee = _characterFOV.isCharacterReachableBy(
                        closerEnemyCharacterToWarn.characterFOV
                    );

                    if (isCharacterToNotifyPossibleToSee) {
                        closerEnemyCharacterToWarn.receiveWarnOfSouspiciousCheck(lastSeenFocusAlarmPosition);
                        isEnemyCharacterToWarnCalled = true;
                    } else { // impossibile raggiungere il closer enemy character

                        Debug.Log("enemyCharacterImpossibleToReach ");
                        enemyCharacterImpossibleToReach = true;
                    }


                } else {
                    stopAgent();
                }

            }
        } else {

            stopAgent();
        }*/

    }
    /// <summary>
    /// implementazione soundAlert1Behaviour
    /// </summary>
    public override void soundAlert1Behaviour() {

    }

    /// <summary>
    /// Implementazione del suspiciousCorpseFoundAlertBehaviour del character nemico
    /// Cerca di raggiungere la lastSeenFocusAlarmPosition
    /// </summary>
    public override void suspiciousCorpseFoundAlertBehaviour() {
        _agent.updateRotation = true; // ruota il character in base alla direzione da raggiungere

        if (!isAgentReachedDestination(lastSeenFocusAlarmPosition)) {

            _agent.SetDestination(lastSeenFocusAlarmPosition);

            _agent.isStopped = false;
            animateAndSpeedMovingAgent(agentSpeed: AgentSpeed.Run);
        } else {


            stopAgent();
        }
    }


    /*protected override void startCorpseFoundConfirmedTimer() {
        stopAgent();

        isEnemyCharacterToWarnCalled = false;

        // get the closer character
        closerEnemyCharacterToWarn = _characterManager.sceneEntitiesController.getCloserEnemyCharacterFromPosition(gameObject.transform.position);

        if (closerEnemyCharacterToWarnSelected) {
            _agent.updateRotation = true;
            _agent.SetDestination(closerEnemyCharacterToWarn.transform.position);
            _agent.isStopped = false;
            animateAndSpeedMovingAgent(agentSpeed: AgentSpeed.Run);
        } else {

            stopAgent();
        }

        base.startCorpseFoundConfirmedTimer();
    }*/

    protected override async void hostilityTimerLoop() {

        /// continua a ciclare fino a quando il character civile 
        /// non ha raggiunto il character nemico da avvisare
        while (!mainBehaviourProcess.processTaskFinished) {
            await Task.Yield();

            if (characterBehaviourStopped) {
                break;
            }
            if (hostilityTimerEndStateValue == 0) {
                break;
            }
        }



        hostilityTimerEndStateValue = Time.time + hostilityTimerValue; // setta
        // una volta raggiunta la posizione esaurisci l'[hostilityTimerEndStateValue]
        while (Time.time < hostilityTimerEndStateValue) { 
            await Task.Yield();

            if (characterBehaviourStopped) {
                break;
            }
        }

        if (!characterBehaviourStopped) {
            stopAgent();
        }



        if (characterAlertState == CharacterAlertState.HostilityAlert) {
            setAlert(CharacterAlertState.Unalert, true);
        }
    }

    /// <summary>
    /// Timer loop usato per gestire la durata dello stato corpseFoundConfirmedAlert
    /// </summary>
    protected override async void corpseFoundConfirmedTimerLoop() {
        /*

        /// continua a ciclare fino a quando il character civile 
        /// non ha raggiunto il character nemico da avvisare
        if (closerEnemyCharacterToWarnSelected) {
            while (!isAgentReachedEnemyCharacterToWarnDestination(closerEnemyCharacterToWarn.transform.position)) {

                await Task.Yield();

                if (characterBehaviourStopped) {
                    break;
                }
                if (corpseFoundConfirmedTimerEndStateValue == 0) {
                    break;
                }
                if (enemyCharacterImpossibleToReach) {
                    break;
                }
            }
        }


        closerEnemyCharacterToWarn = null; // una volta che il character è stato raggiunto può fare a meno del closerEnemyCharacterToWarn

        corpseFoundConfirmedTimerEndStateValue = Time.time + corpseFoundConfirmedTimerValue;
        while (Time.time < corpseFoundConfirmedTimerEndStateValue) {
            await Task.Yield();

            if (characterBehaviourStopped) {
                break;
            }
        }

        if (!characterBehaviourStopped) {
            stopAgent();
        }

        if (characterAlertState == CharacterAlertState.CorpseFoundConfirmedAlert) {
            setAlert(CharacterAlertState.Unalert);
        }*/


    }


    /// <summary>
    /// Quando il character è in allerta non ha alcun comportamento aggiuntivo
    /// </summary>
    public override void onHostilityAlert() {

    }
}
