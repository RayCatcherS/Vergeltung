using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
public class CharacterManager : MonoBehaviour {
    private const int INTERACTABLE_LAYER = 3;


    private Dictionary<int, Interactable> interactableObjects = new Dictionary<int, Interactable>(); // dizionario Interactable ottenuti dagli onTrigger degli 

    
    private CharacterManager _aimedCharacter;
    [Header("References")]
    [SerializeField] private Animator _characterAnimator;
    [SerializeField] private Outline _characterOutline; // outline character
    public Outline characterOutline {
        get { return _characterOutline; }
    }
    [SerializeField] private CharacterFOV _characterFOV; // componente fov del character
    public CharacterFOV characterFOV {
        get { return _characterFOV; }
    }
    [SerializeField] private CharacterMovement _characterMovement;
    public CharacterMovement characterMovement {
        get { return _characterMovement; }
    }
    [SerializeField] private BaseNPCBehaviourManager _baseNPCBehaviourManager;
    public BaseNPCBehaviourManager baseNPCBehaviourManager {
        get { return _baseNPCBehaviourManager; }
    }
    [SerializeField] private TimedInteractionSliderManager timedInteractionSliderManager; // manager slider ui dei timer interaction
    private InteractionUIController _interactionUIController; // controller per interagire con l'UI delle interazioni
    private WeaponUIController _weaponUIController; // ref controller per visualizzare l'UI delle armi
    private AlarmAlertUIController _alarmAlertUIController; // ref controller per visualizzare stati di allerta UI
    public AlarmAlertUIController alarmAlertUIController {
        get { return _alarmAlertUIController; }
        set { _alarmAlertUIController = value; }
    }
    private HealthBarUIController _healthBarController;
    public HealthBarUIController healthBarController {
        get { return _healthBarController; }
        set { _healthBarController = value; }
    }
    private DamageScreenEffect _damageAnimation;
    public DamageScreenEffect damageAnimation {
        get { return _damageAnimation; }
        set { _damageAnimation = value; }
    }
    [SerializeField] private InventoryManager _inventoryManager; // manager dell'intentario del character
    [SerializeField] private Transform _occlusionTargetTransform; // occlusion target che permette di capire quando il character � occluso tra la camera � un oggetto
    private GameState _globalGameState; // game state di gioco, utilizzare per accedere a metodi globali che hanno ripercussioni sul gioco
    public GameState globalGameState {
        get { return _globalGameState; }
    }
    private SceneEntitiesController _sceneEntitiesController; // scene entities controller 
    public SceneEntitiesController sceneEntitiesController {
        get { return _sceneEntitiesController; }
    }

    private PlayerWarpController _playerWarpController;
    public PlayerWarpController playerWarpController {
        get { return _playerWarpController; }
    }
    [SerializeField] private GameObject characterDecalProjectorEffect;
    [SerializeField] private Rigidbody rigidBodyBoneForce; // bone su cui applicare la forza una volta che il character muore


    // stati del player
    [Header("Character States")]
    [SerializeField] private bool _isRunning = false;
    public bool isRunning {
        get { return _isRunning; }
        set { _isRunning = value; }
    }
    [SerializeField] private bool _isBusy = false; // con questo stato il character � impegnato e non pu� muoversi
    public bool isBusy {
        get { return _isBusy; }
        set { _isBusy = value; }
    }

    [SerializeField] private bool _isInteractionsDisabled = false; // con questo stato il character � impegnato e non pu� muoversi
    public bool isInteractionsDisabled {
        get { return _isInteractionsDisabled; }
    }

    [SerializeField] private bool _isPlayer = false; // tiene conto se il character � attualmente controllato dal giocatore
    public bool isPlayer {
        get { return _isPlayer; }
        set { _isPlayer = value; }
    }
    [SerializeField] private bool _isStackControlled = false;
    public bool isStackControlled {
        get { return _isStackControlled; }
        set { _isStackControlled = value; }
    }
    [SerializeField] private bool _isDead = false;
    public bool isDead {
        get { return _isDead; }
    }
    [SerializeField] private bool _isSuspiciousGenericAction = false; // stato che rappresenta se il character sta compiendo una generica azione sospetta
    public bool isSuspiciousGenericAction {
        get { return _isSuspiciousGenericAction; }
        set { _isSuspiciousGenericAction = value; }
    }
    [SerializeField] private bool _isTarget = false; // indica se � un obiettivo del gioco(e quindi va ucciso)
    public bool isTarget {
        get { return _isTarget; }
        set { _isTarget = value; }
    }

