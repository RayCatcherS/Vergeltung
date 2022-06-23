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
    [SerializeField] private float suspiciousTimerValue = 15f;
    private float suspiciousTimerEndStateValue = 0f; // timer che indica il valore in cui il suspiciousTimerLoop si stoppa

    [SerializeField] protected float hostilityTimerValue = 15f;
    protected float hostilityTimerEndStateValue = 0f; // timer che indica il valore in cui il hostilityTimerLoop si stoppa

    [SerializeField] private float warningOfSouspiciousTimerValue = 15f;
    protected private float warnOfSouspiciousTimerEndStateValue = 0;


    [SerializeField] [Range(0.02f, 0.5f)] private float _cNPCBehaviourCoroutineFrequency = 0.1f;
    public float cNPCBehaviourCoroutineFrequency {
        get { return _cNPCBehaviourCoroutineFrequency; }
    }

    [Header("Configurazione agent")]
    [SerializeField] private float walkAgentSpeed = 3.3f;
    [SerializeField] private float runAgentSpeed = 6.3f;
    public enum AgentSpeed { SlowWalk, Walk, Run };

    // states
    [Header("Stati")]
    
    [SerializeField] protected CharacterManager _focusAlarmCharacter; // ref del character che ha provocato gli stati di allarme
    protected CharacterManager focusAlarmCharacter {
        set {
            
            _focusAlarmCharacter = value;
            if (value != null) {
                isFocusedAlarmCharacter = true;
            } else {
                isFocusedAlarmCharacter = false;
            }
        }
        get { return _focusAlarmCharacter; }
    }
    [SerializeField] private bool isFocusedAlarmCharacter = false;
    [SerializeField] public Vector3 lastSeenFocusAlarmPosition; // ultima posizione che � stata visibile del character che ha provocato gli stati di allarme
    [SerializeField] protected bool _stopCharacterBehaviour = false; // comando che equivale a stoppare il character behaviour
    
    public bool stopCharacterBehaviour {
        get { return _stopCharacterBehaviour; }
    }
    [SerializeField] protected bool characterBehaviourStopped = false; // stato che indica se il character si � stoppato


    // queste variabili indicano se uno stato di allerta � stato innescato da loro stessi(tramiteFOV true) o se � stato indotto(false)
    [SerializeField] protected bool checkedByHimselfHostility = false; 
    [SerializeField] protected bool checkedByHimselfSuspicious = false;

    protected bool isFocusAlarmCharacterVisible {
        get {
            if(focusAlarmCharacter != null) {
                return _characterFOV.isCharactersVisibleInSecondFOV(focusAlarmCharacter.characterFOV.recognitionTarget.position);
            } else {
                return false;
            }
        }

    }
    [SerializeField] protected CharacterAlertState _characterState = CharacterAlertState.Unalert; // stato 
    public CharacterAlertState characterAlertState {
        get { return _characterState; }
    }
    [SerializeField] protected bool unalertAgentDestinationSetted = false;
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
    [SerializeField] public CharacterManager characterManager;
    [SerializeField] protected Animator alertSignAnimator;
    protected CharacterActivityManager characterActivityManager;
    protected CharacterSpawnPoint spawnPoint; // gli spawn point contengono le activities che l'NPC dovr� eseguire
    protected CharacterMovement characterMovement; // characterMovement collegato
    [SerializeField] protected NavMeshAgent _agent;
    public NavMeshAgent agent {
        get { return _agent; }
    }
    [SerializeField] protected CharacterFOV _characterFOV;
    public CharacterFOV characterFOV {
        get { return _characterFOV; }
    }
    [SerializeField] protected InventoryManager characterInventoryManager;






    /// <summary>
    /// Forza stop coroutine character chiamata asincrona fino a quando il character
    /// non � disattivo
    /// </summary>
    /// <returns></returns>
    public async Task forceStopCharacterAndAwaitStopProcess() {

        _stopCharacterBehaviour = true;
        while (true) {
            await Task.Yield();

            if (characterBehaviourStopped) {
                break;
            }
        }

        return;
    }
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
            if(characterBehaviourStopped) {
                Debug.Log("stopping coroutine character behaviour ");
                break;
            }

            yield return new WaitForSeconds(_cNPCBehaviourCoroutineFrequency);
            nPCBehaviour();
            
        }
    }
    /// <summary>
    /// Switch dei behaviour
    /// </summary>
    private void nPCBehaviour() {

        if (_stopCharacterBehaviour) {
            characterBehaviourStopped = true;
            Debug.Log("Stopping character");

        } else {

            if (!gameObject.GetComponent<CharacterManager>().isDead) {
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
                            onHostilityAlert();
                        }
                        break;

                    case CharacterAlertState.WarnOfSuspiciousAlert: {
                            warnOfSouspiciousAlertBehaviour();
                        }
                        break;

                    case CharacterAlertState.SuspiciousCorpseFoundAlert: {
                            suspiciousCorpseFoundAlertBehaviour();
                        }
                        break;
                    case CharacterAlertState.SoundAlert1: {
                            soundAlert1Behaviour();
                        }
                        break;
                }
            }
        }

    }

    // stoppa agente e animazione dell'agente che dipende dal move character
    public void stopAgent() {

        if(_agent.enabled) {
            _agent.isStopped = true;
        }
        characterMovement.stopCharacter();
    }

    public void stopAllCoroutines() {
        StopAllCoroutines();
    }


    //controllare un po' tutto


    /// <summary>
    /// cambia lo stato di allerta del character e avvia animazione 
    /// di allerta
    /// </summary>
    /// <param name="alertState"></param>
    protected void setAlert(CharacterAlertState alertState) {

        
        CharacterAlertState oldAlertState = _characterState;
        _characterState = alertState;

        // se avviene la richiesta di warn of souspiciousAlert e lo stato precedente era di souspiciousAlert, il character resta in SuspiciousAlert
        if (alertState == CharacterAlertState.WarnOfSuspiciousAlert && oldAlertState == CharacterAlertState.SuspiciousAlert) {

            _characterState = CharacterAlertState.SuspiciousAlert;
        }

        // se avviene la richiesta di warn of HostilityAlert e lo stato precedente era di HostilityAlert, il character resta in HostilityAlert
        if (alertState == CharacterAlertState.WarnOfSuspiciousAlert && oldAlertState == CharacterAlertState.HostilityAlert) {

            _characterState = CharacterAlertState.HostilityAlert;
        }

        // se avviene la richiesta di souspiciousCorpseFoundAlert e lo stato precedente era di souspiciousAlert, il character resta in SuspiciousAlert
        if (alertState == CharacterAlertState.SuspiciousCorpseFoundAlert && oldAlertState == CharacterAlertState.SuspiciousAlert) {
            _characterState = CharacterAlertState.SuspiciousAlert;
        }

        // se avviene la richiesta di souspiciousCorpseFoundAlert e lo stato precedente era di souspiciousAlert, il character resta in SuspiciousAlert
        if (alertState == CharacterAlertState.SuspiciousCorpseFoundAlert && oldAlertState == CharacterAlertState.HostilityAlert) {
            _characterState = CharacterAlertState.HostilityAlert;
        }



        if (
            (oldAlertState == CharacterAlertState.Unalert ||
            oldAlertState == CharacterAlertState.WarnOfSuspiciousAlert ||
            oldAlertState == CharacterAlertState.SuspiciousCorpseFoundAlert /*||*/
            /*oldAlertState == CharacterAlertState.SuspiciousCorpseFoundConfirmedAlert */)

            && 
            alertState == CharacterAlertState.SuspiciousAlert
        ) { // Unalert | WarnOfSuspiciousAlert | SuspiciousCorpseFoundAlert | SuspiciousCorpseFoundConfirmedAlert => (START) SuspiciousAlert

            //stopSuspiciousCorpseFoundConfirmedTimer();
            //stopSuspiciousCorpseFoundTimer();
            stopWarnOfSouspiciousTimer();
            startSuspiciousTimer();

            // animation sign
            resetAlertAnimatorTrigger();
            alertSignAnimator.SetTrigger("suspiciousAlert");

        }
        if(oldAlertState == CharacterAlertState.SuspiciousAlert && alertState == CharacterAlertState.SuspiciousAlert) { // (CONFIRM) SuspiciousAlert


            stopWarnOfSouspiciousTimer();
            resetSuspiciousTimer();
        }
        if(
            (oldAlertState == CharacterAlertState.Unalert || 
            oldAlertState == CharacterAlertState.WarnOfSuspiciousAlert ||
            oldAlertState == CharacterAlertState.SuspiciousAlert ||
            oldAlertState == CharacterAlertState.SuspiciousCorpseFoundAlert /*||*/
            /*oldAlertState == CharacterAlertState.SuspiciousCorpseFoundConfirmedAlert */
            )
            && 
            alertState == CharacterAlertState.HostilityAlert
        ) { // Unalert | WarnOfSuspiciousAlert | SuspiciousAlert | SuspiciousCorpseFoundAlert | SuspiciousCorpseFoundConfirmedAlert => (START) HostilityAlert

            //stopSuspiciousCorpseFoundConfirmedTimer();
            //stopSuspiciousCorpseFoundTimer();
            stopWarnOfSouspiciousTimer();
            stopSuspiciousTimer();
            startHostilityTimer();

            // animation sign
            resetAlertAnimatorTrigger();
            alertSignAnimator.SetTrigger("hostilityAlert");
        }
        if(oldAlertState == CharacterAlertState.HostilityAlert && alertState == CharacterAlertState.HostilityAlert) { // (CONFIRM) HostilityAlert

            stopWarnOfSouspiciousTimer();
            stopSuspiciousTimer();
            resetHostilityTimer();
        }
        if(oldAlertState == CharacterAlertState.Unalert && alertState == CharacterAlertState.WarnOfSuspiciousAlert) { // Unalert => (START) WarnOfSouspiciousAlert



            startWarnOfSouspiciousTimer();

            // animation sign
            resetAlertAnimatorTrigger();
            alertSignAnimator.SetTrigger("suspiciousAlert");
        }
        

        if (alertState == CharacterAlertState.Unalert) {
            
            initUnalertState(); // inizializza comportamento di unalert
            resetAlertAnimatorTrigger();// animation sign
            alertSignAnimator.SetTrigger("unalertState");


            focusAlarmCharacter = null;
        }


    }


    /// <summary>
    /// Questa funzione implementa il comportamento di unalertBehaviour
    /// Vengono selezionate delle activity in modo casuale e vengono portati a termine tutti i task
    /// </summary>
    public override async void unalertBehaviour() {
        _agent.updateRotation = true; // ruota il character in base alla direzione da raggiungere

        _agent.isStopped = false;

        
        if (characterActivityManager.getCharacterActivities().Count > 0) {

            
            if (unalertAgentDestinationSetted == false) {

                updateUnalertAgentTarget();
            

                unalertAgentDestinationSetted = true;
            } else {



                if (!gameObject.GetComponent<CharacterManager>().isBusy) {


                    Vector3 agentDestinationPosition = characterActivityManager.getCurrentTask().getTaskDestination();
                    if (!isAgentReachedDestination(agentDestinationPosition)) { // controlla se � stata raggiunta la destinazione

                        animateAndSpeedMovingAgent();
                        

                    } else { // task raggiunto

                        
                        // esegui task ed attendi task
                        await characterActivityManager.getCurrentTask().executeTask(
                            gameObject.GetComponent<CharacterManager>(),
                            this,
                            characterMovement
                        );
                        //Debug.Log("task eseguito");


                        if (characterActivityManager.isActualActivityLastTask()) { // se dell'attivit� attuale � l'ultimo task

                            
                            if(characterActivityManager.getCharacterActivities().Count > 1) { // se le attivit� sono pi� di una

                                characterActivityManager.randomCharacterActivity(); // scegli nuova attivit� e parti dal primo task
                                updateUnalertAgentTarget();

                            } else { // se l'attivit� � unica

                                

                                // Debug.Log("solo una attivit�");
                                if (characterActivityManager.getCurrentCharacterActivity().loopActivity) { // se l'attivit� � ripetibile

                                    characterActivityManager.resetSelectedTaskPos(); // scegli nuova attivit� e parti dal primo task
                                    updateUnalertAgentTarget();

                                } else {
                                    
                                    stopAgent(); // resta fermo
                                }
                                
                            }

                        } else { // se non � l'ultimo task dell'attivit� attuale

                            // Debug.Log("passa alla prossima attivit�");
                            characterActivityManager.setNextTaskPosOfActualActivity(); // setta in nuovo task della attivit� corrente
                            updateUnalertAgentTarget();

                        }
                        
                    }
                } else {
                    stopAgent(); // resta fermo
                }
            }
        }
        

    }
    
    private void updateUnalertAgentTarget() {
        if (!gameObject.GetComponent<CharacterManager>().isDead) {
            _agent.SetDestination(
                characterActivityManager.getCurrentTask().getTaskDestination()
            );
        }

    }


    
    public override void suspiciousAlertBehaviour() {
        throw new System.NotImplementedException();
    }
    
    public override void hostilityAlertBehaviour() {
        throw new System.NotImplementedException();
    }

    public override void warnOfSouspiciousAlertBehaviour() {
        throw new System.NotImplementedException();
    }

    
    public override void soundAlert1Behaviour() {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Questo subBehaviour fa ruotare il character verso un character
    /// attualmente sotto focus.
    /// Inoltre aggiorna l'ultima posizione in cui � stato visto il focus Character
    /// </summary>
    protected void rotateAndAimSubBehaviour() {
        _agent.updateRotation = false;

        if (isFocusedAlarmCharacter) {

            
            if (isFocusAlarmCharacterVisible) {


                Vector3 targetDirection = lastSeenFocusAlarmPosition - gameObject.transform.position;

                if (!isAgentReachedDestination(lastSeenFocusAlarmPosition)) {
                    characterMovement.rotateCharacter(new Vector2(targetDirection.x, targetDirection.z), false, rotationLerpSpeedValue: RotationLerpSpeedValue.fast);
                }
            } else {

                if (lastSeenFocusAlarmPosition == Vector3.zero) { // solo se il character non � riuscito a prendere la vecchia posizione del character/player

                    float lastPosX = focusAlarmCharacter.transform.position.x;
                    float lastPosY = focusAlarmCharacter.transform.position.y;
                    float lastPosZ = focusAlarmCharacter.transform.position.z;
                    Vector3 noZeroPosition = new Vector3(lastPosX, lastPosY, lastPosZ);
                    lastSeenFocusAlarmPosition = noZeroPosition; // setta ultima posizione in cui � stato visto l'alarm character
                }
                Vector3 targetDirection = lastSeenFocusAlarmPosition - gameObject.transform.position;

                if (!isAgentReachedDestination(lastSeenFocusAlarmPosition)) {
                    characterMovement.rotateCharacter(new Vector2(targetDirection.x, targetDirection.z), false, rotationLerpSpeedValue: RotationLerpSpeedValue.fast);
                }

            }
        } // se c'� un character focussato durante l'allarme. Il character potrebbe essere pi� non focussato in quanto non pi� sospetto

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
    /// Metodo per verificare se un certo character � sospetto agli occhi di [this]
    /// e quindi pu� entrare nello stato di SuspiciousAlert o meno
    /// </summary>
    /// <param name="seenCharacterManager"></param>
    public override void suspiciousCheck(CharacterManager seenCharacterManager, Vector3 lastSeenCPosition, bool himselfCheck = false)  {


        bool isCharacterInProhibitedAreaCheck = seenCharacterManager.gameObject.GetComponent<CharacterAreaManager>().isCharacterInProhibitedAreaCheck();
        bool isUsedItemProhibitedCheck = seenCharacterManager.gameObject.GetComponent<CharacterManager>().inventoryManager.isUsedItemProhibitedCheck();
        bool isCharacterLockpicking = seenCharacterManager.isPickLocking;

        if (_characterState == CharacterAlertState.Unalert || _characterState == CharacterAlertState.SuspiciousAlert || _characterState == CharacterAlertState.WarnOfSuspiciousAlert) {

            if (isCharacterInProhibitedAreaCheck || isUsedItemProhibitedCheck || isCharacterWantedCheck(seenCharacterManager) || isCharacterLockpicking) {


                
                focusAlarmCharacter = seenCharacterManager; // character che ha fatto cambiare lo stato dell'Base NPC Behaviour
                lastSeenFocusAlarmPosition = lastSeenCPosition;
                if (seenCharacterManager.isRunning || seenCharacterManager.isWeaponCharacterFiring) { // azioni che confermano istantaneamente l'ostilit� nel suspiciousCheck passando direttamente allo stato di HostilityAlert

                    if (himselfCheck) {
                        checkedByHimselfHostility = himselfCheck;
                    }
                    setAlert(CharacterAlertState.HostilityAlert);
                } else {
                    if (himselfCheck) {
                        checkedByHimselfSuspicious = himselfCheck;
                    }
                    setAlert(CharacterAlertState.SuspiciousAlert);
                }

            } else {

                focusAlarmCharacter = null;

            }


        }
        
    }

    /// <summary>
    /// Metodo per verificare se un certo character � ostile agli occhi di [this]
    /// e quindi pu� entrare nello stato di HostilityAlert o meno, tornando ad unalert nel caso contrario
    /// </summary>
    /// <param name="seenCharacterManager"></param>
    public override void hostilityCheck(CharacterManager seenCharacterManager, Vector3 lastSeenCPosition, bool himselfCheck = false) {

        


        bool isCharacterInProhibitedAreaCheck = seenCharacterManager.gameObject.GetComponent<CharacterAreaManager>().isCharacterInProhibitedAreaCheck();
        bool isUsedItemProhibitedCheck = seenCharacterManager.gameObject.GetComponent<CharacterManager>().inventoryManager.isUsedItemProhibitedCheck();
        bool isCharacterLockpicking = seenCharacterManager.isPickLocking;

        
        if (isCharacterInProhibitedAreaCheck || isUsedItemProhibitedCheck || isCharacterWantedCheck(seenCharacterManager) || isCharacterLockpicking) {


            if (himselfCheck) {
                checkedByHimselfHostility = himselfCheck;
            }
            focusAlarmCharacter = seenCharacterManager; // character che ha fatto cambiare lo stato dell'Base NPC Behaviour
            lastSeenFocusAlarmPosition = lastSeenCPosition;


            setAlert(CharacterAlertState.HostilityAlert);


            // aggiungi character al dizionario dei character ostili ricercati
            // se non � gi� contenuto nel dizionario dei character ostili ricercati
            if (!_wantedHostileCharacters.ContainsKey(seenCharacterManager.GetInstanceID())) {
                _wantedHostileCharacters.Add(seenCharacterManager.GetInstanceID(), seenCharacterManager);
            }

        } else {

            
            if (_characterState == CharacterAlertState.SuspiciousAlert || _characterState == CharacterAlertState.Unalert) {
                focusAlarmCharacter = null;
                setAlert(CharacterAlertState.Unalert);
            }
        }
    }


    /// <summary>
    /// Metodo per verificare se [this] pu� entrare nello stato di WarnOfSuspiciousAlert
    /// </summary>
    /// <param name="lastSeenCPosition"></param>
    public override void receiveWarnOfSouspiciousCheck(Vector3 lastSeenCPosition) {
        
        lastSeenFocusAlarmPosition = lastSeenCPosition;
        setAlert(CharacterAlertState.WarnOfSuspiciousAlert);
    }


    public override void suspiciousCorpseFoundCheck(CharacterManager seenCharacterManager, Vector3 lastSeenCPosition) {



        // avvia lo stato di SuspiciousCorpseFoundAlert solo quando il character � nello stato Unalert
        if (_characterState == CharacterAlertState.Unalert) {
            lastSeenFocusAlarmPosition = lastSeenCPosition;
            setAlert(CharacterAlertState.SuspiciousCorpseFoundAlert);
        }
    }

    /// <summary>
    /// Questa funzione setta il punto di fine del warnOfSouspiciousTimerEndStateValue
    /// e avvia il warnOfSouspiciousTimerLoop
    /// </summary>
    protected virtual void startSuspiciousCorpseFoundTimer() {

        stopAgent(); // stop task agent

        //suspiciousCorpseFoundTimerLoop();
    }

    /// <summary>
    /// Questa funzione setta il punto di fine del warnOfSouspiciousTimerEndStateValue
    /// e avvia il warnOfSouspiciousTimerLoop
    /// </summary>
    protected virtual void startWarnOfSouspiciousTimer() {

        stopAgent(); // stop task agent
        
        warnOfSouspiciousTimerLoop();
    }
    

    /// <summary>
    /// Questa funzione setta il punto di fine del suspiciousTimerLoop
    /// e avvia il suspiciousTimerLoop
    /// </summary>
    private void startSuspiciousTimer() {
        stopAgent(); // stop task agent
        
        suspiciousTimerLoop();
    }

    /// <summary>
    /// 
    /// 
    /// 
    /// 
    /// Questa funzione setta il punto di fine del hostilityTimerEndStateValue
    /// e avvia il hostilityTimerLoop
    /// </summary>
    protected virtual void startHostilityTimer() {


        stopAgent(); // stop task agent


        hostilityTimerLoop();
    }
    /// <summary>
    /// Questa funzione resetta il punto di fine del suspiciousTimerEndStateValue usato nel loop suspiciousTimerLoop
    /// </summary>
    private void resetSuspiciousTimer() {

        suspiciousTimerEndStateValue = Time.time + suspiciousTimerValue;
    }

    
    /// <summary>
    /// Questa funzione resetta il punto di fine del hostilityTimerEndStateValue usato nel loop hostilityTimerLoop
    /// </summary>
    private void resetHostilityTimer() {
        

        hostilityTimerEndStateValue = Time.time + hostilityTimerValue;
    }

    public void stopWarnOfSouspiciousTimer() {
        warnOfSouspiciousTimerEndStateValue = 0;
    }
    public void stopSuspiciousTimer() {
        suspiciousTimerEndStateValue = 0;
    }
    public void stopHostilityCheckTimer() {
        hostilityTimerEndStateValue = 0;
    }

    
    private async void suspiciousTimerLoop() {
        // aspetta fino a quando non � stato raggiunto il [lastSeenFocusAlarmCharacterPosition]
        while (!isAgentReachedAlarmDestination(lastSeenFocusAlarmPosition)) {
            await Task.Yield();
            if (characterBehaviourStopped) {
                break;
            }
        }

        suspiciousTimerEndStateValue = Time.time + suspiciousTimerValue;
        while (Time.time < suspiciousTimerEndStateValue) {
            await Task.Yield();

            if (characterBehaviourStopped) {
                break;
            }
        }

        if(characterAlertState != CharacterAlertState.HostilityAlert && characterAlertState == CharacterAlertState.SuspiciousAlert) {
            setAlert(CharacterAlertState.Unalert);
        }
        
    }
    protected virtual async void hostilityTimerLoop() {


        // aspetta fino a quando non � stato raggiunto il [lastSeenFocusAlarmCharacterPosition]
        while (!isAgentReachedAlarmDestination(lastSeenFocusAlarmPosition)) {
            await Task.Yield();
            if (characterBehaviourStopped) {
                break;
            }
        }

        hostilityTimerEndStateValue = Time.time + hostilityTimerValue;
        while (Time.time < hostilityTimerEndStateValue) {
            await Task.Yield();

            if(characterBehaviourStopped) {
                break;
            }
        }

        if (characterAlertState == CharacterAlertState.HostilityAlert) {
            setAlert(CharacterAlertState.Unalert);
        }



        // aggiorna dizionari ostilit� solo se il character non � stoppato
        if (!characterBehaviourStopped) {
            if (!gameObject.GetComponent<CharacterManager>().isDead) {
                onHostilityAlertTimerEnd();
            }
        }
        
    }
    private async void warnOfSouspiciousTimerLoop() {

        // aspetta fino a quando non � stato raggiunto il [lastSeenFocusAlarmCharacterPosition]
        while (!isAgentReachedAlarmDestination(lastSeenFocusAlarmPosition)) {
            await Task.Yield();
            if (characterBehaviourStopped) {
                break;
            }
        }

        warnOfSouspiciousTimerEndStateValue = Time.time + warningOfSouspiciousTimerValue;
        while (Time.time < warnOfSouspiciousTimerEndStateValue) {
            await Task.Yield();

            if (characterBehaviourStopped) {
                break;
            }
        }
        
        stopAgent();


        


        if (characterAlertState == CharacterAlertState.WarnOfSuspiciousAlert) {
            setAlert(CharacterAlertState.Unalert);
        }
    }
    

    private void initUnalertState() {
        unalertAgentDestinationSetted = false;

        checkedByHimselfHostility = false;
        checkedByHimselfSuspicious = false;
    }


    /// <summary>
    /// Implementare metodo nelle classi figle se si vuole eseguire una volta che l'hostilityTimerLoop termina
    /// </summary>
    public virtual void onHostilityAlertTimerEnd() {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Implementare metodo nelle classi figle se si vuole eseguire quando l'HostilityAlert inizia
    /// </summary>
    public virtual void onHostilityAlert() {

        throw new System.NotImplementedException();

    }


    /// <summary>
    /// Resetta animazione punto esclamativo alert del character
    /// </summary>
    void resetAlertAnimatorTrigger() {
        alertSignAnimator.ResetTrigger("suspiciousAlert");
        alertSignAnimator.ResetTrigger("hostilityAlert");
        alertSignAnimator.ResetTrigger("unalertState");
    }

    public void stopAlertAnimator() {
        alertSignAnimator.ResetTrigger("suspiciousAlert");
        alertSignAnimator.ResetTrigger("hostilityAlert");
        alertSignAnimator.ResetTrigger("unalertState");
        alertSignAnimator.SetTrigger("unalertState");
    }


    /// <summary>
    /// Questo metodo verifica se un certo character si trova
    /// all'interno del dizionario wantedHostileCharacters, il dizionario
    /// dei character o stili
    /// </summary>
    /// <param name="character">character da verificare se � all'interno del dizionario</param>
    /// <returns>Torna [true] se il [character] inserito � all'interno del dizionario, altrimenti false </returns>
    protected bool isCharacterWantedCheck(CharacterManager character) {
        bool result = false;

        if(_wantedHostileCharacters.ContainsKey(character.GetInstanceID())) {
            result = true;
        } else {
            result = false;
        }

        return result;
    }

    protected bool isAgentReachedDestination(Vector3 agentDestinationPosition) {
        float distance = Vector3.Distance(transform.position, agentDestinationPosition);
        bool result;

        if (distance > _agent.stoppingDistance) {
            result = false;
        } else {
            result = true;
        }
        return result;
    }
    protected bool isAgentReachedAlarmDestination(Vector3 agentDestinationPosition) {
        float distance = Vector3.Distance(transform.position, agentDestinationPosition);
        bool result;

        if (distance > 3) {
            result = false;
        } else {
            result = true;
        }
        return result;
    }
    protected bool isAgentReachedEnemyCharacterToWarnDestination(Vector3 agentDestinationPosition) {
        float distance = Vector3.Distance(transform.position, agentDestinationPosition);
        bool result;

        if (distance > _characterFOV.alertArea) {
            result = false;
        } else {
            result = true;
        }
        return result;
    }



    /// <summary>
    /// avvia animazione in base a [agentSpeed] (corsa o camminata lenta)
    /// gestisce animazione e velocit� del character in movimento 
    /// </summary>
    /// <param name="agentSpeed"></param>
    protected void animateAndSpeedMovingAgent(AgentSpeed agentSpeed = AgentSpeed.Walk) {
        


        if(agentSpeed == AgentSpeed.Walk) {
            _agent.speed = walkAgentSpeed;
            Vector2 movement = new Vector2(_agent.desiredVelocity.x, _agent.desiredVelocity.z);

            characterMovement.moveCharacter(movement, false); // avvia solo animazione
            
        } else if(agentSpeed == AgentSpeed.Run) {
            _agent.speed = runAgentSpeed;
            Vector2 movement = new Vector2(_agent.desiredVelocity.x, _agent.desiredVelocity.z);

            characterMovement.moveCharacter(movement, isRun: true, autoRotationOnRun: false); // avvia solo animazione
        }
        
    }

    public override void suspiciousCorpseFoundAlertBehaviour() {
        throw new System.NotImplementedException();
    }
}
