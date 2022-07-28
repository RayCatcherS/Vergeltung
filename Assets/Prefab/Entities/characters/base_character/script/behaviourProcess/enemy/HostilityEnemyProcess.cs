using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class HostilityEnemyProcess : BehaviourProcess {

    CharacterFOV _characterFOV;
    float distanceFromHostilityCharacter = 4f;

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
                resetFireState();
                _behaviourAgent.SetDestination(_lastSeenFocusAlarmPosition);


                _behaviourAgent.isStopped = false;
                _baseNPCBehaviour.animateAndSpeedMovingAgent(AgentSpeed.Run);

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

                bool isCharacterToNotifyPossibleToSee = _characterFOV.canCharacterReachableBy(
                    character.Value.characterFOV);

                if (isCharacterToNotifyPossibleToSee) {

                    if(!character.Value.characterManager.isDead) {
                        character.Value.hostilityCheck(_baseNPCBehaviour.focusAlarmCharacter, _lastSeenFocusAlarmPosition);
                    }
                }

            }
        }
    }



    /// <summary>
    /// Seleziona la prima arma utilizzabile(che ha delle munzioni)
    /// </summary>
    private void manageWeaponAction() {
        // implementare nell'inventario is selected weapon empty

        

        if(!_baseNPCBehaviour.characterInventoryManager.isSelectedWeaponEmpty) {

            // fire
            manageFire();

        } else { // l'arma selezionata non ha munzioni disponibili

            if(_baseNPCBehaviour.characterInventoryManager.isAllWeaponEmpty) {


            } else {


                _baseNPCBehaviour.characterInventoryManager.selectFirstWeaponWithAmmunition(); // selezionare con un po' di ritardo(?)
                _baseNPCBehaviour.characterInventoryManager.extractWeapon();
            }
        }


    }


    private float offFireEndTime = -1; // tempo di end in cui l'arma non spara
    private const float offFireMinRange = 0.4f;
    private const float offFireMaxRange = 1.35f;
    private bool offFireTurn = false;

    private float onFireEndTime = -1; // tempo di end in cui l'arma spara
    private const float onFireMinRange = 0.05f;
    private const float onFireMaxRange = 0.4f;
    private bool onFireTurn = false;

    private void manageFire() {

        // off fire end time not initialized
        if(offFireEndTime == -1) {
            onFireTurn = false;

            setOffFireEnd();
            offFireTurn = true;
        }


        // off fire ended?
        if(offFireTurn) {
            if(offFireEndTime < Time.time) {
                offFireTurn = false;

                setOnFireEnd();
                onFireTurn = true;
            }
        }
        

        // on fire ended? => start of fire
        if(onFireTurn) {
            if(onFireEndTime < Time.time) {
                onFireTurn = false;

                setOffFireEnd();
                offFireTurn = true;
            }
        }
        


        if(onFireTurn) {
            _baseNPCBehaviour.characterInventoryManager.useSelectedWeapon();
        }
        
    }

    void setOffFireEnd() {
        
        offFireEndTime = Time.time + Random.Range(offFireMinRange, offFireMaxRange);
    }

    void setOnFireEnd() {
        onFireEndTime = Time.time + Random.Range(onFireMinRange, onFireMaxRange);
    }

    private void resetFireState() {
        offFireEndTime = -1;
        onFireEndTime = -1;

        offFireTurn = false;
        onFireTurn = false;
    }

    public override void resetProcess() {
        _processTaskFinished = false;
    }
}
