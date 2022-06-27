using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public enum AgentSpeed { SlowWalk, Walk, Run };

/// <summary>
/// Comportamento dell'npc base classe padre, implementazione astrazione AbstractNPCBehaviour
/// </summary>
public class BaseNPCBehaviourManager : AbstractNPCBehaviour {

    // const
    private const int INTERACTABLE_LAYER = 3;



    // values
    [Header("Configurazione")]
    [SerializeField] protected float suspiciousTimerValue = 15f; // durata del timer prima della scadenza dello stato
    protected float suspiciousTimerEndStateValue = -1f; // timer che indica il valore in cui il suspiciousTimerLoop si stoppa. -1 indica non settato

    [SerializeField] protected float hostilityTimerValue = 15f; // durata del timer prima della scadenza dello stato
    protected float hostilityTimerEndStateValue = -1f; // timer che indica il valore in cui il hostilityTimerLoop si stoppa. -1 indica non settato

    [SerializeField] protected float warningOfSouspiciousTimerValue = 15f; // durata del timer prima della scadenza dello stato
    protected private float warnOfSouspiciousTimerEndStateValue = -1; // timer che indica il valore in cui il warnOfSouspiciousTimerLoop si stoppa. -1 indica non settato

    [SerializeField] protected float suspiciousCorpseFoundTimerValue = 15f; // durata del timer prima della scadenza dello stato
    protected private float suspiciousCorpseFoundTimerEndStateValue = -1; // timer che indica il valore in cui il suspiciousCorpseFoundTimerLoop si stoppa. -1 indica non settato

    [SerializeField] protected float corpseFoundConfirmedTimerValue = 15f; // durata del timer prima della scadenza dello stato
    protected private float corpseFoundConfirmedTimerEndStateValue = -1; // timer che indica il valore in cui il corpseFoundConfirmedTimerLoop si stoppa. -1 indica non settato



    [SerializeField] [Range(0.02f, 0.5f)] private float _cNPCBehaviourCoroutineFrequency = 0.1f;
    public float cNPCBehaviourCoroutineFrequency {
        get { return _cNPCBehaviourCoroutineFrequency; }
    }

    [Header("Configurazione agent")]
    [SerializeField] private float walkAgentSpeed = 3.3f;
    [SerializeField] private float runAgentSpeed = 6.3f;
    

    // states
    [Header("Stati")]
    
    protected CharacterManager _focusAlarmCharacter; // ref del character che ha provocato gli stati di allarme
    public CharacterManager focusAlarmCharacter {
        protected set {
            
            _focusAlarmCharacter = value;
            if (value != null) {
                isFocusedAlarmCharacter = true;
            } else {
                isFocusedAlarmCharacter = false;
            }
        }
        get { return _focusAlarmCharacter; }
    }
    private bool isFocusedAlarmCharacter = false;
    [SerializeField] public Vector3 lastSeenFocusAlarmPosition; // ultima posizione d'allarma comunicata al character
    protected bool _stopCharacterBehaviour = false; // comando che equivale a stoppare il character behaviour
    
    public bool stopCharacterBehaviour {
        get { return _stopCharacterBehaviour; }
    }
    [SerializeField] protected bool characterBehaviourStopped = false; // stato che indica se il character si è stoppato


