using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Questa funzione implementa il processo di unalertBehaviour
/// Vengono selezionate delle activity in modo casuale e vengono portati a termine tutti i task
/// </summary>
public class GenericUnalertProcess : BehaviourProcess {

    private CharacterActivityManager _characterActivityManager;
    private bool unalertAgentDestinationSetted = false;
    private CharacterMovement _characterMovement;
    private CharacterFOV _characterFOV;
    private CharacterManager _characterManager;

    public GenericUnalertProcess(
        NavMeshAgent navMeshAgent,
        BaseNPCBehaviourManager baseNPCBehaviour,
        CharacterActivityManager characterActivityManager,
        CharacterMovement characterMovement,
        CharacterFOV characterFOV,
        CharacterManager characterManager
    ) {

        _behaviourAgent = navMeshAgent;
        _characterActivityManager = characterActivityManager;
        _baseNPCBehaviour = baseNPCBehaviour;
        _characterMovement = characterMovement;
        _characterFOV = characterFOV;
        _characterManager = characterManager;

        processIdName = "generic_unalert";
    }

    public override async Task runBehaviourAsyncProcess() {
        await base.runBehaviourAsyncProcess();


        _behaviourAgent.updateRotation = true; // ruota il character in base alla direzione da raggiungere

        _behaviourAgent.isStopped = false;


        if (_characterActivityManager.getCharacterActivities().Count > 0) {


            if (unalertAgentDestinationSetted == false) {

                updateUnalertAgentTarget();


                unalertAgentDestinationSetted = true;
            } else {



                if (!_characterManager.isBusy) {


                    Vector3 agentDestinationPosition = _characterActivityManager.getCurrentTask().getTaskDestination();
                    if (!_baseNPCBehaviour.isAgentReachedDestination(agentDestinationPosition)) { // controlla se � stata raggiunta la destinazione

                        _baseNPCBehaviour.animateAndSpeedMovingAgent();


                    } else { // task raggiunto


                        // esegui task ed attendi task
                        await _characterActivityManager.getCurrentTask().executeTask(
                            _characterManager,
                            _baseNPCBehaviour,
                            _characterMovement,
                            executeDuringTask: () => {

                                Vector2 taskDirection = _characterActivityManager.getCurrentTask().getTaskDirection();
                                _behaviourAgent.updateRotation = false;

                                if (_characterFOV.unalertSeenCharacter != Vector3.zero) {


                                    Vector3 unalertSeenCharacterDirection = _characterFOV.unalertSeenCharacter - _characterManager.transform.position;
                                    _characterMovement.rotateCharacter(new Vector2(unalertSeenCharacterDirection.x, unalertSeenCharacterDirection.z), false, RotationLerpSpeedValue.fast);
                                } else {
                                    _characterMovement.rotateCharacter(taskDirection, false);
                                }

                            }
                        );

                        // dopo l'await: se il Play è stato fermato o i componenti sono distrutti, esci
                        // prima di accedere ad agent/manager (evita MissingReferenceException)
                        if (!isProcessAlive()) {
                            return;
                        }

                        if (_characterActivityManager.isActualActivityLastTask()) { // se dell'attivit� attuale � l'ultimo task


                            if (_characterActivityManager.getCharacterActivities().Count > 1) { // se le attivit� sono pi� di una

                                _characterActivityManager.randomCharacterActivity(); // scegli nuova attivit� e parti dal primo task
                                updateUnalertAgentTarget();

                            } else { // se l'attivit� � unica



                                // Debug.Log("solo una attivit�");
                                if (_characterActivityManager.getCurrentCharacterActivity().loopActivity) { // se l'attivit� � ripetibile

                                    _characterActivityManager.resetSelectedTaskPos(); // scegli nuova attivit� e parti dal primo task
                                    updateUnalertAgentTarget();

                                } else {

                                    _baseNPCBehaviour.stopAgent(); // resta fermo
                                }

                            }

                        } else { // se non � l'ultimo task dell'attivit� attuale

                            // Debug.Log("passa alla prossima attivit�");
                            _characterActivityManager.setNextTaskPosOfActualActivity(); // setta in nuovo task della attivit� corrente
                            updateUnalertAgentTarget();

                        }

                    }
                } else {
                    _baseNPCBehaviour.stopAgent(); // resta fermo
                }
            }
        }
    }

    private void updateUnalertAgentTarget() {
        if (!isProcessAlive()) {
            return;
        }
        if (!_characterManager.isDead) {
            _baseNPCBehaviour.agent.SetDestination(
                _characterActivityManager.getCurrentTask().getTaskDestination()
            );
        }

    }

    public void continueWhereUnalertLeftOff() {
        unalertAgentDestinationSetted = false;
    }
}