    // indica se qualcuno si � allarmato trovando il cadavere
    // evita che pi� persone contemporaneamente si avvicinino al cadavere
    private bool _isBusyDeadAlarmCheck = false;
    public bool isBusyDeadAlarmCheck {
        get { return _isBusyDeadAlarmCheck; }
        set { _isBusyDeadAlarmCheck = value; }
    }

    // indica se un character morto � stato marcato 
    // un character marcato non pu� provocare altri stati di allerta nei FOV
    private bool _isDeadCharacterMarked = false;
    public bool isDeadCharacterMarked {
        get { return _isDeadCharacterMarked; }
        set { _isDeadCharacterMarked = value; }
    }

    public Role chracterRole {
        get { return gameObject.GetComponent<CharacterRole>().role; }
    }



    [Header("Character Settings")]
    [SerializeField] private const int characterMaxHealth = 100;
    [SerializeField] private int characterHealth;
    [SerializeField] private int FOVUnmalusFlashlightTimer = 4; // tempo necessario al character per ripristinare FOV tramite la torcia


    public void Start() {
        characterHealth = characterMaxHealth;
    }


    //getter - setter
    public InteractionUIController interactionUIController {
        get { return _interactionUIController; }
        set {
            _interactionUIController = value;
        }
    }
    public WeaponUIController weaponUIController {
        get { return _weaponUIController; }
        set {
            _weaponUIController = value;
        }
    }
    public InventoryManager inventoryManager {
        get { return _inventoryManager; }
    }
    public bool isWeaponCharacterFiring {
        get {
            WeaponType usedWeaponType = inventoryManager.weaponItems[inventoryManager.selectedWeapon].getWeaponType;

            if(usedWeaponType != WeaponType.controlWeapon) {

                return inventoryManager.weaponItems[inventoryManager.selectedWeapon].isWeaponFiring;
            } else {
                return false;
            }
             
        }
    }
    public Animator characterAnimator {
        get { return _characterAnimator; }
    }
    public Transform occlusionTargetTransform {
        get { return _occlusionTargetTransform; }
    }
    public CharacterManager aimedCharacter {
        get { return _aimedCharacter; }
        set {
            if (value == null) { // null quando no si sta mirando un character

                if (_aimedCharacter != null) { // si stava gi� mirando un character
                    _aimedCharacter._characterOutline.setEnableOutline(false); // disattiva outline del character precedentemente mirato
                    _aimedCharacter = value;
                }
            } else {

                if (_aimedCharacter != null) { // si stava gi� mirando un character
                    _aimedCharacter._characterOutline.setEnableOutline(false);
                    _aimedCharacter = value;
                    _aimedCharacter._characterOutline.setEnableOutline(true);
                } else {
                    _aimedCharacter = value;
                    _aimedCharacter._characterOutline.setEnableOutline(true);
                }
            }


        }
    }

    /// <summary>
    /// Aggiunge e inizializza un CharacterManager component al gameObject
    /// Infine restituisce il gameObject
    /// </summary>
    /// <param name="gameObject">gameObject a cui aggiungere il componente CharacterManager</param>
    /// <returns></returns>
    public static GameObject initCharacterManagerComponent(
        GameObject gameObject,
        InteractionUIController controller,
        GameState gameState,
        PlayerWarpController playerWarpController,
        SceneEntitiesController sceneEntitiesController,
        bool isTarget
    ) {

        CharacterManager character = gameObject.GetComponent<CharacterManager>(); // aggiungi componente CharacterInteraction 
        character._interactionUIController = controller; // assegna al interactionUIController al componente CharacterInteraction
        character._globalGameState = gameState;
        character._playerWarpController = playerWarpController;
        character._sceneEntitiesController = sceneEntitiesController;

        character.isTarget = isTarget;
        if(isTarget) {

            character.gameObject.GetComponent<TargetIconManager>().enableTargetUI();
            gameObject.GetComponent<IconMapManager>().changeIcon(IconMapManager.CharacterIcon.targetCharacter);
        } else {
            character.gameObject.GetComponent<TargetIconManager>().disableTargetUI();

            if(gameObject.GetComponent<CharacterRole>().role == Role.Civilian) {
                gameObject.GetComponent<IconMapManager>().changeIcon(IconMapManager.CharacterIcon.civilian);

            } else if(gameObject.GetComponent<CharacterRole>().role == Role.EnemyGuard) {
                gameObject.GetComponent<IconMapManager>().changeIcon(IconMapManager.CharacterIcon.enemy);

            } else if(gameObject.GetComponent<CharacterRole>().role == Role.Player) {
                gameObject.GetComponent<IconMapManager>().changeIcon(IconMapManager.CharacterIcon.player);

            }
        }

        return gameObject;
    }

