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
    protected bool agentDestinationSetted = false;

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

        if(!gameObject.GetComponent<CharacterManager>().isDead) {
            switch (alert) {
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
        
    }

    public override async void unalertBehaviour1() {


        if(characterActivityManager.getCharacterActivities().Count > 0) {
            if (agentDestinationSetted == false) {

                updateAgentTarget();
            

                agentDestinationSetted = true;
            } else {



                if (!gameObject.GetComponent<CharacterManager>().isBusy) {

                    float distance = Vector3.Distance(transform.position, characterActivityManager.getCurrentTask().getTaskDestination());
                    
                    if (distance > agent.stoppingDistance) { // controlla se è stata raggiunta la destinazione

                        Vector2 movement = new Vector2(agent.desiredVelocity.x, agent.desiredVelocity.z);

                        characterMovement.moveCharacter(movement, false); // avvia solo animazione
                        

                    } else { // task raggiunto

                        
                        // esegui task ed attendi task
                        await characterActivityManager.getCurrentTask().executeTask(
                            gameObject.GetComponent<CharacterManager>());
                        //Debug.Log("task eseguito");


                        if (characterActivityManager.isActualActivityLastTask()) { // se dell'attività attuale è l'ultimo task

                            
                            if(characterActivityManager.getCharacterActivities().Count > 1) { // se le attività sono più di una

                                characterActivityManager.randomCharacterActivity(); // scegli nuova attività e parti dal primo task
                                updateAgentTarget();
                            } else { // se l'attività è unica
                                // Debug.Log("solo una attività");
                                if(characterActivityManager.getCurrentCharacterActivity().loopActivity) { // se l'attività è ripetibile

                                    characterActivityManager.resetSelectedTaskPos(); // scegli nuova attività e parti dal primo task
                                    updateAgentTarget();

                                } else {
                                    characterMovement.moveCharacter(Vector2.zero, false); // resta fermo
                                }
                                
                            }

                        } else { // se non è l'ultimo task dell'attività attuale

                            // Debug.Log("passa alla prossima attività");
                            characterActivityManager.setNextTaskPosOfActualActivity(); // setta in nuovo task della attività corrente
                            updateAgentTarget();

                        }
                        


                        
                    }
                } else {
                    characterMovement.moveCharacter(Vector2.zero, false); 
                }
            }
        }
        

    }

    private void updateAgentTarget() {

        if(!gameObject.GetComponent<CharacterManager>().isDead) {
            agent.SetDestination(
                characterActivityManager.getCurrentTask().getTaskDestination()
            );
        }
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
                    doorInteractable.openDoorEvent.Invoke(gameObject.GetComponent<CharacterManager>());
                }
                
            }
        }
    }

    private void OnTriggerStay(Collider collision) {


        if (collision.gameObject.layer == INTERACTABLE_LAYER) {

            DoorInteractable doorInteractable = collision.gameObject.GetComponent<DoorInteractable>();
            if (doorInteractable != null) {

                if (doorInteractable.doorState.isDoorClosed()) {
                    doorInteractable.openDoorEvent.Invoke(gameObject.GetComponent<CharacterManager>());
                }

            }
        }
    }

}
