using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class HostilityEnemyProcess : BehaviourProcess {

    CharacterFOV _characterFOV;

    public HostilityEnemyProcess(
        NavMeshAgent behaviourAgent,
        BaseNPCBehaviourManager baseNPCBehaviour,
        CharacterFOV characterFOV
    ) {
        _behaviourAgent = behaviourAgent;
        _baseNPCBehaviour = baseNPCBehaviour;
        _characterFOV = characterFOV;

        processIdName = "hostility_enemy_process";
    }

    public override async Task runBehaviourAsyncProcess() {
        await base.runBehaviourAsyncProcess();


        onHostilityAlert();


        _baseNPCBehaviour.rotateAndAimSuspiciousAndHostility();


        if (_baseNPCBehaviour.isFocusAlarmCharacterVisible) {
            _baseNPCBehaviour.characterInventoryManager.useSelectedWeapon();
        }


        if (!_baseNPCBehaviour.isAgentReachedDestination(_baseNPCBehaviour.lastSeenFocusAlarmPosition)) {

            
            _behaviourAgent.SetDestination(_baseNPCBehaviour.lastSeenFocusAlarmPosition);


            _behaviourAgent.isStopped = false;
            _baseNPCBehaviour.animateAndSpeedMovingAgent();

            _processTaskFinished = false;
        } else {


            _baseNPCBehaviour.stopAgent();
            _processTaskFinished = true;

            _processTaskFinished = true;
        }
    }


    /// <summary>
    /// Avvisa tutti gli npc nell'area AlertAreaCharacters
    /// </summary>
    void onHostilityAlert() {
        if (!_baseNPCBehaviour.focusAlarmCharacter.isDead && _baseNPCBehaviour.isFocusAlarmCharacterVisible) { // aggiorna dizionario dei characters ricercati in modo istantaneo
            Dictionary<int, BaseNPCBehaviourManager> characters = _characterFOV.getAlertAreaCharacters();

            foreach (var character in characters) {

                bool isCharacterToNotifyPossibleToSee = _characterFOV.isCharacterReachableBy(
                    character.Value.characterFOV);

                if (isCharacterToNotifyPossibleToSee) {

                    if(!character.Value.characterManager.isDead) {
                        character.Value.hostilityCheck(_baseNPCBehaviour.focusAlarmCharacter, _baseNPCBehaviour.lastSeenFocusAlarmPosition);
                    }
                }

            }
        }
    }

}