    /// <summary>
    /// assegna il controller InteractionUIController
    /// da usare per avviare operazioni sulla UI
    /// </summary>
    /// <param name="controller"></param>
    public void setInteractionUIController(InteractionUIController controller) {
        _interactionUIController = controller;
    }




    /// <summary>
    /// Builda lista di interazioni ottenuta da tutti gli Interactable
    /// con cui il Character � in contatto.
    /// 
    /// Se il character � player viene ribuildata anche l'UI "buildUIinteractionList"
    /// </summary>
    public void buildListOfInteraction() {
        List<Interaction> interactions = new List<Interaction>(); // lista di tutte le Interaction disponibili per il player



        // ottieni dal dizionario degli oggetti interabili tutte le interactions
        foreach (var item in interactableObjects) {

            List<Interaction> interactable = item.Value.getInteractions(this);

            for (int i = 0; i < interactable.Count; i++) {

                interactions.Add(interactable[i]);
            }
        }


        // se il character � giocato dal player
        if (isPlayer) {

            // inizializza lista di interazioni e i bottoni partendo dalla lista interactions
            // passa la lista di interactions per inizializzare la lista di interacion che potranno essere effettuate
            _interactionUIController.buildUIinteractionList(interactions, this);
        }

    }

    /// <summary>
    /// Rimuove un interactableObject dell'oggetto interactable venuto a contatto con il player
    /// Da usare quando si raccoglie un InventoryItem generico,
    /// Serve ad aggiornare gli interactableObjects con cui il character pu� interagire
    /// </summary>
    /// <param name="interactableOBJ"></param>
    public void removeCharacterInteractableObject(InteractableObject interactableOBJ) {

        interactableObjects.Remove(interactableOBJ.GetInstanceID());
        buildListOfInteraction();
    }

    /// <summary>
    /// Applica danno al character
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="damageVelocity"></param>
    public void applyCharacterDamage(int damage, Vector3 damageVelocity) {

        float _damage = damage;

        if(_isStackControlled) {
            _damage = damage / 3;
            
        }
        
        characterHealth -= (int)_damage;

        if(_isPlayer) {
            buildUIHealthBar();
            playDamageEffect();
        }

        if (characterHealth <= 0) {
            _isDead = true;

            killCharacterAsync(damageVelocity);
        }
    }

    /// <summary>
    /// Applica malus sul FOV del character riducendone la visibilit�
    /// </summary>
    public async void applyFOVMalus() {

        if (!isDead) {
            _characterFOV.setNightMalus(true);
        }


        // se il character ha una torcia
        if(!_isStackControlled) {
            if(_inventoryManager.isFlashlightTaken) {
                /// Permette di accendere le torce dopo un tempo t
                /// ripristinando il fov del character
                /// Da usare per le guardie pi� specializzate
                float endTime = FOVUnmalusFlashlightTimer + Time.time;
                while(Time.time < endTime) {
                    await Task.Yield();
                }


                // flashlight fov
                if(!isDead) { // ricontrolla se il character � morto, potrebbe essere morto dopo il ciclo sopra
                    await _inventoryManager.characterFlashLight.lightOnFlashLight();
                    _characterFOV.setFlashLightBonus(true);
                }
            }
        }
        
    }



    /// <summary>
    /// Ripristina valori default del FOV
    /// </summary>
    public async Task<bool> restoreFOVMalus() {
        _characterFOV.setNightMalus(false);
        _characterFOV.setFlashLightBonus(false);
        await _inventoryManager.characterFlashLight.lightOffFlashLight();

        return true;
    }