    // queste variabili indicano se uno stato di allerta è stato innescato da loro stessi(tramiteFOV true) o se è stato indotto(false)
    protected bool checkedByHimselfSuspicious = false;
    public bool isFocusAlarmCharacterVisible {
        get {
            if(focusAlarmCharacter != null) {
                return _characterFOV.isCharactersVisibleInSecondFOV(
                    focusAlarmCharacter,
                    focusAlarmCharacter.characterFOV.recognitionTarget.position);
            } else {
                return false;
            }
        }

    }
    [SerializeField] protected CharacterAlertState _characterState = CharacterAlertState.Unalert; // stato 
    public CharacterAlertState characterAlertState {
        get { return _characterState; }
    }
    
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
    [SerializeField] protected CharacterManager _characterManager;
    public CharacterManager characterManager {
        get { return _characterManager; }
    }
    [SerializeField] protected Animator alertSignAnimator;
    protected CharacterActivityManager characterActivityManager;
    protected CharacterSpawnPoint spawnPoint; // gli spawn point contengono le activities che l'NPC dovrà eseguire
    [SerializeField] protected CharacterMovement _characterMovement; // characterMovement collegato
    [SerializeField] protected NavMeshAgent _agent;
    public NavMeshAgent agent {
        get { return _agent; }
    }
    [SerializeField] protected CharacterFOV _characterFOV;
    public CharacterFOV characterFOV {
        get { return _characterFOV; }
    }
    [SerializeField] protected InventoryManager _characterInventoryManager;
    public InventoryManager characterInventoryManager {
        get { return _characterInventoryManager; }
    }

    [Header("Behaviour process")]
    protected MoveNPCBetweenRandomPointsProcess simulateSearchingPlayerSubBehaviourProcess;
    private GenericUnalertProcess unalertBehaviourProcess;
    protected BehaviourProcess mainBehaviourProcess;
    public void initNPCComponent(CharacterSpawnPoint spawnPoint) {
        this.spawnPoint = spawnPoint;

        this.characterActivityManager = this.spawnPoint.gameObject.GetComponent<CharacterActivityManager>();
    }
    
    
    
    
    
    
    
    public void Start() {

        // inizializza unalert behaviour process
        unalertBehaviourProcess = new GenericUnalertProcess(
            agent,
            this,
            characterActivityManager,
            _characterMovement,
            _characterFOV,
            _characterManager
        );

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
                            hostilityAlertBehaviourAsync();
                        }
                        break;

                    case CharacterAlertState.WarnOfSuspiciousAlert: {
                            warnOfSuspiciousAlertBehaviour();
                        }
                        break;

