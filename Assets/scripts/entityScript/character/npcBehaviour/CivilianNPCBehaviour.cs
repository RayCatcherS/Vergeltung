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

    /// <summary>
    /// implementazione suspiciousAlertBehaviour
    /// </summary>
    public override void suspiciousAlertBehaviour() {

        rotateAndAimSubBehaviour();


        if (!isAgentReachedDestination(lastSeenFocusAlarmCharacterPosition)) {

            agent.SetDestination(lastSeenFocusAlarmCharacterPosition);

            agent.isStopped = false;
            animateMovingAgent();
        } else {
            stopAgent();
        }
    }
    /// <summary>
    /// implementazione hostilityAlertBehaviour
    /// </summary>
    public override void hostilityAlertBehaviour() {
        //rotateAndAimSubBehaviour();


        if(closerEnemyCharacterToWarnSelected) {

            if (!isAgentReachedEnemyCharacterToWarnDestination(closerEnemyCharacterToWarn.transform.position)) {


                agent.SetDestination(closerEnemyCharacterToWarn.transform.position);

                agent.isStopped = false;
                animateMovingAgent();
            } else {
                stopAgent();
                closerEnemyCharacterToWarn = null;
            }
        } else {
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
            Dictionary<int, BaseNPCBehaviour> characters = gameObject.GetComponent<CharacterFOV>().getAlertAreaCharacters();

            foreach (var character in characters) {

                character.Value.suspiciousCheck(focusAlarmCharacter, lastSeenFocusAlarmCharacterPosition);
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
        base.startHostilityTimer();

        // get the closer character
        closerEnemyCharacterToWarn = characterManager.sceneEntitiesController.getCloserEnemyCharacterFromPosition(gameObject.transform.position);

        if(closerEnemyCharacterToWarnSelected) {
            agent.updateRotation = true;
            agent.SetDestination(closerEnemyCharacterToWarn.transform.position);
            agent.isStopped = false;
            animateMovingAgent();
        }
    }
}
