using UnityEngine.AI;
using System.Collections.Generic;
using UnityEngine;


public class CharacterManager : MonoBehaviour {
    private const int INTERACTABLE_LAYER = 3;

    
    private Dictionary<int, Interactable> interactableObjects = new Dictionary<int, Interactable>(); // dizionario Interactable ottenuti dagli onTrigger degli 

    [Header("References")]
    [SerializeField] private InteractionUIController _interactionUIController; // controller per interagire con l'UI delle interazioni
    [SerializeField] private WeaponUIController _weaponUIController; // ref controller per visualizzare l'UI delle armi
    [SerializeField] private InventoryManager _inventoryManager; // manager dell'intentario del character
    [SerializeField] private Transform _occlusionTargetTransform; // occlusion target che permette di capire quando il character è occluso tra la camera è un oggetto

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
                    _aimedCharacter.GetComponent<Outline>().setEnableOutline(false); // disattiva outline del character precedentemente mirato
                    _aimedCharacter = value;
                }
            } else {

                if (_aimedCharacter != null) { // si stava già mirando un character
                    _aimedCharacter.GetComponent<Outline>().setEnableOutline(false);
                    _aimedCharacter = value;
                    _aimedCharacter.GetComponent<Outline>().setEnableOutline(true);
                } else {
                    _aimedCharacter = value;
                    _aimedCharacter.GetComponent<Outline>().setEnableOutline(true);
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
    public static GameObject initCharacterManagerComponent(GameObject gameObject, InteractionUIController controller) {
        
        CharacterManager characterInteraction = gameObject.GetComponent<CharacterManager>(); // aggiungi componente CharacterInteraction 
        characterInteraction._interactionUIController = controller; // assegna al interactionUIController al componente CharacterInteraction

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

        // reset interactable objects
        resetAllInteractableObjects();

        // stoppa componenti
        gameObject.GetComponent<CharacterFOV>().stopAllCoroutines();
        gameObject.GetComponent<CharacterFOV>().enabled = false;

        _inventoryManager.setInventoryAsInteractable();


        _characterAnimator.StopPlayback();
        _characterAnimator.enabled = false;



        if(!isPlayer) {
            gameObject.GetComponent<NavMeshAgent>().enabled = false;

            Role role = gameObject.GetComponent<CharacterRole>().role;

            if(role == Role.Enemy) {

                //Destroy(gameObject.GetComponent<EnemyNPCBehaviour>());
                gameObject.GetComponent<EnemyNPCBehaviour>().enabled = false;

            } else if (role == Role.Civilian) {
                //Destroy(gameObject.GetComponent<CivilianNPCBehaviour>());
                gameObject.GetComponent<CivilianNPCBehaviour>().enabled = false;
            }
        } else {
            _inventoryManager.weaponLineRenderer.enabled = false;
        }

        gameObject.GetComponent<RagdollManager>().enableRagdoll();
    }

    public void resetAllInteractableObjects() {
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