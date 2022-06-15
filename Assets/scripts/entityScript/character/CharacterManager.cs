using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.AI;
public class CharacterManager : MonoBehaviour {
    private const int INTERACTABLE_LAYER = 3;


    private Dictionary<int, Interactable> interactableObjects = new Dictionary<int, Interactable>(); // dizionario Interactable ottenuti dagli onTrigger degli 

    [Header("References")]
    [SerializeField] private CharacterManager _aimedCharacter;
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
    [SerializeField] private TimedInteractionSliderManager timedInteractionSliderManager; // manager slider ui dei timer interaction
    [SerializeField] private InteractionUIController _interactionUIController; // controller per interagire con l'UI delle interazioni
    [SerializeField] private WeaponUIController _weaponUIController; // ref controller per visualizzare l'UI delle armi
    [SerializeField] public AlarmAlertUIController alarmAlertUIController; // ref controller per visualizzare stati di allerta UI
    [SerializeField] private InventoryManager _inventoryManager; // manager dell'intentario del character
    [SerializeField] private Transform _occlusionTargetTransform; // occlusion target che permette di capire quando il character � occluso tra la camera � un oggetto
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
    
    [SerializeField] private bool _isPlayer = false; // tiene conto se il character � attualmente controllato dal giocatore
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
    [SerializeField] private bool _isTarget = false; // indica se � un obiettivo del gioco(e quindi va ucciso)
    public bool isTarget {
        get { return _isTarget; }
    }



    [Header("Character Settings")]
    [SerializeField] private int characterHealth = 100;
    [SerializeField] private int FOVUnmalusFlashlightTimer = 4; // tempo necessario al character per ripristinare FOV tramite la torcia 
    [Range(0, 360)]
    [SerializeField] private float _firstMalusFovAngle = 60;
    