    /// <summary>
    /// Porta il character nello stato Dead
    /// Disabilita componenti e abilita ragdoll
    /// </summary>
    /// <param name="damageVelocity"></param>
    public async Task killCharacterAsync(Vector3 damageVelocity) {

        // rimuovi character dal dizionario dei character in stato di allerta
        sceneEntitiesController.removeCharacterInstanceAndAlertStateToDictionary(this);

        resetCharacterStates();


        characterDecalProjectorEffect.SetActive(false); // disabilita decal projector

        // disabilita componenti
        gameObject.GetComponent<CharacterMovement>().enabled = false;
        _inventoryManager.enabled = false;
        gameObject.GetComponent<CharacterController>().enabled = false;

        // target UI
        gameObject.GetComponent<TargetIconManager>().disableTargetUI();


        gameObject.GetComponent<CapsuleCollider>().isTrigger = true; // non � possibile avere collisioni fisiche ma il character resta
        gameObject.GetComponent<NavMeshObstacle>().enabled = false;


        
        // stoppa componenti
        gameObject.GetComponent<CharacterFOV>().enabled = false;
        _characterAnimator.StopPlayback();
        _characterAnimator.enabled = false;
        gameObject.GetComponent<RagdollManager>().enableRagdoll();

        // setta inventario come oggetto con cui poter interagire
        _inventoryManager.setInventoryAsInteractable();


        if (!_isStackControlled) {

            inventoryManager.characterFlashLight.instantLightOffFlashLight();

            Role role = gameObject.GetComponent<CharacterRole>().role;
            

            if (role == Role.EnemyGuard) {

                gameObject.GetComponent<EnemyNPCBehaviourManager>().stopSuspiciousTimer();
                gameObject.GetComponent<EnemyNPCBehaviourManager>().stopHostilityTimer();
                gameObject.GetComponent<EnemyNPCBehaviourManager>().stopAlertAnimator();

                //Destroy(gameObject.GetComponent<EnemyNPCBehaviour>());
                await gameObject.GetComponent<EnemyNPCBehaviourManager>().forceStopCharacterAndAwaitStopProcess();
                gameObject.GetComponent<EnemyNPCBehaviourManager>().enabled = false;
                gameObject.GetComponent<EnemyNPCBehaviourManager>().stopAllCoroutines();
                gameObject.GetComponent<EnemyNPCBehaviourManager>().stopAgent();
                gameObject.GetComponent<NavMeshAgent>().enabled = false;

                
                
            } else if (role == Role.Civilian) {

                gameObject.GetComponent<CivilianNPCBehaviourManager>().stopSuspiciousTimer();
                gameObject.GetComponent<CivilianNPCBehaviourManager>().stopHostilityTimer();
                gameObject.GetComponent<CivilianNPCBehaviourManager>().stopAlertAnimator();


                //Destroy(gameObject.GetComponent<CivilianNPCBehaviour>());
                await gameObject.GetComponent<CivilianNPCBehaviourManager>().forceStopCharacterAndAwaitStopProcess();
                gameObject.GetComponent<CivilianNPCBehaviourManager>().enabled = false;
                gameObject.GetComponent<CivilianNPCBehaviourManager>().stopAllCoroutines();
                gameObject.GetComponent<CivilianNPCBehaviourManager>().stopAgent();
                gameObject.GetComponent<NavMeshAgent>().enabled = false;

                
                
            }
        } else { // ucciso character del warp stack

            _inventoryManager.weaponLineRenderer.enabled = false;
            _isInteractionsDisabled = true;
            emptyAllInteractableDictionaryObjects();
            _playerWarpController.unstackDeadCharacterAndControlPreviewCharacter(this);
        }

        // setta il character manager come figlio dell'hips della ragdoll del character
        // questo fa in modo che tutto il character manager e collider si muova insieme alla ragdoll
        Transform characterParent = gameObject.transform.parent;
        gameObject.GetComponent<RagdollManager>().ragdollHips.gameObject.transform.SetParent(characterParent);
        gameObject.transform.SetParent(gameObject.GetComponent<RagdollManager>().ragdollHips.gameObject.transform);

        //Debug.Log("Character dead at: " + gameObject.transform.position);

        // aggiungi forza
        rigidBodyBoneForce.AddForce(damageVelocity, ForceMode.Impulse);


        // check se � un target => invia game goal event
        if(isTarget) {
            sendGameGoalEvent();
        }

        // disable map icon
        gameObject.GetComponent<IconMapManager>().disableIcon();
    }

