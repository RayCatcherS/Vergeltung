using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
public class CharacterManager : MonoBehaviour {
    private const int INTERACTABLE_LAYER = 3;


    private Dictionary<int, Interactable> interactableObjects = new Dictionary<int, Interactable>(); // dizionario Interactable ottenuti dagli onTrigger degli 

    [Header("References")]
    private CharacterManager _aimedCharacter;
    [SerializeField] private Animator _characterAnimator;
    [SerializeField] private Outline _characterOutline; // outline character
    public Outline characterOutline {
        get { return _characterOutline; }
    }
    [SerializeField] private CharacterFOV _characterFOV; // componente fov del character
    public CharacterFOV characterFOV {
        get { return _characterFOV; }
    }
    [SerializeField] private CharacterMovement characterMovement;
    [SerializeField] private BaseNPCBehaviourManager _baseNPCBehaviourManager;
    public BaseNPCBehaviourManager baseNPCBehaviourManager {
        get { return _baseNPCBehaviourManager; }
    }
    [SerializeField] private TimedInteractionSliderManager timedInteractionSliderManager; // manager slider ui dei timer interaction
    [SerializeField] private InteractionUIController _interactionUIController; // controller per interagire con l'UI delle interazioni
    [SerializeField] private WeaponUIController _weaponUIController; // ref controller per visualizzare l'UI delle armi
    [SerializeField] public AlarmAlertUIController alarmAlertUIController; // ref controller per visualizzare stati di allerta UI
    [SerializeField] private InventoryManager _inventoryManager; // manager dell'intentario del character
    [SerializeField] private Transform _occlusionTargetTransform; // occlusion target che permette di capire quando il character è occluso tra la camera è un oggetto
    [SerializeField] private GameState _globalGameState; // game state di gioco, utilizzare per accedere a metodi globali che hanno ripercussioni sul gioco
    public GameState globalGameState {
        get { return _globalGameState; }
    }
    [SerializeField] private SceneEntitiesController _sceneEntitiesController; // scene entities controller 
    public SceneEntitiesController sceneEntitiesController {
        get { return _sceneEntitiesController; }
    }

    [SerializeField] private PlayerWarpController _playerWarpController;
    public PlayerWarpController playerWarpController {
        get { return _playerWarpController; }
    }
    [SerializeField] private GameObject characterDecalProjectorEffect;

    // stati del player
    [Header("Character States")]
    [SerializeField] private bool _isRunning = false;
    public bool isRunning {
        get { return _isRunning; }
        set { _isRunning = value; }
    }
    [SerializeField] private bool _isBusy = false; // con questo stato il character è impegnato e non può muoversi
    public bool isBusy {
        get { return _isBusy; }
        set { _isBusy = value; }
    }

    [SerializeField] private bool _isPlayer = false; // tiene conto se il character è attualmente controllato dal giocatore
    public bool isPlayer {
        get { return _isPlayer; }
        set { _isPlayer = value; }
    }
    [SerializeField] private bool _isDead = false;
    public bool isDead {
        get { return _isDead; }
    }
    [SerializeField] private bool _isPickLocking = false; // stato che rappresenta se il character sta scassinando
    public bool isPickLocking {
        get { return _isPickLocking; }
        set { _isPickLocking = value; }
    }
    [SerializeField] private bool _isTarget = false; // indica se è un obiettivo del gioco(e quindi va ucciso)
    public bool isTarget {
        get { return _isTarget; }
    }

    // indica se qualcuno si è allarmato trovando il cadavere
    // evita che più persone contemporaneamente si avvicinino al cadavere
    private bool _isBusyDeadAlarmCheck = false;
    public bool isBusyDeadAlarmCheck {
        get { return _isBusyDeadAlarmCheck; }
        set { _isBusyDeadAlarmCheck = value; }
    }

    // indica se un character morto è stato marcato 
    // un character marcato non può provocare altri stati di allerta nei FOV
    private bool _isDeadCharacterMarked = false;
    public bool isDeadCharacterMarked {
        get { return _isDeadCharacterMarked; }
        set { _isDeadCharacterMarked = value; }
    }

    Vector3 _deadPosition = new Vector3();



    [Header("Character Settings")]
    [SerializeField] private int characterHealth = 100;
    [SerializeField] private int FOVUnmalusFlashlightTimer = 4; // tempo necessario al character per ripristinare FOV tramite la torcia


