using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Comportamento dell'npc base classe padre, implementazione astrazione AbstractNPCBehaviour
/// </summary>
public class BaseNPCBehaviour : AbstractNPCBehaviour {

    // const
    private const int INTERACTABLE_LAYER = 3;

    // values
    [Header("Configurazione")]
    [SerializeField] private const float suspiciousTimerValue = 15f;
    private float suspiciousTimerEndStateValue = 0f; // timer che indica il valore in cui il suspiciousTimerLoop si stoppa
    [SerializeField] private const float hostilityTimerValue = 15f;
    private float hostilityTimerEndStateValue = 0f; // timer che indica il valore in cui il hostilityTimerLoop si stoppa
    [SerializeField] private const float cNPCBehaviourCoroutineFrequency = 0.02f;

    // states
    [Header("Stati")]
    protected CharacterManager alarmFocusCharacter; // ref del character che ha provocato gli stati di allarme 
    [SerializeField] protected CharacterAlertState _characterState = CharacterAlertState.Unalert;
    public CharacterAlertState characterAlertState {
        get { return _characterState; }
    }
    
    // ref
    protected CharacterSpawnPoint spawnPoint; // gli spawn point contengono le activities che l'NPC dovr� eseguire
    protected CharacterMovement characterMovement; // characterMovement collegato
    protected NavMeshAgent agent;
    protected bool agentDestinationSetted = false;

    protected CharacterActivityManager characterActivityManager;


    public void initNPCComponent(CharacterSpawnPoint spawnPoint, CharacterMovement movement) {
        this.spawnPoint = spawnPoint;
        this.characterMovement = movement;

        this.characterActivityManager = this.spawnPoint.gameObject.GetComponent<CharacterActivityManager>();
        this.agent = gameObject.gameObject.GetComponent<NavMeshAgent>();


    }
    public void Start() {
        StartCoroutine(cNPCBehaviourCoroutine());
    }
    private IEnumerator cNPCBehaviourCoroutine() {

        while (true) {
            yield return new WaitForSeconds(cNPCBehaviourCoroutineFrequency);
            nPCBehaviour();
        }
    }

    // stoppa agente e animazione dell'agente che dipende dal move character
    public void stopAgent() {
        agent.isStopped = true;
        characterMovement.moveCharacter(Vector2.zero, false);
    }

    public void stopAllCoroutines() {
        StopAllCoroutines();
    }






    public void setAlert(CharacterAlertState alertState) {

        if(_characterState == CharacterAlertState.Unalert && alertState == CharacterAlertState.SuspiciousAlert) {

            startSuspiciousTimer();
        }else if(_characterState == CharacterAlertState.SuspiciousAlert && alertState == CharacterAlertState.SuspiciousAlert) {

            resetSuspiciousTimer();
        } else if(_characterState == CharacterAlertState.Unalert && alertState == CharacterAlertState.HostilityAlert) {

            startHostilityTimer();
        } else if (_characterState == CharacterAlertState.SuspiciousAlert && alertState == CharacterAlertState.HostilityAlert) {

            stopSuspiciousTimer();
            startHostilityTimer();
        } else if(_characterState == CharacterAlertState.HostilityAlert && alertState == CharacterAlertState.HostilityAlert) {

            stopSuspiciousTimer();
            resetHostilityTimer();
        } else if((_characterState == CharacterAlertState.HostilityAlert || _characterState == CharacterAlertState.SuspiciousAlert) && alertState == CharacterAlertState.Unalert) {

            
        }
        _characterState = alertState;
    }


    private void nPCBehaviour() {

        if(!gameObject.GetComponent<CharacterManager>().isDead) {
            switch (_characterState) {
                case CharacterAlertState.Unalert: {
                        unalertBehaviour();
                    }
                    break;
                case CharacterAlertState.SuspiciousAlert: {
                        suspiciousAlertBehaviour();
                    }
                    break;
                case CharacterAlertState.HostilityAlert: {
                        hostilityAlertBehaviour();
                    }
                    break;
                case CharacterAlertState.SoundAlert1: {
                        soundAlert1Behaviour();
                    }
                    break;
            }
        }
        
    }

    /// <summary>
    /// Questa funzione implementa il comportamento di unalertBehaviour
    /// Vengono selezionate delle activity in modo casuale e vengono portati a termine tutti i task
    /// </summary>
    public override async void unalertBehaviour() {
        agent.isStopped = false;

        if (characterActivityManager.getCharacterActivities().Count > 0) {
            if (agentDestinationSetted == false) {

                updateUnalertAgentTarget();
            

                agentDestinationSetted = true;
            } else {



                if (!gameObject.GetComponent<CharacterManager>().isBusy) {

                    float distance = Vector3.Distance(transform.position, characterActivityManager.getCurrentTask().getTaskDestination());
                    
                    if (distance > agent.stoppingDistance) { // controlla se � stata raggiunta la destinazione

                        Vector2 movement = new Vector2(agent.desiredVelocity.x, agent.desiredVelocity.z);

                        characterMovement.moveCharacter(movement, false); // avvia solo animazione
                        

                    } else { // task raggiunto

                        
                        // esegui task ed attendi task
                        await characterActivityManager.getCurrentTask().executeTask(
                            gameObject.GetComponent<CharacterManager>(),
                            this
                        );
                        //Debug.Log("task eseguito");


                        if (characterActivityManager.isActualActivityLastTask()) { // se dell'attivit� attuale � l'ultimo task

                            
                            if(characterActivityManager.getCharacterActivities().Count > 1) { // se le attivit� sono pi� di una

                                characterActivityManager.randomCharacterActivity(); // scegli nuova attivit� e parti dal primo task
                                updateUnalertAgentTarget();
                            } else { // se l'attivit� � unica
                                // Debug.Log("solo una attivit�");
                                if(characterActivityManager.getCurrentCharacterActivity().loopActivity) { // se l'attivit� � ripetibile

                                    characterActivityManager.resetSelectedTaskPos(); // scegli nuova attivit� e parti dal primo task
                                    updateUnalertAgentTarget();

                                } else {
                                    characterMovement.moveCharacter(Vector2.zero, false); // resta fermo
                                }
                                
                            }

                        } else { // se non � l'ultimo task dell'attivit� attuale

                            // Debug.Log("passa alla prossima attivit�");
                            characterActivityManager.setNextTaskPosOfActualActivity(); // setta in nuovo task della attivit� corrente
                            updateUnalertAgentTarget();

                        }
                        
                    }
                } else {
                    characterMovement.moveCharacter(Vector2.zero, false); 
                }
            }
        }
        

    }