    private void sendGameGoalEvent() {
        const string gameGoalName = "Kill the guardians";

        _sceneEntitiesController.gameObject.GetComponent<GameModeController>()
            .updateGameGoalsStatus(gameGoalName, GameGoal.GameGoalOperation.addGoal);
    }

    /// <summary>
    /// Disattiva gli outline di tutti gli interactable objects nel dizionario del character
    /// Resetta dizionario del character svuotandolo
    /// Rebuilda UI
    /// </summary>
    public void emptyAllInteractableDictionaryObjects() {
        // unfocus outline di tutti gli interactable
        foreach(var interactable in interactableObjects) {
            interactable.Value.unFocusInteractableOutline();
        }
        interactableObjects = new Dictionary<int, Interactable>();
        buildListOfInteraction();

    }


    public void resetCharacterStates() {
        isRunning = false;
        isBusy = false;
        isSuspiciousGenericAction = false;
    }

    /// <summary>
    /// Forza detection dei trigger, caso in cui non si vuole aspettare di entrare/uscire da una trigger collision
    /// </summary>
    public void forceTriggerDetection() {

        if(isPlayer) {
            CapsuleCollider characterCapsuleCollider = gameObject.GetComponent<CapsuleCollider>();
            foreach(Collider collision in Physics.OverlapSphere(characterCapsuleCollider.transform.position, characterCapsuleCollider.radius)) {

                if(!_isInteractionsDisabled) {
                    if(collision.gameObject.layer == INTERACTABLE_LAYER) {


                        InteractableObject interactableObject = collision.gameObject.GetComponent<InteractableObject>();



                        // aggiungi interactable al dizionario dell'interactable solo se non � mai stata inserita
                        // evita che collisioni multiple aggiungano la stessa key al dizionario
                        if(!interactableObjects.ContainsKey(interactableObject.GetInstanceID())) {
                            interactableObjects.Add(interactableObject.GetInstanceID(), interactableObject.interactable);
                        }


                        // rebuild lista interactions
                        buildListOfInteraction();
                    }
                }
            }
        }
        
    }

    private void OnTriggerEnter(Collider collision) {


        if(isPlayer) {

            if(!_isInteractionsDisabled) {
                if(collision.gameObject.layer == INTERACTABLE_LAYER) {


                    InteractableObject interactableObject = collision.gameObject.GetComponent<InteractableObject>();



                    // aggiungi interactable al dizionario dell'interactable solo se non � mai stata inserita
                    // evita che collisioni multiple aggiungano la stessa key al dizionario
                    if(!interactableObjects.ContainsKey(interactableObject.GetInstanceID())) {
                        interactableObjects.Add(interactableObject.GetInstanceID(), interactableObject.interactable);
                    }


                    // rebuild lista interactions
                    buildListOfInteraction();
                }
            }
            
        }
        
    }
    private void OnTriggerExit(Collider collision) {


        if (collision.gameObject.layer == INTERACTABLE_LAYER) {

            InteractableObject interactableObject = collision.gameObject.GetComponent<InteractableObject>();


            if (isPlayer) {
                interactableObject.interactable.unFocusInteractableOutline(); // disattiva effetto focus sull'oggetto interagibile


                // rimuovi interazione al dizionario delle interazioni
                interactableObjects.Remove(interactableObject.GetInstanceID());

                // rebuild lista interactions
                buildListOfInteraction();

            }
        }
    }



