using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyNPCBehaviour : BaseNPCBehaviour {

    static public GameObject initEnemyNPComponent(GameObject gameObject, CharacterSpawnPoint spwanPoint) {

        EnemyNPCBehaviour enemyNPCNewComponent = gameObject.GetComponent<EnemyNPCBehaviour>();
        enemyNPCNewComponent.initNPCComponent(spwanPoint, gameObject.GetComponent<CharacterMovement>());

        return gameObject;
    }


    /// <summary>
    /// implementazione suspiciousAlertBehaviour
    /// </summary>
    public override void suspiciousAlertBehaviour() {


        rotateAndAimSuspiciousAndHostilitySubBehaviour();


        if (!isAgentReachedDestination(lastSeenFocusAlarmPosition)) {

            _agent.SetDestination(lastSeenFocusAlarmPosition);

            _agent.isStopped = false;
            animateAndSpeedMovingAgent();
        } else {
            stopAgent();
        }



    }
    /// <summary>
    /// implementazione hostilityAlertBehaviour
    /// </summary>
    public override void hostilityAlertBehaviour() {

        rotateAndAimSuspiciousAndHostilitySubBehaviour();

        if (isFocusAlarmCharacterVisible) {
            characterInventoryManager.useSelectedWeapon();
        }


        if (!isAgentReachedDestination(lastSeenFocusAlarmPosition)) {


            _agent.SetDestination(lastSeenFocusAlarmPosition);


            _agent.isStopped = false;
            animateAndSpeedMovingAgent();
        } else {
            stopAgent();
        }
    }
    /// <summary>
    /// implementazione warnOfSouspiciousAlertBehaviour
    /// il character nemico cerca di raggiungere la lastSeenFocusAlarmPosition
    /// </summary>
    public override void warnOfSouspiciousAlertBehaviour() {
        _agent.updateRotation = true; // ruota il character in base alla direzione da raggiungere

        if (!isAgentReachedDestination(lastSeenFocusAlarmPosition)) {

            _agent.SetDestination(lastSeenFocusAlarmPosition);

            _agent.isStopped = false;
            animateAndSpeedMovingAgent(agentSpeed: AgentSpeed.Run);
        } else {
            stopAgent();
        }
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

    /// <summary>
    /// Per le guardie nemiche quando termina l'HostilityTimer viene aggiornato il dizionario a livello globale
    /// passando il dizionario del character (viene fatta l'unione)
    /// Inoltre tutti gli altri character nemici avranno il dizionario hostility aggiornato
    /// </summary>
    public override void onHostilityAlertTimerEnd() {
        gameObject.GetComponent<CharacterManager>().globalGameState.updateGlobalWantedHostileCharacters(this._wantedHostileCharacters);
    }

    /// <summary>
    /// Avvisa tutti gli npc nell'area AlertAreaCharacters
    /// </summary>
    public override void onHostilityAlert() {

        if(focusAlarmCharacter != null) {
            if (!focusAlarmCharacter.isDead && isFocusAlarmCharacterVisible) { // aggiorna dizionario dei characters ricercati in modo istantaneo
                Dictionary<int, BaseNPCBehaviour> characters = gameObject.GetComponent<CharacterFOV>().getAlertAreaCharacters();

                foreach (var character in characters) {

                    bool isCharacterToNotifyPossibleToSee = _characterFOV.isCharacterReachableBy(
                        character.Value.characterFOV);

                    if (isCharacterToNotifyPossibleToSee) {
                        character.Value.hostilityCheck(focusAlarmCharacter, lastSeenFocusAlarmPosition);
                    }

                }
            }
        } else {
            throw new System.NotImplementedException();
        }
        

    }
}
