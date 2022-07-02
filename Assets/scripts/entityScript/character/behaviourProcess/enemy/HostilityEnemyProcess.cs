using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class HostilityEnemyProcess : BehaviourProcess {

    CharacterFOV _characterFOV;
    float distanceFromHostilityCharacter = 3f;

    public HostilityEnemyProcess(
        Vector3 lastSeenFocusAlarmPosition,
        NavMeshAgent behaviourAgent,
        BaseNPCBehaviourManager baseNPCBehaviour,
        CharacterFOV characterFOV
    ) {
        _lastSeenFocusAlarmPosition = lastSeenFocusAlarmPosition;

        _behaviourAgent = behaviourAgent;
        _baseNPCBehaviour = baseNPCBehaviour;
        _characterFOV = characterFOV;

        processIdName = "hostility_enemy_process";
    }

    public override async Task runBehaviourAsyncProcess() {
        await base.runBehaviourAsyncProcess();


        onHostilityAlert();


        _baseNPCBehaviour.rotateAndAimSuspiciousAndHostility(_lastSeenFocusAlarmPosition);


        if (_baseNPCBehaviour.isFocusAlarmCharacterVisible) {
            manageWeaponAction();
        }


        if (!_baseNPCBehaviour.isAgentReachedDestination(_lastSeenFocusAlarmPosition)) {

            
            if (_baseNPCBehaviour.isFocusAlarmCharacterVisible) {

                float distance = Vector3.Distance(_baseNPCBehaviour.gameObject.transform.position, _baseNPCBehaviour.focusAlarmCharacter.transform.position);
                if(distance > distanceFromHostilityCharacter) {
                    _behaviourAgent.SetDestination(_lastSeenFocusAlarmPosition);


                    _behaviourAgent.isStopped = false;
                    _baseNPCBehaviour.animateAndSpeedMovingAgent();

                } else {
                    _baseNPCBehaviour.stopAgent();
                }
            } else {
                _behaviourAgent.SetDestination(_lastSeenFocusAlarmPosition);


                _behaviourAgent.isStopped = false;
                _baseNPCBehaviour.animateAndSpeedMovingAgent();

                _processTaskFinished = false;
            }
            
        } else {


            _baseNPCBehaviour.stopAgent();
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
                        character.Value.hostilityCheck(_baseNPCBehaviour.focusAlarmCharacter, _lastSeenFocusAlarmPosition);
                    }
                }

            }
        }
    }




    private void manageWeaponAction() {
        // implementare nell'inventario is selected weapon empty

        

        if(!_baseNPCBehaviour.characterInventoryManager.isSelectedWeaponEmpty) {

            // fire
            _baseNPCBehaviour.characterInventoryManager.useSelectedWeapon();

        } else { // l'arma selezionata non ha munzioni disponibili

            if(_baseNPCBehaviour.characterInventoryManager.isAllWeaponEmpty) {


            } else {


                _baseNPCBehaviour.characterInventoryManager.selectFirstWeaponWithAmmunition(); // selezionare con un po' di ritardo(?)
            }
        }


    }

}