    public void Start() {

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

            return inventoryManager.weaponItems[inventoryManager.selectedWeapon].isWeaponFiring;
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

                if (_aimedCharacter != null) { // si stava già mirando un character
                    _aimedCharacter._characterOutline.setEnableOutline(false); // disattiva outline del character precedentemente mirato
                    _aimedCharacter = value;
                }
            } else {

                if (_aimedCharacter != null) { // si stava già mirando un character
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
    public static GameObject initCharacterManagerComponent(GameObject gameObject, InteractionUIController controller, GameState gameState, PlayerWarpController playerWarpController, SceneEntitiesController sceneEntitiesController) {

        CharacterManager characterInteraction = gameObject.GetComponent<CharacterManager>(); // aggiungi componente CharacterInteraction 
        characterInteraction._interactionUIController = controller; // assegna al interactionUIController al componente CharacterInteraction
        characterInteraction._globalGameState = gameState;
        characterInteraction._playerWarpController = playerWarpController;
        characterInteraction._sceneEntitiesController = sceneEntitiesController;

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
    /// con cui il Character è in contatto.
    /// 
    /// Se il character è player viene ribuildata anche l'UI "buildUIinteractionList"
    /// </summary>
    public void buildListOfInteraction() {
        List<Interaction> interactions = new List<Interaction>(); // lista di tutte le Interaction disponibili per il player



        // ottieni dal dizionario degli oggetti interabili tutte le interactions
        foreach (var item in interactableObjects) {

            List<Interaction> interactable = item.Value.getInteractions();

            for (int i = 0; i < interactable.Count; i++) {

                interactions.Add(interactable[i]);
            }
        }


        // se il character è giocato dal player
        if (isPlayer) {

            // inizializza lista di interazioni e i bottoni e la partendo dalla lista interactions
            // passa la lista di interactions per inizializzare la lista di interacion che potranno essere effettuate
            _interactionUIController.buildUIinteractionList(interactions, this);
        }

    }

    /// <summary>
    /// Rimuove un interactableObject dell'oggetto interactable venuto a contatto con il player
    /// Da usare quando si raccoglie un InventoryItem generico,
    /// Serve ad aggiornare gli interactableObjects con cui il character può interagire
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

        if (!isDead) {
            characterHealth -= damage;

            if (characterHealth <= 0) {
                _isDead = true;
                killCharacterAsync(damageVelocity);
            } else {

                // se è un NPC avvia il behaviour check sull'aver ricevuto del danno
                if (!isPlayer) {
                    _baseNPCBehaviourManager.instantOnCurrentPositionWarnOfSouspiciousCheck();
                }
            }
        }
    }

    /// <summary>
    /// Applica malus sul FOV del character riducendone la visibilità
    /// </summary>
    public async void applyFOVMalus() {

        if (!isDead) {
            _characterFOV.setNightMalus(true);
        }


        // se il character ha una torcia
        if (_inventoryManager.isFlashlightTaken) {
            /// Permette di accendere le torce dopo un tempo t
            /// ripristinando il fov del character
            /// Da usare per le guardie più specializzate
            float endTime = FOVUnmalusFlashlightTimer + Time.time;
            while (Time.time < endTime) {
                await Task.Yield();
            }





            // flashlight fov

            if (!isDead) { // ricontrolla se il character è morto, potrebbe essere morto dopo il ciclo sopra
                await _inventoryManager.characterFlashLight.lightOnFlashLight();
                _characterFOV.setFlashLightBonus(true);
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

        Debug.Log("Character dead at: " + gameObject.transform.position);
        resetCharacterStates();


        characterDecalProjectorEffect.SetActive(false); // disabilita decal projector

        // disabilita componenti
        gameObject.GetComponent<CharacterMovement>().enabled = false;
        gameObject.GetComponent<CharacterManager>().enabled = false;
        _inventoryManager.enabled = false;
        gameObject.GetComponent<CharacterController>().enabled = false;
        


        // setta il character manager come figlio dell'hips della ragdoll del character
        // questo fa in modo che tutto il character manager e collider si muova insieme alla ragdoll
        Transform characterParent = gameObject.transform.parent;
        gameObject.GetComponent<RagdollManager>().ragdollHips.gameObject.transform.SetParent(characterParent);
        gameObject.transform.SetParent(gameObject.GetComponent<RagdollManager>().ragdollHips.gameObject.transform);



        gameObject.GetComponent<CapsuleCollider>().isTrigger = true; // non è possibile avere collisioni fisiche ma il character resta
        gameObject.GetComponent<NavMeshObstacle>().enabled = false;


        
        // stoppa componenti
        gameObject.GetComponent<CharacterFOV>().enabled = false;
        _characterAnimator.StopPlayback();
        _characterAnimator.enabled = false;
        gameObject.GetComponent<RagdollManager>().enableRagdoll();

        // setta inventario come oggetto con cui poter interagire
        _inventoryManager.setInventoryAsInteractable();


        if (!isPlayer) {

            inventoryManager.characterFlashLight.instantLightOffFlashLight();

            Role role = gameObject.GetComponent<CharacterRole>().role;
            

            if (role == Role.EnemyGuard) {

                gameObject.GetComponent<EnemyNPCBehaviourManager>().stopSuspiciousTimer();
                gameObject.GetComponent<EnemyNPCBehaviourManager>().stopHostilityCheckTimer();
                gameObject.GetComponent<EnemyNPCBehaviourManager>().stopAlertAnimator();

                //Destroy(gameObject.GetComponent<EnemyNPCBehaviour>());
                await gameObject.GetComponent<EnemyNPCBehaviourManager>().forceStopCharacterAndAwaitStopProcess();
                gameObject.GetComponent<EnemyNPCBehaviourManager>().enabled = false;
                gameObject.GetComponent<EnemyNPCBehaviourManager>().stopAllCoroutines();
                gameObject.GetComponent<EnemyNPCBehaviourManager>().stopAgent();
                gameObject.GetComponent<NavMeshAgent>().enabled = false;

                
                
            } else if (role == Role.Civilian) {

                gameObject.GetComponent<CivilianNPCBehaviourManager>().stopSuspiciousTimer();
                gameObject.GetComponent<CivilianNPCBehaviourManager>().stopHostilityCheckTimer();
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
            // reset character interactable objects
            emptyAllInteractableDictionaryObjects();

            _playerWarpController.unstackDeadCharacterAndControlPreviewCharacter(this);
        }

        
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
        isPickLocking = false;
    }


    private void OnTriggerEnter(Collider collision) {


        if(isPlayer) {
            if (collision.gameObject.layer == INTERACTABLE_LAYER) {


                InteractableObject interactableObject = collision.gameObject.GetComponent<InteractableObject>();



                // aggiungi interactable al dizionario dell'interactable solo se non è mai stata inserita
                // evita che collisioni multiple aggiungano la stessa key al dizionario
                if (!interactableObjects.ContainsKey(interactableObject.GetInstanceID())) {
                    interactableObjects.Add(interactableObject.GetInstanceID(), interactableObject.interactable);
                }


                // rebuild lista interactions
                buildListOfInteraction();
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
    /// ritorna [true] se il task è stato completato correttamente
    /// altrimenti [false]
    /// </summary>
    /// <param name="timeToWait">Tempo durata del task (Sospensione controllo)</param>
    /// <param name="interactionTitle">Titolo dell'interazione da avviare</param>
    /// <param name="triggerToInterrupt">Istanza tipo booleano che contiene l'informazione booleana di stop dell'interazione</param>
    /// <param name="valueTointerrupt">Se il triggerToInterrupt è uguale al valueTointerrupt l'interaction si interrompe</param>
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
            alarmAlertUIController.potentialProhibitedAreaAlarmOn();
        } else {
            alarmAlertUIController.potentialProhibitedAreaAlarmOff();
        }
    }
    public void discardCharacterAction() {
        _isBusy = false;
    }

    /// <summary>
    /// Partendo dal character restituisce la posizione più raggiungibile per un agent(sulla navmesh)
    /// </summary>
    /// <returns></returns>
    public Vector3 getCharacterPositionReachebleByAgents() {
        Vector3 pos = new Vector3();
        NavMeshHit hit;


        if (isDead) {
            
            if (NavMesh.SamplePosition(gameObject.GetComponent<RagdollManager>().ragdollHips.gameObject.transform.position, out hit, 5.0f, NavMesh.AllAreas)) {
                _deadPosition = hit.position;

                pos = _deadPosition;
            } else {

                // nel caso in cui fallisse restituisce la posizione del character
                pos = gameObject.transform.position;
            }

            
        } else {

            if (NavMesh.SamplePosition(gameObject.GetComponent<RagdollManager>().ragdollHips.gameObject.transform.position, out hit, 5.0f, NavMesh.AllAreas)) {

                pos = hit.position;
            } else {

                // nel caso in cui fallisse restituisce la posizione del character
                pos = gameObject.transform.position;
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
    }
#endif
}