                    case CharacterAlertState.SuspiciousCorpseFoundAlert: {

                            
                            suspiciousCorpseFoundAlertBehaviour();
                        }
                        break;
                    case CharacterAlertState.CorpseFoundConfirmedAlert: {
                            corpseFoundConfirmedAlertBehaviour();
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

    /// <summary>
    /// cambia lo stato di allerta del character e avvia animazione 
    /// di allerta
    /// </summary>
    /// <param name="alertState"></param>
    protected void setAlert(CharacterAlertState alertState, bool checkedByHimself) {


        CharacterAlertState oldAlertState = _characterState;
        _characterState = alertState;



        // Unalert | WarnOfSuspiciousAlert | SuspiciousCorpseFoundAlert | CorpseFoundConfirmedAlert => (START) SuspiciousAlert
        if (
            (oldAlertState == CharacterAlertState.Unalert ||
            oldAlertState == CharacterAlertState.WarnOfSuspiciousAlert ||
            oldAlertState == CharacterAlertState.SuspiciousCorpseFoundAlert ||
            oldAlertState == CharacterAlertState.CorpseFoundConfirmedAlert)

            &&
            alertState == CharacterAlertState.SuspiciousAlert
        ) {

            stopSuspiciousCorpseFoundTimer();
            stopCorpseFoundConfirmedTimer();
            stopWarnOfSouspiciousTimer();
            startSuspiciousTimer();

            // animation sign
            resetAlertAnimatorTrigger();
            alertSignAnimator.SetTrigger("suspiciousAlert");

        }

        // (CONFIRM) SuspiciousAlert
        if (oldAlertState == CharacterAlertState.SuspiciousAlert && alertState == CharacterAlertState.SuspiciousAlert) {
            stopSuspiciousCorpseFoundTimer();
            stopCorpseFoundConfirmedTimer();
            stopWarnOfSouspiciousTimer();

            resetSuspiciousBehaviour();
        }

        // Unalert | WarnOfSuspiciousAlert | SuspiciousAlert | SuspiciousCorpseFoundAlert | CorpseFoundConfirmedAlert => (START) HostilityAlert
        if (
            (oldAlertState == CharacterAlertState.Unalert ||
            oldAlertState == CharacterAlertState.WarnOfSuspiciousAlert ||
            oldAlertState == CharacterAlertState.SuspiciousAlert ||
            oldAlertState == CharacterAlertState.SuspiciousCorpseFoundAlert ||
            oldAlertState == CharacterAlertState.CorpseFoundConfirmedAlert
            )
            &&
            alertState == CharacterAlertState.HostilityAlert
        ) {

            stopSuspiciousCorpseFoundTimer();
            stopCorpseFoundConfirmedTimer();
            stopWarnOfSouspiciousTimer();
            stopSuspiciousTimer();

            

            startHostilityTimer(checkedByHimself);

            // animation sign
            resetAlertAnimatorTrigger();
            alertSignAnimator.SetTrigger("hostilityAlert");
        }


        // (CONFIRM) HostilityAlert
        if (oldAlertState == CharacterAlertState.HostilityAlert && alertState == CharacterAlertState.HostilityAlert) {

            stopSuspiciousCorpseFoundTimer();
            stopCorpseFoundConfirmedTimer();
            stopWarnOfSouspiciousTimer();
            stopSuspiciousTimer();

            resetHostilityBehaviour();
        }


        // Unalert => (START) WarnOfSouspiciousAlert
        if (oldAlertState == CharacterAlertState.Unalert && alertState == CharacterAlertState.WarnOfSuspiciousAlert) {



            startWarnOfSouspiciousTimer();

            // animation sign
            resetAlertAnimatorTrigger();
            alertSignAnimator.SetTrigger("suspiciousAlert");
        }

        // Unalert => (START) SuspiciousCorpseFoundAlert
        if (oldAlertState == CharacterAlertState.Unalert && alertState == CharacterAlertState.SuspiciousCorpseFoundAlert) {
            stopWarnOfSouspiciousTimer(); // stop stato di warning


            startSuspiciousCorpseFoundTimer();

            // animation sign
            resetAlertAnimatorTrigger();
            alertSignAnimator.SetTrigger("suspiciousAlert");
        }

        // Unalert | SuspiciousCorpseFoundAlert | WarnOfSuspiciousAlert => (START) SuspiciousCorpseFoundAlert
        if ((oldAlertState == CharacterAlertState.Unalert || oldAlertState == CharacterAlertState.SuspiciousCorpseFoundAlert || oldAlertState == CharacterAlertState.WarnOfSuspiciousAlert)
            &&
            alertState == CharacterAlertState.CorpseFoundConfirmedAlert) {

            stopWarnOfSouspiciousTimer(); // stop stato di warning
            stopSuspiciousCorpseFoundTimer();


            startCorpseFoundConfirmedTimer();

            // animation sign
            resetAlertAnimatorTrigger();
            alertSignAnimator.SetTrigger("hostilityAlert");
        }




        if (alertState == CharacterAlertState.Unalert) {

            unalertBehaviourProcess.continueWhereUnalertLeftOff(); // contina da dove aveva lasciato
            resetAlertAnimatorTrigger();// animation sign
            alertSignAnimator.SetTrigger("unalertState");


            focusAlarmCharacter = null;
        }


    }




    /// <summary>
    /// Forza stop coroutine character chiamata asincrona fino a quando il character
    /// non è disattivo
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

    /// stoppa agente e animazione dell'agente che dipende dal move character
    public void stopAgent() {

        if(_agent.enabled) {
            _agent.isStopped = true;
        }
        _characterMovement.stopCharacter();
    }

    public void stopAllCoroutines() {
        StopAllCoroutines();
    }
    


    public override async void unalertBehaviour() {
        await unalertBehaviourProcess.runBehaviourAsyncProcess();
    }

    public override void suspiciousAlertBehaviour() {
        throw new System.NotImplementedException();
    }
    
    public override void hostilityAlertBehaviourAsync() {
        throw new System.NotImplementedException();
    }

    public override void warnOfSuspiciousAlertBehaviour() {
        throw new System.NotImplementedException();
    }

    public override void suspiciousCorpseFoundAlertBehaviour() {
        throw new System.NotImplementedException();
    }
    public override void corpseFoundConfirmedAlertBehaviour() {
        throw new System.NotImplementedException();
    }

    public override void soundAlert1Behaviour() {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Questo subBehaviour fa ruotare il character verso un character
    /// attualmente sotto focus negli stati di Souspicious e Hostility alarm.
    /// Usando l'ultima rotazione in cui è stato avvistato il character oppure usando la
    /// posizione attuale del character se questo è sotto focus di [this]
    /// Inoltre aggiorna l'ultima posizione in cui è stato visto il focus Character
    /// </summary>
    public void rotateAndAimSuspiciousAndHostility() {
        _agent.updateRotation = false;

        if (isFocusedAlarmCharacter) {

            
            if (isFocusAlarmCharacterVisible) {


                Vector3 targetDirection = lastSeenFocusAlarmPosition - gameObject.transform.position;

                if (!isAgentReachedDestination(lastSeenFocusAlarmPosition)) {
                    _characterMovement.rotateCharacter(new Vector2(targetDirection.x, targetDirection.z), false, rotationLerpSpeedValue: RotationLerpSpeedValue.fast);
                }
            } else {

                if (lastSeenFocusAlarmPosition == Vector3.zero) { // solo se il character non è riuscito a prendere la vecchia posizione del character/player

                    float lastPosX = focusAlarmCharacter.transform.position.x;
                    float lastPosY = focusAlarmCharacter.transform.position.y;
                    float lastPosZ = focusAlarmCharacter.transform.position.z;
                    Vector3 noZeroPosition = new Vector3(lastPosX, lastPosY, lastPosZ);
                    lastSeenFocusAlarmPosition = noZeroPosition; // setta ultima posizione in cui è stato visto l'alarm character
                }
                Vector3 targetDirection = lastSeenFocusAlarmPosition - gameObject.transform.position;

                if (!isAgentReachedDestination(lastSeenFocusAlarmPosition)) {
                    _characterMovement.rotateCharacter(new Vector2(targetDirection.x, targetDirection.z), false, rotationLerpSpeedValue: RotationLerpSpeedValue.fast);
                }

            }
        } // se c'è un character focussato durante l'allarme. Il character potrebbe essere più non focussato in quanto non più sospetto

    }

    private void OnTriggerEnter(Collider collision) {


        if (collision.gameObject.layer == INTERACTABLE_LAYER) {


            // le porte venongo aperte dagli NPC solo se non sono morti
            DoorInteractable doorInteractable = collision.gameObject.GetComponent<DoorInteractable>();
            if (doorInteractable != null) {

                if (!collision.gameObject.GetComponent<CharacterManager>().isDead) {
                    if (doorInteractable.doorState.isDoorClosed().value) {
                        doorInteractable.openDoorEvent.Invoke(gameObject.GetComponent<CharacterManager>());
                    }
                }
                
            }
        }
    }

    /// <summary>
    /// Metodo per verificare se un certo character è sospetto agli occhi di [this]
    /// e quindi può entrare nello stato di SuspiciousAlert o meno
    /// </summary>
    /// <param name="seenCharacterManager"></param>
    public override void suspiciousCheck(CharacterManager seenCharacterManager, Vector3 lastSeenCPosition, bool himselfCheck = false)  {


        bool isCharacterInProhibitedAreaCheck = seenCharacterManager.gameObject.GetComponent<CharacterAreaManager>().isCharacterInProhibitedAreaCheck();
        bool isUsedItemProhibitedCheck = seenCharacterManager.gameObject.GetComponent<CharacterManager>().inventoryManager.isUsedItemProhibitedCheck();
        bool isCharacterLockpicking = seenCharacterManager.isPickLocking;

        if (_characterState == CharacterAlertState.Unalert || 
            _characterState == CharacterAlertState.SuspiciousAlert ||
            _characterState == CharacterAlertState.WarnOfSuspiciousAlert ||
            _characterState == CharacterAlertState.SuspiciousCorpseFoundAlert
        ) {

            if (isCharacterInProhibitedAreaCheck || isUsedItemProhibitedCheck || isCharacterWantedCheck(seenCharacterManager) || isCharacterLockpicking) {


                
                focusAlarmCharacter = seenCharacterManager; // character che ha fatto cambiare lo stato dell'Base NPC Behaviour
                lastSeenFocusAlarmPosition = lastSeenCPosition;
                if (seenCharacterManager.isRunning || seenCharacterManager.isWeaponCharacterFiring) { // azioni che confermano istantaneamente l'ostilità nel suspiciousCheck passando direttamente allo stato di HostilityAlert
                    
                    setAlert(CharacterAlertState.HostilityAlert, himselfCheck);
                } else {

                    setAlert(CharacterAlertState.SuspiciousAlert, himselfCheck);
                }

            } else {

                focusAlarmCharacter = null;

            }


        }
        
    }

    /// <summary>
    /// Metodo per verificare se un certo character è ostile agli occhi di [this]
    /// e quindi può entrare nello stato di HostilityAlert o meno, tornando ad unalert nel caso contrario
    /// </summary>
    /// <param name="seenCharacterManager"></param>
    public override void hostilityCheck(CharacterManager seenCharacterManager, Vector3 lastSeenCPosition, bool himselfCheck = false) {


        bool isCharacterInProhibitedAreaCheck = seenCharacterManager.gameObject.GetComponent<CharacterAreaManager>().isCharacterInProhibitedAreaCheck();
        bool isUsedItemProhibitedCheck = seenCharacterManager.gameObject.GetComponent<CharacterManager>().inventoryManager.isUsedItemProhibitedCheck();
        bool isCharacterLockpicking = seenCharacterManager.isPickLocking;

        
        if (isCharacterInProhibitedAreaCheck || isUsedItemProhibitedCheck || isCharacterWantedCheck(seenCharacterManager) || isCharacterLockpicking) {


            focusAlarmCharacter = seenCharacterManager; // character che ha fatto cambiare lo stato dell'Base NPC Behaviour
            lastSeenFocusAlarmPosition = lastSeenCPosition;

            setAlert(CharacterAlertState.HostilityAlert, himselfCheck);


            // aggiungi character al dizionario dei character ostili ricercati
            // se non è già contenuto nel dizionario dei character ostili ricercati
            if (!_wantedHostileCharacters.ContainsKey(seenCharacterManager.GetInstanceID())) {
                _wantedHostileCharacters.Add(seenCharacterManager.GetInstanceID(), seenCharacterManager);
            }

        } else {

            
            if (_characterState == CharacterAlertState.SuspiciousAlert || _characterState == CharacterAlertState.Unalert) {
                focusAlarmCharacter = null;
                setAlert(CharacterAlertState.Unalert, true);
            }
        }
    }


    /// <summary>
    /// Metodo per verificare se [this] può entrare nello stato di WarnOfSuspiciousAlert
    /// </summary>
    /// <param name="lastSeenCPosition"></param>
    public override void receiveWarnOfSouspiciousCheck(Vector3 lastSeenCPosition) {

        // avvia lo stato di SuspiciousCorpseFoundAlert solo quando il character è nello stato Unalert
        // character che è in tutti gli altri stati(compreso WarnOfSuspiciousAlert) non cambiano il loro alert
        if(_characterState != CharacterAlertState.SuspiciousAlert ||
            _characterState != CharacterAlertState.HostilityAlert ||
            _characterState != CharacterAlertState.SuspiciousCorpseFoundAlert ||
            _characterState != CharacterAlertState.CorpseFoundConfirmedAlert
        ) {

            lastSeenFocusAlarmPosition = lastSeenCPosition;
            setAlert(CharacterAlertState.WarnOfSuspiciousAlert, false);
        }
            
    }

    /// <summary>
    /// Metodo per verificare se [this] può entrare nello stato di SuspiciousCorpseFoundAlert
    /// </summary>
    /// <param name="seenCharacterManager"></param>
    /// <param name="lastSeenCPosition"></param>
    public override void suspiciousCorpseFoundCheck(CharacterManager seenDeadCharacter, Vector3 lastSeenCPosition) {



        // avvia lo stato di SuspiciousCorpseFoundAlert solo quando il character è nello stato Unalert
        // character che è in tutti gli altri stati(compreso SuspiciousCorpseFoundAlert) non cambiano il loro alert
        if (_characterState != CharacterAlertState.SuspiciousAlert ||
            _characterState != CharacterAlertState.HostilityAlert
        ) {

            if (!seenDeadCharacter.isBusyDeadAlarmCheck) {

                seenDeadCharacter.isBusyDeadAlarmCheck = true;
                lastSeenFocusAlarmPosition = lastSeenCPosition;
                setAlert(CharacterAlertState.SuspiciousCorpseFoundAlert, true);
            }
        }
    }
    /// <summary>
    /// Metodo per verificare se [this] può entrare nello stato di CorpseFoundConfirmedAlert
    /// </summary>
    /// <param name="seenDeadCharacter"></param>
    /// <param name="lastSeenCPosition"></param>
    public override void corpseFoundConfirmedCheck(CharacterManager seenDeadCharacter, Vector3 lastSeenCPosition) {


        if ((
            _characterState == CharacterAlertState.WarnOfSuspiciousAlert ||
            _characterState == CharacterAlertState.SuspiciousCorpseFoundAlert)
            &&
            (_characterState != CharacterAlertState.SuspiciousAlert ||
            _characterState != CharacterAlertState.HostilityAlert)
        ) {

            if(!seenDeadCharacter.isDeadCharacterMarked) {
                seenDeadCharacter.isBusyDeadAlarmCheck = true;
                lastSeenFocusAlarmPosition = lastSeenCPosition;

                if(gameObject.GetComponent<CharacterRole>().role == Role.EnemyGuard) {
                    seenDeadCharacter.isDeadCharacterMarked = true;
                }

                setAlert(CharacterAlertState.CorpseFoundConfirmedAlert, true);
            }
            

        }
    }


    protected virtual void startCorpseFoundConfirmedTimer() {
        throw new System.NotImplementedException();
    }


    /// <summary>
    /// Questa funzione avvia il warnOfSouspiciousTimerLoop
    /// </summary>
    protected virtual void startSuspiciousCorpseFoundTimer() {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Questa funzione avvia il warnOfSouspiciousTimerLoop
    /// </summary>
    protected virtual void startWarnOfSouspiciousTimer() {
        throw new System.NotImplementedException();
    }
    

    /// <summary>
    /// Questa funzione setta il punto di fine del suspiciousTimerLoop
    /// e avvia il suspiciousTimerLoop
    /// </summary>
    protected virtual void startSuspiciousTimer() {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Questa funzione setta il punto di fine del hostilityTimerEndStateValue
    /// e avvia il hostilityTimerLoop
    /// </summary>
    protected virtual void startHostilityTimer(bool checkedByHimself) {
        throw new System.NotImplementedException();
    }



    /// <summary>
    /// Questa funzione resetta il punto di fine del suspiciousTimerEndStateValue usato nel loop suspiciousTimerLoop
    /// </summary>
    protected virtual void resetSuspiciousBehaviour() {
        throw new System.NotImplementedException();
    }
    /// <summary>
    /// Questa funzione resetta il punto di fine del hostilityTimerEndStateValue usato nel loop hostilityTimerLoop
    /// </summary>
    protected virtual void resetHostilityBehaviour() {

        throw new System.NotImplementedException();
    }

    public void stopSuspiciousCorpseFoundTimer() {
        suspiciousCorpseFoundTimerEndStateValue = 0;
    }
    public void stopCorpseFoundConfirmedTimer() {
        corpseFoundConfirmedTimerEndStateValue = 0;
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

    /// <summary>
    /// Timer loop usato per gestire la durata dello stato suspiciousAlert
    /// </summary>
    protected virtual async void suspiciousTimerLoop() {
        throw new System.NotImplementedException();

    }
    /// <summary>
    /// Timer loop usato per gestire la durata dello stato hostilityAlert
    /// </summary>
    protected virtual async void hostilityTimerLoop() {
        throw new System.NotImplementedException();

    }
    /// <summary>
    /// Timer loop usato per gestire la durata dello stato warnOfSouspiciousAlert
    /// </summary>
    protected virtual async void warnOfSouspiciousTimerLoop() {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Timer loop usato per gestire la durata dello stato suspiciousCorpseFoundAlert
    /// </summary>
    protected virtual async void suspiciousCorpseFoundTimerLoop() {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Timer loop usato per gestire la durata dello stato corpseFoundConfirmedAlert
    /// </summary>
    protected virtual async void corpseFoundConfirmedTimerLoop() {
        throw new System.NotImplementedException();
    }



    /// <summary>
    /// Implementare metodo nelle classi figle se si vuole eseguire una volta che l'hostilityTimerLoop termina
    /// </summary>
    public virtual void onHostilityAlertTimerEnd() {
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
    /// <param name="character">character da verificare se è all'interno del dizionario</param>
    /// <returns>Torna [true] se il [character] inserito è all'interno del dizionario, altrimenti false </returns>
    protected bool isCharacterWantedCheck(CharacterManager character) {
        bool result = false;

        if(_wantedHostileCharacters.ContainsKey(character.GetInstanceID())) {
            result = true;
        } else {
            result = false;
        }

        return result;
    }

    public bool isAgentReachedDestination(Vector3 agentDestinationPosition) {
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
    public bool isAgentReachedEnemyCharacterToWarnDestination(Vector3 agentDestinationPosition) {
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
    /// gestisce animazione e velocità del character in movimento 
    /// </summary>
    /// <param name="agentSpeed"></param>
    public void animateAndSpeedMovingAgent(AgentSpeed agentSpeed = AgentSpeed.Walk) {


        if(agentSpeed == AgentSpeed.Walk) {
            _agent.speed = walkAgentSpeed;
            Vector2 movement = new Vector2(_agent.desiredVelocity.x, _agent.desiredVelocity.z);

            _characterMovement.moveCharacter(movement, false); // avvia solo animazione
            
        } else if(agentSpeed == AgentSpeed.Run) {
            _agent.speed = runAgentSpeed;
            Vector2 movement = new Vector2(_agent.desiredVelocity.x, _agent.desiredVelocity.z);

            _characterMovement.moveCharacter(movement, isRun: true, autoRotationOnRun: false); // avvia solo animazione
        }
        
    }

#if UNITY_EDITOR
    void OnDrawGizmos() {



        if(simulateSearchingPlayerSubBehaviourProcess != null) {

            for(int i = 0; i < simulateSearchingPlayerSubBehaviourProcess.randomNavMeshPositions.Count; i++) {
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(simulateSearchingPlayerSubBehaviourProcess.randomNavMeshPositions[i], 0.4f);
            }
            
        }

    }
#endif
}