    private void updateUnalertAgentTarget() {

        if(!gameObject.GetComponent<CharacterManager>().isDead) {
            agent.SetDestination(
                characterActivityManager.getCurrentTask().getTaskDestination()
            );
        }
    }


    /// <summary>
    /// comportamento suspiciousAlertBehaviour da implementare nelle classi figlie
    /// </summary>
    public override void suspiciousAlertBehaviour() {

    }
    /// <summary>
    /// comportamento HostilityAlertBehaviour da implementare nelle classi figlie
    /// </summary>
    public override void hostilityAlertBehaviour() {

    }
    /// <summary>
    /// comportamento SoundAlert1Behaviour da implementare nelle classi figlie
    /// </summary>
    public override void soundAlert1Behaviour() {

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

    /// <summary>
    /// Metodo avviato dal FOV dei character
    /// </summary>
    /// <param name="seenCharacterManager"></param>
    public override void suspiciousCheck(CharacterManager seenCharacterManager) {
        bool isCharacterInProhibitedAreaCheck = seenCharacterManager.gameObject.GetComponent<CharacterAreaManager>().isCharacterInProhibitedAreaCheck();

        if(_characterState == CharacterAlertState.Unalert) {

            if (isCharacterInProhibitedAreaCheck) {
                setAlert(CharacterAlertState.SuspiciousAlert);
            }
        }
        
        //.
        //throw new System.NotImplementedException();
    }

    public override void hostilityCheck(CharacterManager seenCharacterManager) {
        bool isCharacterInProhibitedAreaCheck = seenCharacterManager.gameObject.GetComponent<CharacterAreaManager>().isCharacterInProhibitedAreaCheck();

        if (isCharacterInProhibitedAreaCheck) {
            setAlert(CharacterAlertState.HostilityAlert);
        }

        // TODO
        // aggiunta character id nel dizionario
        //throw new System.NotImplementedException();
    }



    /// <summary>
    /// Questa funzione setta il punto di fine del suspiciousTimerLoop
    /// e avvia il suspiciousTimerLoop
    /// </summary>
    private void startSuspiciousTimer() {
        suspiciousTimerEndStateValue = Time.time + suspiciousTimerValue;
        suspiciousTimerLoop();
    }
    /// <summary>
    /// Questa funzione resetta il punto di fine del suspiciousTimerEndStateValue usato nel loop suspiciousTimerLoop
    /// </summary>
    private void resetSuspiciousTimer() {
        suspiciousTimerEndStateValue = Time.time + suspiciousTimerValue;
    }

    /// <summary>
    /// Questa funzione setta il punto di fine del hostilityTimerEndStateValue
    /// e avvia il hostilityTimerLoop
    /// </summary>
    private void startHostilityTimer() {
        hostilityTimerEndStateValue = Time.time + hostilityTimerValue;
        hostilityTimerLoop();
    }
    /// <summary>
    /// Questa funzione resetta il punto di fine del hostilityTimerEndStateValue usato nel loop hostilityTimerLoop
    /// </summary>
    private void resetHostilityTimer() {
        hostilityTimerEndStateValue = Time.time + hostilityTimerValue;
    }

    private void stopSuspiciousTimer() {
        suspiciousTimerEndStateValue = 0;
    }
    private void stopHostilityCheckTimer() {
        hostilityTimerEndStateValue = 0;
    }

    
    private async void suspiciousTimerLoop() {
        

        while (Time.time < suspiciousTimerEndStateValue) {
            await Task.Yield();
        }

        if(characterAlertState != CharacterAlertState.HostilityAlert && characterAlertState == CharacterAlertState.SuspiciousAlert) {
            setAlert(CharacterAlertState.Unalert);
        }
        

        // TODO fine timer
        // ritorno allo stato Unalert
        // rimozione del alarmFocusCharacter
    }
    private async void hostilityTimerLoop() {
        
        while (Time.time < hostilityTimerEndStateValue) {
            await Task.Yield();
        }
        if (characterAlertState == CharacterAlertState.HostilityAlert) {
            setAlert(CharacterAlertState.Unalert);
        }

        // TODO fine timer
        // aggiunta character id nel dizionario
        // ritorno allo stato Unalert
        // rimozione del alarmFocusCharacter
        // (caso enemy)comunicazione del character a tutti gli altri character della mappa
        // (caso civilian)
    }

    // TODO check area accessibile

    // TODO check character � contenuto nel dizionario dei character ostili

    // TODO check il character impugna un item non compatibile con il suo ruolo
}
