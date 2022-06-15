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

    [SerializeField] private CharacterManager closerEnemyCharacterToWarn = null;
    private bool closerEnemyCharacterToWarnSelected {
        get { 
            if(closerEnemyCharacterToWarn == null) {
                return false;
            } else {
                return true;
            }
        }
    }


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
        //rotateAndAimSubBehaviour();


        if (closerEnemyCharacterToWarnSelected) {

            if (!isAgentReachedEnemyCharacterToWarnDestination(closerEnemyCharacterToWarn.transform.position)) {


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


        
        if (!focusAlarmCharacter.isDead) { // aggiorna dizionario dei characters in modo istantaneo


            if (checkedByHimselfHostility) {
                Dictionary<int, BaseNPCBehaviour> characters = gameObject.GetComponent<CharacterFOV>().getAlertAreaCharacters();


                if (!isEnemyCharacterToWarnCalled) {
                    foreach (var character in characters) {

                        character.Value.receiveWarnOfSouspiciousCheck(lastSeenFocusAlarmCharacterPosition);
                        isEnemyCharacterToWarnCalled = true;
                        break; // avvisa solo un character vicino
                    }
                }
            }
        }
    }


    /// <summary>
    /// Reimplementazione dell'hostility loop timer
    /// Nei Civili il loop si interrompe anche quando riescono a portare a termine
    /// la consegna di messaggi di allerta ad un nemico
    /// </summary>
    /*protected override async void hostilityTimerLoop() {
        Debug.Log("SPECIFIED LOOP");
        while (Time.time < hostilityTimerEndStateValue) {
            await Task.Yield();

            if(agentReachedDestination(agent.destination)) {
                break;
            }

            if (characterBehaviourStopped) {
                break;
            }
        }

        if (characterAlertState == CharacterAlertState.HostilityAlert) {
            setAlert(CharacterAlertState.Unalert);
        }


        // TODO

        // rimozione del alarmFocusCharacter

        // aggiorna dizionari ostilità solo se il character non è stoppato
        if (characterBehaviourStopped) {
            if (!gameObject.GetComponent<CharacterManager>().isDead) {
                onHostilityAlertTimerEnd();
            }
        }

    }*/

    protected override void startHostilityTimer() {

        // se ha scoperto da solo il character hostile (tramite il suo stesso fov)
        if(checkedByHimselfHostility) {
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
