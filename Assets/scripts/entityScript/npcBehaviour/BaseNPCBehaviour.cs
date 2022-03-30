using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Comportamento dell'npc base classe padre, implementazione astrazione AbstractNPCBehaviour
/// </summary>
public class BaseNPCBehaviour : AbstractNPCBehaviour {
    private const int INTERACTABLE_LAYER = 3;


    protected AlertState alert = AlertState.Unalert;
    protected CharacterSpawnPoint spawnPoint; // gli spawn point contengono le activities che l'NPC dovrà eseguire
    protected CharacterMovement characterMovement; // characterMovement collegato
    protected NavMeshAgent agent;
    protected bool agentPositionSetted = false;

    protected CharacterActivityManager characterActivityManager;

    public void Start() {
        
    }

    void setAlert(AlertState alertState) {
        alert = alertState;
    }

    public void initNPCComponent(CharacterSpawnPoint spawnPoint, CharacterMovement movement) {
        this.spawnPoint = spawnPoint;
        this.characterMovement = movement;

        this.characterActivityManager = this.spawnPoint.gameObject.GetComponent<CharacterActivityManager>();
        this.agent = gameObject.gameObject.GetComponent<NavMeshAgent>();


    }

    

    public void Update() {
        switch(alert) {
            case AlertState.Unalert: {
                unalertBehaviour1();
            }
            break;
            case AlertState.Alert1: {
                alertBehaviour1();
            }
            break;
            case AlertState.Alert2: {
                alertBehaviour2();
            }
            break;
            case AlertState.Alert3: {
                alertBehaviour3();
            }
            break;
        }
    }

    public override async void unalertBehaviour1() {


        if(characterActivityManager.getCharacterActivities().Count > 0) {
            if (agentPositionSetted == false) {

            updateAgentTarget();
            

            agentPositionSetted = true;
        } else {


            

            if(!gameObject.GetComponent<CharacterState>().isBusy) {

                if (agent.remainingDistance >= agent.stoppingDistance) {

                    Vector2 movement = new Vector2(agent.desiredVelocity.x, agent.desiredVelocity.z);

                    characterMovement.moveCharacter(movement, false); // avvia solo animazione

                } else { // task raggiunto




                    // esegui task ed aspetta task
                    await characterActivityManager.getCurrentTask().executeTask(
                        gameObject.GetComponent<CharacterInteractionManager>(),
                        gameObject.GetComponent<CharacterState>());

                    characterActivityManager.setNextTaskPos();
                    updateAgentTarget();
                }
            } else {
                characterMovement.moveCharacter(Vector2.zero, false); 

                
            }
        }
        }
        

    }

    private void updateAgentTarget() {
        agent.SetDestination(
            characterActivityManager.getCurrentTask().gameObject.transform.position
        );
    }


    protected void startBehaviour() {

    }

    /// <summary>
    /// comportamento di allerta 1 da implementare nelle classi figlie
    /// </summary>
    public override void alertBehaviour1() {

    }
    /// <summary>
    /// comportamento di allerta 2 da implementare nelle classi figlie
    /// </summary>
    public override void alertBehaviour2() {

    }
    /// <summary>
    /// comportamento di allerta 3 da implementare nelle classi figlie
    /// </summary>
    public override void alertBehaviour3() {

    }

    private void OnTriggerEnter(Collider collision) {


        if (collision.gameObject.layer == INTERACTABLE_LAYER) {

            DoorInteractable doorInteractable = collision.gameObject.GetComponent<DoorInteractable>();
            if (doorInteractable != null) {

                if(doorInteractable.doorState.isDoorClosed()) {
                    doorInteractable.openDoorEvent.Invoke(gameObject.GetComponent<CharacterInteractionManager>());
                }
                
            }
        }
    }

    private void OnTriggerStay(Collider collision) {


        if (collision.gameObject.layer == INTERACTABLE_LAYER) {

            DoorInteractable doorInteractable = collision.gameObject.GetComponent<DoorInteractable>();
            if (doorInteractable != null) {

                if (doorInteractable.doorState.isDoorClosed()) {
                    doorInteractable.openDoorEvent.Invoke(gameObject.GetComponent<CharacterInteractionManager>());
                }

            }
        }
    }

}
