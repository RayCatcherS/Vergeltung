using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public enum AgentSpeed { SlowWalk, Walk, Run };

/// <summary>
/// Comportamento dell'npc base classe padre, implementazione astrazione AbstractNPCBehaviour
/// </summary>
public class BaseNPCBehaviourManager : AbstractNPCBehaviour {

    // const
    private const int INTERACTABLE_LAYER = 3;
    private const int DOOR_LAYER = 10;


    // values
    [Header("Configurazione")]
    [SerializeField] protected float suspiciousTimerValue = 15f; // durata del timer prima della scadenza dello stato
    protected float suspiciousTimerEndStateValue = -1f; // timer che indica il valore in cui il suspiciousTimerLoop si stoppa. -1 indica non settato

    [SerializeField] protected float hostilityTimerValue = 15f; // durata del timer prima della scadenza dello stato
    protected float hostilityTimerEndStateValue = -1f; // timer che indica il valore in cui il hostilityTimerLoop si stoppa. -1 indica non settato

    [SerializeField] protected float warningOfSouspiciousTimerValue = 10; // durata del timer prima della scadenza dello stato
    protected private float warnOfSouspiciousTimerEndStateValue = -1; // timer che indica il valore in cui il warnOfSouspiciousTimerLoop si stoppa. -1 indica non settato

    [SerializeField] protected float suspiciousCorpseFoundTimerValue = 10; // durata del timer prima della scadenza dello stato
    protected private float suspiciousCorpseFoundTimerEndStateValue = -1; // timer che indica il valore in cui il suspiciousCorpseFoundTimerLoop si stoppa. -1 indica non settato

    [SerializeField] protected float corpseFoundConfirmedTimerValue = 10; // durata del timer prima della scadenza dello stato
    protected private float corpseFoundConfirmedTimerEndStateValue = -1; // timer che indica il valore in cui il corpseFoundConfirmedTimerLoop si stoppa. -1 indica non settato

    [SerializeField] protected float instantOnCurrentPositionWarnOfSouspiciousTimerValue = 10; // durata del timer prima della scadenza dello stato
    protected private float instantOnCurrentPositionWarnOfSouspiciousTimerEndStateValue = -1; // timer che indica il valore in cui il instantOnCurrentPositionWarnOfSouspiciousTimerLoop si stoppa. -1 indica non settato

    [SerializeField] protected float playerLoudRunSuspiciousTimerValue = 3; // durata del timer prima della scadenza dello stato
    protected private float playerLoudRunSuspiciousTimerEndStateValue = -1; // timer che indica il valore in cui il startstayOnPositionSuspiciousTimerLoop si stoppa. -1 indica non settato


