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

    [SerializeField] protected CharacterManager focusAlarmCharacter; // ref del character che ha provocato gli stati di allarme
    public Vector3 lastSeenFocusAlarmCharacterPosition; // ultima posizione che è stata visibile del character che ha provocato gli stati di allarme
    protected bool isFocusAlarmCharacterVisible {
        get {
            if(focusAlarmCharacter != null) {
                return characterFOV.checkObjectInSecondFOV(focusAlarmCharacter.gameObject.transform.position, focusAlarmCharacter.gameObject.GetComponent<CharacterFOV>().recognitionTarget.position);
            } else {
                return false;
            }
        }

    }
    [SerializeField] protected CharacterAlertState _characterState = CharacterAlertState.Unalert; // stato 
    public CharacterAlertState characterAlertState {
        get { return _characterState; }
    }
    protected bool agentDestinationSetted = false;
    protected Dictionary<int, CharacterManager> _wantedHostileCharacters = new Dictionary<int, CharacterManager>();
    public Dictionary<int, CharacterManager> wantedHostileCharacters {
        set {
            _wantedHostileCharacters = value;
        }
        get {
            return _wantedHostileCharacters;
        }
    }

    // ref
    [Header("Reference")]
    [SerializeField] protected Animator alertSignAnimator;
    protected CharacterActivityManager characterActivityManager;
    protected CharacterSpawnPoint spawnPoint; // gli spawn point contengono le activities che l'NPC dovrà eseguire
    protected CharacterMovement characterMovement; // characterMovement collegato
    [SerializeField] protected NavMeshAgent agent;
    [SerializeField] protected CharacterFOV characterFOV;
    [SerializeField] protected InventoryManager characterInventoryManager;
    


    public void initNPCComponent(CharacterSpawnPoint spawnPoint, CharacterMovement movement) {
        this.spawnPoint = spawnPoint;
        this.characterMovement = movement;

        this.characterActivityManager = this.spawnPoint.gameObject.GetComponent<CharacterActivityManager>();
    }
    public void Start() {
        StartCoroutine(cNPCBehaviourCoroutine());
    }
    private IEnumerator cNPCBehaviourCoroutine() {

        while (true) {
            yield return new WaitForSeconds(cNPCBehaviourCoroutineFrequency);
            nPCBehaviour();

            if(_characterState == CharacterAlertState.HostilityAlert) {
                onHostilityAlert(); // start dell'evento on hostility
            }
            
        }
    }

    // stoppa agente e animazione dell'agente che dipende dal move character
    public void stopAgent() {
        agent.isStopped = true;
        characterMovement.stopCharacter();
    }

    public void stopAllCoroutines() {
        StopAllCoroutines();
    }





    /// <summary>
    /// cambia lo stato di allerta del character e avvia animazione 
    /// di allerta
    /// </summary>
    /// <param name="alertState"></param>
    private void setAlert(CharacterAlertState alertState) {

        
        CharacterAlertState oldAlertState = _characterState;

        _characterState = alertState;

        if (oldAlertState == CharacterAlertState.Unalert && alertState == CharacterAlertState.SuspiciousAlert) { // SuspiciousAlert

            startSuspiciousTimer();

            // animation sign
            resetAlertAnimatorTrigger();
            alertSignAnimator.SetTrigger("suspiciousAlert");

        } else if(oldAlertState == CharacterAlertState.SuspiciousAlert && alertState == CharacterAlertState.SuspiciousAlert) { // SuspiciousAlert

            resetSuspiciousTimer();
        } else if(oldAlertState == CharacterAlertState.Unalert && alertState == CharacterAlertState.HostilityAlert) { // HostilityAlert

            startHostilityTimer();

            // animation sign
            resetAlertAnimatorTrigger();
            alertSignAnimator.SetTrigger("hostilityAlert");
        } else if (oldAlertState == CharacterAlertState.SuspiciousAlert && alertState == CharacterAlertState.HostilityAlert) { // HostilityAlert

            stopSuspiciousTimer();
            startHostilityTimer();
            // animation sign
            resetAlertAnimatorTrigger();
            alertSignAnimator.SetTrigger("hostilityAlert");
        } else if(oldAlertState == CharacterAlertState.HostilityAlert && alertState == CharacterAlertState.HostilityAlert) { // HostilityAlert


            stopSuspiciousTimer();
            resetHostilityTimer();
        } else if((oldAlertState == CharacterAlertState.HostilityAlert || oldAlertState == CharacterAlertState.SuspiciousAlert) && alertState == CharacterAlertState.Unalert) {

            
        }
        
        if(alertState == CharacterAlertState.Unalert) {
            // animation sign
            resetAlertAnimatorTrigger();
            alertSignAnimator.SetTrigger("unalertState");

            focusAlarmCharacter = null;
        }

        
    }


    /// <summary>
    /// Switch dei behaviour
    /// </summary>
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
                    
                    if (distance > agent.stoppingDistance) { // controlla se è stata raggiunta la destinazione

                        Vector2 movement = new Vector2(agent.desiredVelocity.x, agent.desiredVelocity.z);

                        characterMovement.moveCharacter(movement, false); // avvia solo animazione
                        

                    } else { // task raggiunto

                        
                        // esegui task ed attendi task
                        await characterActivityManager.getCurrentTask().executeTask(
                            gameObject.GetComponent<CharacterManager>(),
                            this
                        );
                        //Debug.Log("task eseguito");


                        if (characterActivityManager.isActualActivityLastTask()) { // se dell'attività attuale è l'ultimo task

                            
                            if(characterActivityManager.getCharacterActivities().Count > 1) { // se le attività sono più di una

                                characterActivityManager.randomCharacterActivity(); // scegli nuova attività e parti dal primo task
                                updateUnalertAgentTarget();
                            } else { // se l'attività è unica
                                // Debug.Log("solo una attività");
                                if(characterActivityManager.getCurrentCharacterActivity().loopActivity) { // se l'attività è ripetibile

                                    characterActivityManager.resetSelectedTaskPos(); // scegli nuova attività e parti dal primo task
                                    updateUnalertAgentTarget();

                                } else {
                                    characterMovement.moveCharacter(Vector2.zero, false); // resta fermo
                                }
                                
                            }

                        } else { // se non è l'ultimo task dell'attività attuale

                            // Debug.Log("passa alla prossima attività");
                            characterActivityManager.setNextTaskPosOfActualActivity(); // setta in nuovo task della attività corrente
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

                if(doorInteractable.doorState.isDoorClosed().value) {
                    doorInteractable.openDoorEvent.Invoke(gameObject.GetComponent<CharacterManager>());
                }
                
            }
        }
    }

    /// <summary>
    /// Metodo avviato dal FOV dei character
    /// </summary>
    /// <param name="seenCharacterManager"></param>
    public override void suspiciousCheck(CharacterManager seenCharacterManager)  {
        bool isCharacterInProhibitedAreaCheck = seenCharacterManager.gameObject.GetComponent<CharacterAreaManager>().isCharacterInProhibitedAreaCheck();
        bool isUsedItemProhibitedCheck = seenCharacterManager.gameObject.GetComponent<CharacterManager>().inventoryManager.isUsedItemProhibitedCheck();
        bool isCharacterLockpicking = seenCharacterManager.isPickLocking;

        if (_characterState == CharacterAlertState.Unalert || _characterState == CharacterAlertState.SuspiciousAlert) {

            

            if (isCharacterInProhibitedAreaCheck || isUsedItemProhibitedCheck || isCharacterWantedCheck(seenCharacterManager) || isCharacterLockpicking) {

                focusAlarmCharacter = seenCharacterManager; // character che ha fatto cambiare lo stato dell'Base NPC Behaviour

                if (seenCharacterManager.isRunning || seenCharacterManager.isWeaponCharacterFiring) { // azioni che confermano istantaneamente l'ostilità nel suspiciousCheck passando direttamente allo stato di HostilityAlert

                    setAlert(CharacterAlertState.HostilityAlert);
                } else {
                    setAlert(CharacterAlertState.SuspiciousAlert);
                }
                
            } else {
                focusAlarmCharacter = null;
            }
        }
    }

    public override void hostilityCheck(CharacterManager seenCharacterManager) {

        bool isCharacterInProhibitedAreaCheck = seenCharacterManager.gameObject.GetComponent<CharacterAreaManager>().isCharacterInProhibitedAreaCheck();
        bool isUsedItemProhibitedCheck = seenCharacterManager.gameObject.GetComponent<CharacterManager>().inventoryManager.isUsedItemProhibitedCheck();
        bool isCharacterLockpicking = seenCharacterManager.isPickLocking;


        if (isCharacterInProhibitedAreaCheck || isUsedItemProhibitedCheck || isCharacterWantedCheck(seenCharacterManager) || isCharacterLockpicking) {

            focusAlarmCharacter = seenCharacterManager; // character che ha fatto cambiare lo stato dell'Base NPC Behaviour


            setAlert(CharacterAlertState.HostilityAlert);


            // aggiungi character al dizionario dei character ostili ricercati
            // se non è già contenuto nel dizionario dei character ostili ricercati
            if (!_wantedHostileCharacters.ContainsKey(seenCharacterManager.GetInstanceID())) {
                _wantedHostileCharacters.Add(seenCharacterManager.GetInstanceID(), seenCharacterManager);
            }

        } else {

            focusAlarmCharacter = null;
            if (_characterState == CharacterAlertState.SuspiciousAlert || _characterState == CharacterAlertState.Unalert) {
                setAlert(CharacterAlertState.Unalert);
            }
        }
    }



    /// <summary>
    /// Questa funzione setta il punto di fine del suspiciousTimerLoop
    /// e avvia il suspiciousTimerLoop
    /// </summary>
    private void startSuspiciousTimer() {
        stopAgent(); // stop task agent
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
        stopAgent(); // stop task agent

        hostilityTimerEndStateValue = Time.time + hostilityTimerValue;
        hostilityTimerLoop();
    }
    /// <summary>
    /// Questa funzione resetta il punto di fine del hostilityTimerEndStateValue usato nel loop hostilityTimerLoop
    /// </summary>
    private void resetHostilityTimer() {
        

        hostilityTimerEndStateValue = Time.time + hostilityTimerValue;
    }

    public void stopSuspiciousTimer() {
        suspiciousTimerEndStateValue = 0;
    }
    public void stopHostilityCheckTimer() {
        hostilityTimerEndStateValue = 0;
    }

    
    private async void suspiciousTimerLoop() {
        

        while (Time.time < suspiciousTimerEndStateValue) {
            await Task.Yield();
        }

        if(characterAlertState != CharacterAlertState.HostilityAlert && characterAlertState == CharacterAlertState.SuspiciousAlert) {
            setAlert(CharacterAlertState.Unalert);
        }
        

        // TODO
        // rimozione del alarmFocusCharacter
    }
    private async void hostilityTimerLoop() {
        
        while (Time.time < hostilityTimerEndStateValue) {
            await Task.Yield();
        }

        if (characterAlertState == CharacterAlertState.HostilityAlert) {
            setAlert(CharacterAlertState.Unalert);
        }

        // TODO

        // rimozione del alarmFocusCharacter

        // aggiorna dizionari ostilità
        if(!gameObject.GetComponent<CharacterManager>().isDead) {
            onHostilityAlertTimerEnd();
        }
    }

    /// <summary>
    /// Implementare metodo nelle classi figle se si vuole eseguire una volta che l'hostilityTimerLoop termina
    /// </summary>
    public virtual void onHostilityAlertTimerEnd() {

    }

    /// <summary>
    /// Implementare metodo nelle classi figle se si vuole eseguire quando l'HostilityAlert inizia
    /// </summary>
    public virtual void onHostilityAlert() {
        Dictionary<int, BaseNPCBehaviour> characters = gameObject.GetComponent<CharacterFOV>().getAlertAreaCharacters();


        // aggiorna dizionario dei characters in modo istantaneo
        foreach (var character in characters) {

            character.Value.lastSeenFocusAlarmCharacterPosition = lastSeenFocusAlarmCharacterPosition;
            character.Value.hostilityCheck(focusAlarmCharacter);
        }
    }


    /// <summary>
    /// Resetta animazione punto esclamativo alert del character
    /// </summary>
    void resetAlertAnimatorTrigger() {
        alertSignAnimator.ResetTrigger("suspiciousAlert");
        alertSignAnimator.ResetTrigger("hostilityAlert");
        alertSignAnimator.ResetTrigger("unalertState");
    }


    /// <summary>
    /// Questo metodo verifica se un certo character si trova
    /// all'interno del dizionario wantedHostileCharacters, il dizionario
    /// dei character o stili
    /// </summary>
    /// <param name="character">character da verificare se è all'interno del dizionario</param>
    /// <returns>Torna [true] se il [character] inserito è all'interno del dizionario, altrimenti false </returns>
    bool isCharacterWantedCheck(CharacterManager character) {
        bool result = false;

        if(_wantedHostileCharacters.ContainsKey(character.GetInstanceID())) {
            result = true;
        } else {
            result = false;
        }

        return result;
    }
}