    /// <summary>
    /// Esecuzione task a tempo
    /// ritorna [true] se il task � stato completato correttamente
    /// altrimenti [false]
    /// </summary>
    /// <param name="timeToWait">Tempo durata del task (Sospensione controllo)</param>
    /// <param name="interactionTitle">Titolo dell'interazione da avviare</param>
    /// <param name="triggerToInterrupt">Istanza tipo booleano che contiene l'informazione booleana di stop dell'interazione</param>
    /// <param name="valueTointerrupt">Se il triggerToInterrupt � uguale al valueTointerrupt l'interaction si interrompe</param>
    /// <returns></returns>
    public async Task<bool> startTimedInteraction(float timeToWait, string interactionTitle, Boolean triggerToInterrupt = null, bool valueTointerrupt = false) {

        bool result = true;
        float startTime = Time.time;
        float endTime = Time.time + timeToWait;

        timedInteractionSliderManager.enableAndInitializeTimerSlider(minValue: 0, maxValue: endTime - startTime, sliderTitle: interactionTitle);

        _isBusy = true;
        while (Time.time < endTime) {

            timedInteractionSliderManager.setSliderValue(Time.time - startTime);

            if(!_isBusy) {
                result = false; // interaction fallita
                break;
            }

            if(triggerToInterrupt != null) {
                if(triggerToInterrupt.value == valueTointerrupt) {
                    result = false; // interaction interrotta
                    break;
                }
            }
            await Task.Yield();
        }
        _isBusy = false;
        timedInteractionSliderManager.disableTimeSlider();


        return result;
    }

    /// <summary>
    /// Attiva o meno l'icona dell'area proibita in base al check dell'area proibita
    /// </summary>
    public void rebuildUIProhibitedAreaIcon() {
        if (gameObject.GetComponent<CharacterAreaManager>().isCharacterInProhibitedAreaCheck()) {
            _alarmAlertUIController.potentialProhibitedAreaAlarmOn();
        } else {
            _alarmAlertUIController.potentialProhibitedAreaAlarmOff();
        }
    }
    
    public void buildUIHealthBar() {
        if(_healthBarController != null) {
            _healthBarController.setValue(characterHealth, characterMaxHealth);
        }
    }

    private void playDamageEffect() {
        if(damageAnimation != null) {
            damageAnimation.playDamageEffect();
        }
        
    }

    public void discardCharacterAction() {
        _isBusy = false;
    }

    /// <summary>
    /// Partendo dalla posizione del character restituisce la posizione raggiungibile per un agent(sulla navmesh)
    /// </summary>
    /// <returns></returns>
    public Vector3 getCharacterPositionReachebleByAgents() {
        Vector3 pos = new Vector3();
        NavMeshHit hit;


        if (isDead) {
            
            if (NavMesh.SamplePosition(gameObject.GetComponent<RagdollManager>().ragdollHips.gameObject.transform.position, out hit, 5.0f, NavMesh.AllAreas)) {
              
                pos = hit.position;
            }

            
        } else {

            if (NavMesh.SamplePosition(gameObject.GetComponent<RagdollManager>().ragdollHips.gameObject.transform.position, out hit, 5.0f, NavMesh.AllAreas)) {

                pos = hit.position;
            }
        }

        return pos;
    }

    static public Vector3 getPositionReachebleByAgents(CharacterManager character, Vector3 position) {
        Vector3 pos = new Vector3();
        NavMeshHit hit;


        if(character.isDead) {

            if(NavMesh.SamplePosition(position, out hit, 5.0f, NavMesh.AllAreas)) {

                pos = hit.position;
            }


        } else {

            if(NavMesh.SamplePosition(position, out hit, 5.0f, NavMesh.AllAreas)) {

                pos = hit.position;
            }
        }

        return pos;
    }

#if UNITY_EDITOR
    void OnDrawGizmos() {
        if (isDead) {
            Gizmos.color = Color.grey;
            Gizmos.DrawLine(transform.position, getCharacterPositionReachebleByAgents());
            Gizmos.DrawSphere(getCharacterPositionReachebleByAgents(), 0.25f);
        }




        // calcola distanza tra la camera e lo spawn point
        SceneView sceneView = SceneView.lastActiveSceneView;
        float scenViewCameraDistance = Vector3.Distance(sceneView.camera.transform.position, transform.position);

        //draw character info
        if(scenViewCameraDistance < 50f) {
            string debugInfo = "AreaID: " + areaID.ToString() + "\n" + "CharacterID: " + characterID.ToString();
            Handles.color = Color.red;
            Handles.Label(
                characterFOV.recognitionTarget.position,
                debugInfo
            );
        }
            
        
    }

    int areaID {
        get { return gameObject.GetComponent<CharacterAreaManager>().belongingAreaId; }
    }

    int characterID {
        get { return this.GetInstanceID(); }
    }
#endif
}