    [SerializeField][Range(0.02f, 0.5f)] private float _cNPCBehaviourCoroutineFrequency = 0.1f;
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
            if(value != null) {
                _isFocusedAlarmCharacter = true;
            } else {
                _isFocusedAlarmCharacter = false;
            }
        }
        get { return _focusAlarmCharacter; }
    }
    private bool _isFocusedAlarmCharacter = false;
    public bool isFocusedAlarmCharacter {
        get{ return _isFocusedAlarmCharacter; }
    }
    protected bool _stopCharacterBehaviour = false; // comando che equivale a stoppare il character behaviour
    
    public bool stopCharacterBehaviour {
        get { return _stopCharacterBehaviour; }
    }
    [SerializeField] public bool characterBehaviourStopped = false; // stato che indica se il character si � stoppato


    // queste variabili indicano se uno stato di allerta � stato innescato da loro stessi(tramiteFOV true) o se � stato indotto(false)
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
    public bool isAgentInMovement {
        get {
            return (agent.velocity != Vector3.zero); }
    }


    // ref
    [Header("Reference")]
    [SerializeField] protected CharacterManager _characterManager;
    public CharacterManager characterManager {
        get { return _characterManager; }
    }
    [SerializeField] protected Animator alertSignAnimator;
    protected CharacterActivityManager characterActivityManager;
    protected CharacterSpawnPoint spawnPoint; // gli spawn point contengono le activities che l'NPC dovr� eseguire
    [SerializeField] protected CharacterMovement _characterMovement; // characterMovement collegato
    public CharacterMovement characterMovement {
        get { return _characterMovement; }
    }
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

        if(!characterBehaviourStopped) {

            if (!gameObject.GetComponent<CharacterManager>().isDead) {
                switch (_characterState) {
                    case CharacterAlertState.Unalert: {

                        _characterFOV.setAlertBonus(false);
                        unalertBehaviour();
                    }
                    break;
                    case CharacterAlertState.SuspiciousAlert: {

                        _characterFOV.setAlertBonus(true);
                        suspiciousAlertBehaviour();
                    }
                    break;
                    case CharacterAlertState.HostilityAlert: {

                        _characterFOV.setAlertBonus(true);
                        hostilityAlertBehaviourAsync();
                    }
                    break;

                    case CharacterAlertState.WarnOfSuspiciousAlert: {

                        _characterFOV.setAlertBonus(true);
                        warnOfSuspiciousAlertBehaviour();
                    }
                    break;

                    case CharacterAlertState.SuspiciousCorpseFoundAlert: {

                        _characterFOV.setAlertBonus(true);
                        suspiciousCorpseFoundAlertBehaviour();
                    }
                    break;
                    case CharacterAlertState.CorpseFoundConfirmedAlert: {

                        _characterFOV.setAlertBonus(true);
                        corpseFoundConfirmedAlertBehaviour();
                    }
                    break;
                    case CharacterAlertState.instantOnCurrentPositionWarnOfSouspiciousAlert: {

                        _characterFOV.setAlertBonus(true);
                        instantOnCurrentPositionWarnOfSouspiciousAlertBehaviour();
                    }
                    break;
                    case CharacterAlertState.playerLoudRunAlert: {
                            
                        _characterFOV.setAlertBonus(true);
                        playerLoudRunSuspiciousAlertBehaviour();
                    }
                    break;
                }
            }
        }
        if (_stopCharacterBehaviour) {
            characterBehaviourStopped = true;
            Debug.Log("Stopping character");

        }

    }
    

    public delegate void Delegate();
    /// <summary>
    /// cambia lo stato di allerta del character e avvia animazione 
    /// di allerta
    /// </summary>
    /// <param name="alertState"></param>
    protected void setAlert(CharacterAlertState alertState, bool checkedByHimself, Delegate actionToExcuteOnChangeAlert = null, Vector3 lastSeenFocusAlarmPosition = new Vector3()) {


        if (!characterManager.isDead) {


            //  Unalert | WarnOfSuspiciousAlert | SuspiciousCorpseFoundAlert | CorpseFoundConfirmedAlert |
            //  instantOnCurrentPositionWarnOfSouspiciousAlert | stayOnPositionSuspiciousAlert => (START) SuspiciousAlert
            if(
                (
                    _characterState == CharacterAlertState.Unalert ||
                    _characterState == CharacterAlertState.WarnOfSuspiciousAlert ||
                    _characterState == CharacterAlertState.SuspiciousCorpseFoundAlert ||
                    _characterState == CharacterAlertState.CorpseFoundConfirmedAlert ||
                    _characterState == CharacterAlertState.instantOnCurrentPositionWarnOfSouspiciousAlert ||
                    _characterState == CharacterAlertState.playerLoudRunAlert
                ) 
                &&
                alertState == CharacterAlertState.SuspiciousAlert
            ) {

                stopWarnOfSouspiciousTimer();
                stopSuspiciousCorpseFoundTimer();
                stopCorpseFoundConfirmedTimer();
                stopInstantOnCurrentPositionWarnOfSouspiciousTimer();
                stopStayOnPositionSuspiciousTimer();

                startSuspiciousTimer(lastSeenFocusAlarmPosition);

                // animation sign
                resetAlertAnimatorTrigger();
                alertSignAnimator.SetTrigger("suspiciousAlert");


                if(actionToExcuteOnChangeAlert != null) {
                    actionToExcuteOnChangeAlert();
                }
                _characterState = alertState;
            }

            // (CONFIRM) SuspiciousAlert
            if (_characterState == CharacterAlertState.SuspiciousAlert && alertState == CharacterAlertState.SuspiciousAlert) {
                

                resetSuspiciousBehaviour(lastSeenFocusAlarmPosition);

                if(actionToExcuteOnChangeAlert != null) {
                    actionToExcuteOnChangeAlert();
                }
                _characterState = alertState;
            }

            // * - {HostilityAlert} => (START) HostilityAlert
            if(
                (
                    _characterState == CharacterAlertState.Unalert ||
                    _characterState == CharacterAlertState.SuspiciousAlert ||
                    _characterState == CharacterAlertState.WarnOfSuspiciousAlert ||
                    _characterState == CharacterAlertState.SuspiciousCorpseFoundAlert ||
                    _characterState == CharacterAlertState.CorpseFoundConfirmedAlert ||
                    _characterState == CharacterAlertState.instantOnCurrentPositionWarnOfSouspiciousAlert ||
                    _characterState == CharacterAlertState.playerLoudRunAlert
                )
                && 
                alertState == CharacterAlertState.HostilityAlert
            ) {

                stopSuspiciousTimer();
                stopWarnOfSouspiciousTimer();
                stopSuspiciousCorpseFoundTimer();
                stopCorpseFoundConfirmedTimer();
                stopInstantOnCurrentPositionWarnOfSouspiciousTimer();
                stopStayOnPositionSuspiciousTimer();


                startHostilityTimer(lastSeenFocusAlarmPosition, checkedByHimself);

                // animation sign
                resetAlertAnimatorTrigger();
                alertSignAnimator.SetTrigger("hostilityAlert");


                if(actionToExcuteOnChangeAlert != null) {
                    actionToExcuteOnChangeAlert();
                }
                _characterState = alertState;
            }


            // (CONFIRM) HostilityAlert
            if (_characterState == CharacterAlertState.HostilityAlert && alertState == CharacterAlertState.HostilityAlert) {


                resetHostilityBehaviour(lastSeenFocusAlarmPosition);

                if(actionToExcuteOnChangeAlert != null) {
                    actionToExcuteOnChangeAlert();
                }
                _characterState = alertState;
            }


            // Unalert => (START) WarnOfSouspiciousAlert
            if (_characterState == CharacterAlertState.Unalert && alertState == CharacterAlertState.WarnOfSuspiciousAlert) {



                startWarnOfSouspiciousTimer(lastSeenFocusAlarmPosition);

                // animation sign
                resetAlertAnimatorTrigger();
                alertSignAnimator.SetTrigger("suspiciousAlert");


                if(actionToExcuteOnChangeAlert != null) {
                    actionToExcuteOnChangeAlert();
                }
                _characterState = alertState;
            }


            // Unalert | WarnOfSuspiciousAlert | instantOnCurrentPositionWarnOfSouspiciousAlert |
            // stayOnPositionSuspiciousAlert  => (START) SuspiciousCorpseFoundAlert
            if(
                (
                    _characterState == CharacterAlertState.Unalert ||
                    _characterState == CharacterAlertState.WarnOfSuspiciousAlert || 
                    _characterState == CharacterAlertState.instantOnCurrentPositionWarnOfSouspiciousAlert ||
                    _characterState == CharacterAlertState.playerLoudRunAlert
                )
                &&
                alertState == CharacterAlertState.SuspiciousCorpseFoundAlert
            ) {

                stopWarnOfSouspiciousTimer();
                stopInstantOnCurrentPositionWarnOfSouspiciousTimer(); // stop stato di warning
                stopStayOnPositionSuspiciousTimer();

                startSuspiciousCorpseFoundTimer(lastSeenFocusAlarmPosition);


                // animation sign
                resetAlertAnimatorTrigger();
                alertSignAnimator.SetTrigger("suspiciousAlert");


                if(actionToExcuteOnChangeAlert != null) {
                    actionToExcuteOnChangeAlert();
                }
                _characterState = alertState;
            }

            // WarnOfSuspiciousAlert | SuspiciousCorpseFoundAlert => (START) CorpseFoundConfirmedAlert
            if(
                ( 
                    _characterState == CharacterAlertState.WarnOfSuspiciousAlert || 
                    _characterState == CharacterAlertState.SuspiciousCorpseFoundAlert
                )
                &&
                alertState == CharacterAlertState.CorpseFoundConfirmedAlert
            ) {

                stopWarnOfSouspiciousTimer();
                stopSuspiciousCorpseFoundTimer();


                startCorpseFoundConfirmedTimer(lastSeenFocusAlarmPosition);


                // animation sign
                resetAlertAnimatorTrigger();
                alertSignAnimator.SetTrigger("hostilityAlert");


                if(actionToExcuteOnChangeAlert != null) {
                    actionToExcuteOnChangeAlert();
                }
                _characterState = alertState;
            }

            // Unalert | WarnOfSuspiciousAlert | stayOnPositionSuspiciousAlert => (START) instantOnCurrentPositionWarnOfSouspiciousAlert
            if(
                (
                    _characterState == CharacterAlertState.Unalert || 
                    _characterState == CharacterAlertState.WarnOfSuspiciousAlert ||
                    _characterState == CharacterAlertState.CorpseFoundConfirmedAlert ||
                    _characterState == CharacterAlertState.playerLoudRunAlert
                    
                )
                && 
                alertState == CharacterAlertState.instantOnCurrentPositionWarnOfSouspiciousAlert
            ) {
                stopWarnOfSouspiciousTimer();
                stopCorpseFoundConfirmedTimer();
                stopStayOnPositionSuspiciousTimer();


                startInstantOnCurrentPositionWarnOfSouspiciousTimer(lastSeenFocusAlarmPosition);

                // animation sign
                resetAlertAnimatorTrigger();
                alertSignAnimator.SetTrigger("suspiciousAlert");


                if(actionToExcuteOnChangeAlert != null) {
                    actionToExcuteOnChangeAlert();
                }
                _characterState = alertState;
            }

            //  => (START) stayOnPositionSuspiciousAlert
            if((_characterState == CharacterAlertState.Unalert) && alertState == CharacterAlertState.playerLoudRunAlert) {
                

                startStayOnPositionSuspiciousTimer(lastSeenFocusAlarmPosition);

                resetAlertAnimatorTrigger();
                alertSignAnimator.SetTrigger("suspiciousAlert");


                if(actionToExcuteOnChangeAlert != null) {
                    actionToExcuteOnChangeAlert();
                }
                _characterState = alertState;
            }



            if (alertState == CharacterAlertState.Unalert) {

                unalertBehaviourProcess.continueWhereUnalertLeftOff(); // continua unalert behv dall'ultimo punto lasciato

                resetAlertAnimatorTrigger();// animation sign
                alertSignAnimator.SetTrigger("unalertState");


                focusAlarmCharacter = null;


                if(actionToExcuteOnChangeAlert != null) {
                    actionToExcuteOnChangeAlert();
                }
                _characterState = alertState;
            }
        }

    }




    /// <summary>
    /// Forza stop coroutine character chiamata asincrona fino a quando il character
    /// non è disattivo
    /// </summary>
    /// <returns></returns>
    public async Task forceStopCharacterAndAwaitStopProcess() {

        Debug.Log("Force stopping");
        _stopCharacterBehaviour = true;
        while (true) {
            await Task.Yield();

            if (characterBehaviourStopped) {
                break;
            }
        }

        return;
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

    public override void instantOnCurrentPositionWarnOfSouspiciousAlertBehaviour() {
        throw new System.NotImplementedException();
    }

    public override void playerLoudRunSuspiciousAlertBehaviour() {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Questo subBehaviour fa ruotare il character verso un character
    /// attualmente sotto focus negli stati di Souspicious e Hostility alarm.
    /// Usando l'ultima rotazione in cui � stato avvistato il character oppure usando la
    /// posizione attuale del character se questo � sotto focus di [this]
    /// Inoltre aggiorna l'ultima posizione in cui � stato visto il focus Character
    /// </summary>
    public void rotateAndAimSuspiciousAndHostility(Vector3 lastSeenFocusAlarmPosition) {
        _agent.updateRotation = false;

        if (_isFocusedAlarmCharacter) {

            
            if (isFocusAlarmCharacterVisible) {


                Vector3 targetDirection = focusAlarmCharacter.transform.position - gameObject.transform.position;

                _characterMovement.rotateCharacter(new Vector2(targetDirection.x, targetDirection.z), false, rotationLerpSpeedValue: RotationLerpSpeedValue.fast);
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
                    _agent.updateRotation = true;
                } else {
                    _characterMovement.rotateCharacter(new Vector2(targetDirection.x, targetDirection.z), false, rotationLerpSpeedValue: RotationLerpSpeedValue.fast);
                }

            }
        } // se c'� un character focussato durante l'allarme. Il character potrebbe essere pi� non focussato in quanto non pi� sospetto

    }

    private void OnTriggerEnter(Collider collider) {


        if (collider.gameObject.layer == INTERACTABLE_LAYER) {


            openFrontDoor(collider);
        }
    }

    private void OnTriggerStay(Collider collider) {
        if (collider.gameObject.layer == INTERACTABLE_LAYER) {


            openFrontDoor(collider);
        }
    }

    private void openFrontDoor(Collider collider) {
        // le porte venongo aperte dagli NPC solo se non sono morti
        DoorInteractable doorInteractable = collider.gameObject.GetComponent<DoorInteractable>();
        if (doorInteractable != null) {

            if (!_characterManager.isDead && !_characterManager.isPlayer && isAgentInMovement) {
                if (doorInteractable.doorState.isDoorClosed().value) {

                    RaycastHit hit;
                    // agent.velocity.normalized indica il vettore direzione dell'agent
                    if (Physics.Raycast(characterFOV.reachableTarget.position, agent.velocity.normalized, out hit, 1.2f, ~(DOOR_LAYER), QueryTriggerInteraction.Ignore)) {

                        if (hit.transform.gameObject.layer == DOOR_LAYER) {
                            Debug.DrawLine(characterFOV.reachableTarget.position, hit.point, Color.black, 0.5f);
                            doorInteractable.openDoorEvent.Invoke(gameObject.GetComponent<CharacterManager>());
                        }
                        
                    }
                        
                }
            }

        }
    }

    /// <summary>
    /// Metodo per verificare se un certo character è sospetto agli occhi di [this]
    /// e quindi può chiamare il cambio di stato su SuspiciousAlert o meno
    /// </summary>
    /// <param name="seenCharacterManager"></param>
    public override void suspiciousCheck(CharacterManager seenCharacterManager, Vector3 lastSeenCPosition, bool himselfCheck = false)  {


        bool isCharacterInProhibitedAreaCheck = seenCharacterManager.gameObject.GetComponent<CharacterAreaManager>().isCharacterInProhibitedAreaCheck();
        bool isUsedItemProhibitedCheck = seenCharacterManager.gameObject.GetComponent<CharacterManager>().inventoryManager.isUsedItemProhibitedCheck();
        bool isCharacterLockpicking = seenCharacterManager.isPickLocking;


        if(isCharacterInProhibitedAreaCheck || isUsedItemProhibitedCheck || isCharacterWantedCheck(seenCharacterManager) || isCharacterLockpicking) {

            
            if(seenCharacterManager.isRunning || seenCharacterManager.isWeaponCharacterFiring) { // azioni che confermano istantaneamente l'ostilit� nel suspiciousCheck passando direttamente allo stato di HostilityAlert

                setAlert(CharacterAlertState.HostilityAlert, himselfCheck, 
                    () => {
                        focusAlarmCharacter = seenCharacterManager; // character che ha fatto cambiare lo stato dell'Base NPC Behaviour
                    },
                    lastSeenFocusAlarmPosition: lastSeenCPosition
                 );
            } else {

                setAlert(CharacterAlertState.SuspiciousAlert, himselfCheck,
                    () => {
                        focusAlarmCharacter = seenCharacterManager; // character che ha fatto cambiare lo stato dell'Base NPC Behaviour
                    },
                    lastSeenFocusAlarmPosition: lastSeenCPosition
                );
            }

        }

    }

    /// <summary>
    /// Metodo per verificare se un certo character è ostile agli occhi di [this]
    /// può chiamare il cambio di stato su HostilityAlert o meno, cercando di andare in unalert nel caso contrario
    /// </summary>
    /// <param name="seenCharacterManager"></param>
    public override void hostilityCheck(CharacterManager seenCharacterManager, Vector3 lastSeenCPosition, bool himselfCheck = false) {


        bool isCharacterInProhibitedAreaCheck = seenCharacterManager.gameObject.GetComponent<CharacterAreaManager>().isCharacterInProhibitedAreaCheck();
        bool isUsedItemProhibitedCheck = seenCharacterManager.gameObject.GetComponent<CharacterManager>().inventoryManager.isUsedItemProhibitedCheck();
        bool isCharacterLockpicking = seenCharacterManager.isPickLocking;

        
        if (isCharacterInProhibitedAreaCheck || isUsedItemProhibitedCheck || isCharacterWantedCheck(seenCharacterManager) || isCharacterLockpicking || seenCharacterManager.isWeaponCharacterFiring) {


            

            setAlert(CharacterAlertState.HostilityAlert, himselfCheck,

                () => {
                    focusAlarmCharacter = seenCharacterManager; // character che ha fatto cambiare lo stato dell'Base NPC Behaviour

                    // aggiungi character al dizionario dei character ostili ricercati
                    // se non è già contenuto nel dizionario dei character ostili ricercati
                    if(!_wantedHostileCharacters.ContainsKey(seenCharacterManager.GetInstanceID())) {
                        _wantedHostileCharacters.Add(seenCharacterManager.GetInstanceID(), seenCharacterManager);
                    }
                },
                lastSeenFocusAlarmPosition: lastSeenCPosition
            );


            

        } else {

            
            if (_characterState == CharacterAlertState.SuspiciousAlert) {
                
                setAlert(CharacterAlertState.Unalert, true, 
                
                    () => {
                        focusAlarmCharacter = null;
                    }
                );
            }
        }
    }


    /// <summary>
    /// Metodo per verificare se [this] può chiamare il cambio di stato su WarnOfSuspiciousAlert
    /// </summary>
    /// <param name="lastSeenCPosition"></param>
    public override void warnOfSouspiciousCheck(Vector3 lastSeenCPosition) {

        
        setAlert(CharacterAlertState.WarnOfSuspiciousAlert, false,
            lastSeenFocusAlarmPosition: lastSeenCPosition
        );

    }

    /// <summary>
    /// Metodo per verificare se [this] può chiamare il cambio di stato su SuspiciousCorpseFoundAlert
    /// </summary>
    /// <param name="seenCharacterManager"></param>
    /// <param name="lastSeenCPosition"></param>
    public override void suspiciousCorpseFoundCheck(CharacterManager seenDeadCharacter, Vector3 lastSeenCPosition) {

        if(!seenDeadCharacter.isBusyDeadAlarmCheck) {

            
            setAlert(CharacterAlertState.SuspiciousCorpseFoundAlert, true,
            
                () => {
                    seenDeadCharacter.isBusyDeadAlarmCheck = true;
                },
                lastSeenFocusAlarmPosition: lastSeenCPosition
            );
        }
    }
    /// <summary>
    /// Metodo per verificare se [this] può chiamare il cambio di stato su CorpseFoundConfirmedAlert
    /// </summary>
    /// <param name="seenDeadCharacter"></param>
    /// <param name="lastSeenCPosition"></param>
    public override void corpseFoundConfirmedCheck(CharacterManager seenDeadCharacter, Vector3 lastSeenCPosition) {


        if(!seenDeadCharacter.isDeadCharacterMarked) {

            setAlert(CharacterAlertState.CorpseFoundConfirmedAlert, true, 
            
                () => {
                    seenDeadCharacter.isBusyDeadAlarmCheck = true;

                    if(gameObject.GetComponent<CharacterRole>().role == Role.EnemyGuard) {
                        seenDeadCharacter.isDeadCharacterMarked = true;
                    }
                },
                lastSeenFocusAlarmPosition: lastSeenCPosition
            );
        }
    }
    /// <summary>
    /// Metodo per verificare se [this] può chiamare il cambio di stato su SuspiciousHitReceived
    /// </summary>
    public override void instantOnCurrentPositionWarnOfSouspiciousCheck(Vector3 lastSeenCPosition) {


        setAlert(CharacterAlertState.instantOnCurrentPositionWarnOfSouspiciousAlert, true, lastSeenFocusAlarmPosition: lastSeenCPosition);
    }

    /// <summary>
    /// Metodo per verificare se [this] può chiamare il cambio di stato su SuspiciousHitReceived
    /// </summary>
    public override void playerLoudRunSuspiciousCheck(CharacterManager characterThatStartAlarm, Vector3 lastSeenCPosition) {


        if(_characterState != CharacterAlertState.Unalert) {

            // se è in qualsiasi altro stato di allerta rispetto ad Unalert aggiorna la lastSeenFocusAlarmPosition
            // del processo attualmente in esecuzione in modo da deviare la ricerca o il cammino di alerBehv verso la fonte della Loud Area
            // generata dalla corsa del character
            mainBehaviourProcess.changeCurrentLastSeenFocusAlarmPosition(lastSeenCPosition);

            if(simulateSearchingPlayerSubBehaviourProcess != null) {
                simulateSearchingPlayerSubBehaviourProcess.changeCurrentLastSeenFocusAlarmPosition(lastSeenCPosition);
            }
            

        } else {

            // altrimenti se il character è nello stato di unalert avvia lo stato playerLoudRunAlert
            setAlert(
                CharacterAlertState.playerLoudRunAlert, true,
                () => {
                    focusAlarmCharacter = characterThatStartAlarm; // character che ha fatto cambiare lo stato dell'Base NPC Behaviour
                },
                lastSeenFocusAlarmPosition: lastSeenCPosition
            );
        }

        
        
    }










    protected virtual void startCorpseFoundConfirmedTimer(Vector3 _lastSeenFocusAlarmPosition) {
        throw new System.NotImplementedException();
    }


    /// <summary>
    /// Questa funzione avvia il startSuspiciousCorpseFoundTimerLoop
    /// </summary>
    protected virtual void startSuspiciousCorpseFoundTimer(Vector3 _lastSeenFocusAlarmPosition) {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Questa funzione avvia il warnOfSouspiciousTimerLoop
    /// </summary>
    protected virtual void startWarnOfSouspiciousTimer(Vector3 _lastSeenFocusAlarmPosition) {
        throw new System.NotImplementedException();
    }
    

    /// <summary>
    /// Questa funzione setta il punto di fine del suspiciousTimerLoop
    /// e avvia il suspiciousTimerLoop
    /// </summary>
    protected virtual void startSuspiciousTimer(Vector3 _lastSeenFocusAlarmPosition) {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Questa funzione setta il punto di fine del hostilityTimerEndStateValue
    /// e avvia il hostilityTimerLoop
    /// </summary>
    protected virtual void startHostilityTimer(Vector3 _lastSeenFocusAlarmPosition, bool checkedByHimself) {
        throw new System.NotImplementedException();
    }
    /// <summary>
    /// Questa funzione setta il punto di fine del InstantOnCurrentPositionWarnOfSouspiciousEndStateValue
    /// e avvia il hostilityTimerLoop
    /// </summary>
    protected virtual void startInstantOnCurrentPositionWarnOfSouspiciousTimer(Vector3 _lastSeenFocusAlarmPosition) {
        throw new System.NotImplementedException();
    }
    /// <summary>
    /// Questa funzione setta il punto di fine del InstantOnCurrentPositionWarnOfSouspiciousEndStateValue
    /// e avvia il hostilityTimerLoop
    /// </summary>
    protected virtual void startStayOnPositionSuspiciousTimer(Vector3 _lastSeenFocusAlarmPosition) {
        throw new System.NotImplementedException();
    }







    /// <summary>
    /// Questa funzione resetta il punto di fine del suspiciousTimerEndStateValue usato nel loop suspiciousTimerLoop
    /// </summary>
    protected virtual void resetSuspiciousBehaviour(Vector3 _lastSeenFocusAlarmPosition) {
        throw new System.NotImplementedException();
    }
    /// <summary>
    /// Questa funzione resetta il punto di fine del hostilityTimerEndStateValue usato nel loop hostilityTimerLoop
    /// </summary>
    protected virtual void resetHostilityBehaviour(Vector3 _lastSeenFocusAlarmPosition) {

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
    public void stopHostilityTimer() {
        hostilityTimerEndStateValue = 0;
    }

    public void stopInstantOnCurrentPositionWarnOfSouspiciousTimer() {
        instantOnCurrentPositionWarnOfSouspiciousTimerEndStateValue = 0;
    }
    public void stopStayOnPositionSuspiciousTimer() {
        playerLoudRunSuspiciousTimerEndStateValue = 0;
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
    /// gestisce animazione e velocit� del character in movimento 
    /// </summary>
    /// <param name="agentSpeed"></param>
    public void animateAndSpeedMovingAgent(AgentSpeed agentSpeed = AgentSpeed.Walk) {


        if (agentSpeed == AgentSpeed.Walk) {
            _agent.speed = walkAgentSpeed;
            Vector2 movement = new Vector2(_agent.desiredVelocity.x, _agent.desiredVelocity.z);

            _characterMovement.moveCharacter(movement, false, onlyAnimation: true); // avvia solo animazione
            
        } else if(agentSpeed == AgentSpeed.Run) {
            _agent.speed = runAgentSpeed;
            Vector2 movement = new Vector2(_agent.desiredVelocity.x, _agent.desiredVelocity.z);

            _characterMovement.moveCharacter(movement, isRun: true, autoRotationOnRun: false, onlyAnimation: true); // avvia solo animazione
        }
        
    }

    /// stoppa agente e animazione dell'agente che dipende dal move character
    public void stopAgent() {
        if (_agent.enabled) {
            _agent.isStopped = true;
        }
        _characterMovement.stopCharacter();
    }

#if UNITY_EDITOR
    void OnDrawGizmos() {



        if(simulateSearchingPlayerSubBehaviourProcess != null) {

            for(int i = 0; i < simulateSearchingPlayerSubBehaviourProcess.randomNavMeshPositions.Count; i++) {
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(simulateSearchingPlayerSubBehaviourProcess.randomNavMeshPositions[i], 0.4f);

                Handles.color = Color.black;
                Handles.Label(
                        simulateSearchingPlayerSubBehaviourProcess.randomNavMeshPositions[i],
                        i.ToString()
                    );
            }
            
        }

    }
#endif
}
