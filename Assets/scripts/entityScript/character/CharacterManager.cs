using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class CharacterManager : MonoBehaviour {
    private const int INTERACTABLE_LAYER = 3;

    
    private Dictionary<int, Interactable> interactableObjects = new Dictionary<int, Interactable>(); // dizionario Interactable ottenuti dagli onTrigger degli 

    [Header("References")]
    [SerializeField] private Outline _characterOutline; // outline character
    public Outline characterOutline {
        get { return _characterOutline; }
    }
    [SerializeField] private CharacterFOV characterFOV; // componente fov del character
    [SerializeField] private InteractionUIController _interactionUIController; // controller per interagire con l'UI delle interazioni
    [SerializeField] private WeaponUIController _weaponUIController; // ref controller per visualizzare l'UI delle armi
    [SerializeField] private InventoryManager _inventoryManager; // manager dell'intentario del character
    [SerializeField] private Transform _occlusionTargetTransform; // occlusion target che permette di capire quando il character è occluso tra la camera è un oggetto
    [SerializeField] private GameState _globalGameState; // game state di gioco, utilizzare per accedere a metodi globali che hanno ripercussioni sul gioco
    public GameState globalGameState {
        get { return _globalGameState; }
    }
    // stati del player
    [Header("Character States")]
    [SerializeField] public bool isRunning = false;
    [SerializeField] public bool isBusy = false;
    [SerializeField] public bool isPlayer = false; // tiene conto se il character è attualmente controllato dal giocatore
    [SerializeField] public bool isDead = false;
    [SerializeField] private CharacterManager _aimedCharacter;
    [SerializeField] private Animator _characterAnimator;

    [Header("Character Settings")]
    [SerializeField] private int characterHealth = 100;
    [SerializeField] private int FOVUnmalusFlashlightTimer = 4; // tempo necessario al character per ripristinare FOV tramite la torcia 
    [Range(0, 360)]
    [SerializeField] private float _firstMalusFovAngle = 60;
    
    [Range(0, 360)]
    [SerializeField] private float _secondMalusFovAngle = 90;
    [SerializeField] private int dividerFOVMalusValue = 2; // valore divisore fov malus 
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

                if(_aimedCharacter != null) { // si stava già mirando un character
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
    public static GameObject initCharacterManagerComponent(GameObject gameObject, InteractionUIController controller, GameState gameState) {
        
        CharacterManager characterInteraction = gameObject.GetComponent<CharacterManager>(); // aggiungi componente CharacterInteraction 
        characterInteraction._interactionUIController = controller; // assegna al interactionUIController al componente CharacterInteraction
        characterInteraction._globalGameState = gameState;
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

    private void OnTriggerEnter(Collider collision) {
        
        if (collision.gameObject.layer == INTERACTABLE_LAYER) {

            
            InteractableObject interactableObject = collision.gameObject.GetComponent<InteractableObject>();



            // aggiungi interactable al dizionario dell'interactable solo se non è mai stata inserita
            // evita che collisioni multiple aggiungano la stessa key al dizionario
            if(!interactableObjects.ContainsKey(interactableObject.GetInstanceID())) {
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
            
            for(int i = 0; i < interactable.Count; i++) {

                interactions.Add(interactable[i]);
            }
        }


        // se il character è giocato dal player
        if(isPlayer) {

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

        if(!isDead) {
            characterHealth -= damage;

            if (characterHealth <= 0) {
                isDead = true;
                killCharacter(damageVelocity);
            }
        }
    }

    /// <summary>
    /// Applica malus sul FOV del character riducendone la visibilità
    /// </summary>
    public async void applyFOVMalus() {


        if(!isDead) {
            characterFOV.setFOVValues(
            firstFovRadius: characterFOV.usedFirstFovRadius / dividerFOVMalusValue,
            firstFovAngle: _firstMalusFovAngle,

            secondFovRadius: characterFOV.usedSecondFovRadius / dividerFOVMalusValue,
            secondFovAngle: _secondMalusFovAngle
        );


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
                characterFOV.setFOVValuesToDefault();

                await _inventoryManager.characterFlashLight.lightOnFlashLight();

                characterFOV.setFOVValues(
                    firstFovRadius: characterFOV.usedFirstFovRadius / dividerFOVMalusFlashlightValue,
                    firstFovAngle: _firstMalusFovAngle,

                    secondFovRadius: characterFOV.usedSecondFovRadius / dividerFOVMalusFlashlightValue,
                    secondFovAngle: _secondMalusFovAngle
                );
            }
        }
        
    }

    /// <summary>
    /// Ripristina valori default del FOV
    /// </summary>
    public async Task<bool> restoreFOVMalus() {
        characterFOV.setFOVValuesToDefault();
        await _inventoryManager.characterFlashLight.lightOffFlashLight();

        return true;
    }

    /// <summary>
    /// Porta il character nello stato Dead
    /// Disabilita componenti e abilita ragdoll
    /// </summary>
    /// <param name="damageVelocity"></param>
    public void killCharacter(Vector3 damageVelocity) {

        
        resetCharacterMovmentState();

        // disabilita componenti
        gameObject.GetComponent<CharacterMovement>().enabled = false;
        gameObject.GetComponent<CharacterManager>().enabled = false;
        _inventoryManager.enabled = false;
        gameObject.GetComponent<CharacterController>().enabled = false;
        gameObject.GetComponent<CapsuleCollider>().enabled = false;

        // reset character interactable objects
        emptyAllInteractableDictionaryObjects();

        // stoppa componenti
        gameObject.GetComponent<CharacterFOV>().stopAllCoroutines();
        gameObject.GetComponent<CharacterFOV>().enabled = false;

        _inventoryManager.setInventoryAsInteractable();


        _characterAnimator.StopPlayback();
        _characterAnimator.enabled = false;



        if(!isPlayer) {

            inventoryManager.characterFlashLight.instantLightOffFlashLight();

            Role role = gameObject.GetComponent<CharacterRole>().role;
            
            if (role == Role.EnemyGuard) {

                //Destroy(gameObject.GetComponent<EnemyNPCBehaviour>());
                gameObject.GetComponent<EnemyNPCBehaviour>().enabled = false;
                gameObject.GetComponent<EnemyNPCBehaviour>().stopAllCoroutines();
                gameObject.GetComponent<EnemyNPCBehaviour>().stopAgent();

                gameObject.GetComponent<EnemyNPCBehaviour>().stopSuspiciousTimer();
                gameObject.GetComponent<EnemyNPCBehaviour>().stopHostilityCheckTimer();
            } else if (role == Role.Civilian) {

                //Destroy(gameObject.GetComponent<CivilianNPCBehaviour>());
                gameObject.GetComponent<CivilianNPCBehaviour>().enabled = false;
                gameObject.GetComponent<EnemyNPCBehaviour>().stopAllCoroutines();
                gameObject.GetComponent<EnemyNPCBehaviour>().stopAgent();
            }
        } else {
            _inventoryManager.weaponLineRenderer.enabled = false;
        }

        gameObject.GetComponent<RagdollManager>().enableRagdoll();
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
}