    [Range(0, 360)]
    [SerializeField] private float _secondMalusFovAngle = 90;
    [SerializeField] private float dividerFOVMalusValue = 2; // valore divisore fov malus 
    [SerializeField] private float dividerFOVMalusFlashlightValue = 1.3f; // valore divisore fov malus


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
            if(value == null) { // null quando no si sta mirando un character

                if(_aimedCharacter != null) { // si stava gi� mirando un character
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
    /// con cui il Character � in contatto.
    /// 
    /// Se il character � player viene ribuildata anche l'UI "buildUIinteractionList"
    /// </summary>
    public void buildListOfInteraction() {
        List<Interaction> interactions = new List<Interaction>(); // lista di tutte le Interaction disponibili per il player
    


        // ottieni dal dizionario degli oggetti interabili tutte le interactions
        foreach (var item in interactableObjects) {

            List<Interaction> interactable = item.Value.getInteractions();
            
            for(int i = 0; i < interactable.Count; i++) {

                interactions.Add(interactable[i]);
            }
        }


        // se il character � giocato dal player
        if(isPlayer) {

            // inizializza lista di interazioni e i bottoni e la partendo dalla lista interactions
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

        if(!isDead) {
            characterHealth -= damage;

            if (characterHealth <= 0) {
                _isDead = true;
                killCharacterAsync(damageVelocity);
            }
        }
    }

    /// <summary>
    /// Applica malus sul FOV del character riducendone la visibilit�
    /// </summary>
    public async void applyFOVMalus() {


        if(!isDead) {
            _characterFOV.setFOVValues(
            firstFovRadius: _characterFOV.usedFirstFovRadius / dividerFOVMalusValue,
            firstFovAngle: _firstMalusFovAngle,

            secondFovRadius: _characterFOV.usedSecondFovRadius / dividerFOVMalusValue,
            secondFovAngle: _secondMalusFovAngle
        );


            // se il character ha una torcia
            if (_inventoryManager.isFlashlightTaken) {
                /// Permette di accendere le torce dopo un tempo t
                /// ripristinando il fov del character
                /// Da usare per le guardie pi� specializzate
                float endTime = FOVUnmalusFlashlightTimer + Time.time;
                while (Time.time < endTime) {
                    await Task.Yield();
                }




                
                // flashlight fov
                await _inventoryManager.characterFlashLight.lightOnFlashLight();
                _characterFOV.setFOVValues(
                    firstFovRadius: _characterFOV.defaultFirstFovRadius / dividerFOVMalusFlashlightValue,
                    firstFovAngle: _firstMalusFovAngle,

                    secondFovRadius: _characterFOV.defaultSecondFovRadius / dividerFOVMalusFlashlightValue,
                    secondFovAngle: _secondMalusFovAngle
                );
            }
        }
        
    }

    /// <summary>
    /// Ripristina valori default del FOV
    /// </summary>
    public async Task<bool> restoreFOVMalus() {
        _characterFOV.setFOVValuesToDefault();
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
        resetCharacterMovmentState();

        // disabilita componenti
        gameObject.GetComponent<CharacterMovement>().enabled = false;
        gameObject.GetComponent<CharacterManager>().enabled = false;
        _inventoryManager.enabled = false;
        gameObject.GetComponent<CharacterController>().enabled = false;
        gameObject.GetComponent<CapsuleCollider>().enabled = false;
        gameObject.GetComponent<NavMeshObstacle>().enabled = false;


        
        // stoppa componenti
        gameObject.GetComponent<CharacterFOV>().stopAllCoroutines();
        gameObject.GetComponent<CharacterFOV>().enabled = false;

        _inventoryManager.setInventoryAsInteractable();


        _characterAnimator.StopPlayback();
        _characterAnimator.enabled = false;
        gameObject.GetComponent<RagdollManager>().enableRagdoll();


        if (!isPlayer) {

            inventoryManager.characterFlashLight.instantLightOffFlashLight();

            Role role = gameObject.GetComponent<CharacterRole>().role;
            

            if (role == Role.EnemyGuard) {

                //Destroy(gameObject.GetComponent<EnemyNPCBehaviour>());
                await gameObject.GetComponent<EnemyNPCBehaviour>().forceStopCharacterAndAwaitStopProcess();
                gameObject.GetComponent<EnemyNPCBehaviour>().enabled = false;
                gameObject.GetComponent<EnemyNPCBehaviour>().stopAllCoroutines();
                gameObject.GetComponent<EnemyNPCBehaviour>().stopAgent();
                gameObject.GetComponent<NavMeshAgent>().enabled = false;

                gameObject.GetComponent<EnemyNPCBehaviour>().stopSuspiciousTimer();
                gameObject.GetComponent<EnemyNPCBehaviour>().stopHostilityCheckTimer();
                gameObject.GetComponent<EnemyNPCBehaviour>().stopAlertAnimator();
                
            } else if (role == Role.Civilian) {

                //Destroy(gameObject.GetComponent<CivilianNPCBehaviour>());
                await gameObject.GetComponent<CivilianNPCBehaviour>().forceStopCharacterAndAwaitStopProcess();
                gameObject.GetComponent<CivilianNPCBehaviour>().enabled = false;
                gameObject.GetComponent<CivilianNPCBehaviour>().stopAllCoroutines();
                gameObject.GetComponent<CivilianNPCBehaviour>().stopAgent();
                gameObject.GetComponent<NavMeshAgent>().enabled = false;

                gameObject.GetComponent<CivilianNPCBehaviour>().stopSuspiciousTimer();
                gameObject.GetComponent<CivilianNPCBehaviour>().stopHostilityCheckTimer();
                gameObject.GetComponent<CivilianNPCBehaviour>().stopAlertAnimator();
                
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
            interactable.Value.unFocusInteractable();
        }
        interactableObjects = new Dictionary<int, Interactable>();
        buildListOfInteraction();
    }

    public void resetCharacterMovmentState() {
        isRunning = false;
        isBusy = false;
    }


    private void OnTriggerEnter(Collider collision) {

        if (collision.gameObject.layer == INTERACTABLE_LAYER) {


            InteractableObject interactableObject = collision.gameObject.GetComponent<InteractableObject>();



            // aggiungi interactable al dizionario dell'interactable solo se non � mai stata inserita
            // evita che collisioni multiple aggiungano la stessa key al dizionario
            if (!interactableObjects.ContainsKey(interactableObject.GetInstanceID())) {
                interactableObjects.Add(interactableObject.GetInstanceID(), interactableObject.interactable);
            }


            // rebuild lista interactions
            buildListOfInteraction();
        }
    }


    private void OnTriggerExit(Collider collision) {


        if (collision.gameObject.layer == INTERACTABLE_LAYER) {

            InteractableObject interactableObject = collision.gameObject.GetComponent<InteractableObject>();


            if (isPlayer) {
                interactableObject.interactable.unFocusInteractable(); // disattiva effetto focus sull'oggetto interagibile
            }


            // rimuovi interazione al dizionario delle interazioni
            interactableObjects.Remove(interactableObject.GetInstanceID());

            // rebuild lista interactions
            buildListOfInteraction();
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
            alarmAlertUIController.potentialProhibitedAreaAlarmOn();
        } else {
            alarmAlertUIController.potentialProhibitedAreaAlarmOff();
        }
    }

    public void discardCharacterAction() {
        _isBusy = false;
    }
}