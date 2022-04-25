using UnityEngine.AI;
using System.Collections.Generic;
using UnityEngine;


public class CharacterManager : MonoBehaviour {
    private const int INTERACTABLE_LAYER = 3;

    [Header("References")]
    private Dictionary<int, Interactable> interactableObjects = new Dictionary<int, Interactable>(); // dizionario Interactable ottenuti dagli onTrigger degli 
    [SerializeField] private InteractionUIController interactionUIController; // controller per interagire con l'UI delle interazioni
    [SerializeField] private InventoryManager _inventoryManager; // manager dell'intentario del character


    // stati del player
    [Header("Character States")]
    [SerializeField] public bool isRunning = false;
    [SerializeField] public bool isBusy = false;
    [SerializeField] public bool isPlayer = false; // tiene conto se il character è attualmente controllato dal giocatore
    [SerializeField] public bool isDead = false;
    [SerializeField] private CharacterManager _aimedCharacter;
    [SerializeField] private Animator _characterAnimator;

    [Header("Character Settings")]
    [SerializeField] private int characterLife = 100;

    public void Start() {

        initCharacterManager();
    }

    private void initCharacterManager() {

        _inventoryManager = gameObject.GetComponent<InventoryManager>();

        if (_inventoryManager == null) {
            gameObject.AddComponent<InventoryManager>();
        }
        _inventoryManager = gameObject.GetComponent<InventoryManager>();
    }


    //getter
    public InventoryManager inventoryManager {
        get { return _inventoryManager; }
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
    public static GameObject addToGOCharacterManagerComponent(GameObject gameObject, InteractionUIController controller) {
        
        if(gameObject.GetComponent<CharacterManager>() == null) {
            gameObject.AddComponent<CharacterManager>();
        }
        
        CharacterManager characterInteraction = gameObject.GetComponent<CharacterManager>(); // aggiungi componente CharacterInteraction 
        characterInteraction.interactionUIController = controller; // assegna al interactionUIController al componente CharacterInteraction

        return gameObject;
    }

    /// <summary>
    /// assegna il controller InteractionUIController
    /// da usare per avviare operazioni sulla UI
    /// </summary>
    /// <param name="controller"></param>
    public void setInteractionUIController(InteractionUIController controller) {
        interactionUIController = controller;
    }

    private void OnTriggerEnter(Collider collision) {
        if (collision.gameObject.layer == INTERACTABLE_LAYER) {

            InteractableObject interactableObject = collision.gameObject.GetComponent<InteractableObject>();


            // aggiungi interazione al dizionario delle interazioni
            interactableObjects.Add(interactableObject.GetInstanceID(), interactableObject.interactable);
            
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

            List<Interaction> interactable = item.Value.getInteractable();
            
            for(int i = 0; i < interactable.Count; i++) {

                interactions.Add(interactable[i]);
            }
        }


        // se il character è giocato dal player
        if(isPlayer) {

            // inizializza lista di interazioni e i bottoni e la partendo dalla lista interactions
            // passa la lista di interactions per inizializzare la lista di interacion che potranno essere effettuate
            interactionUIController.buildUIinteractionList(interactions, this);
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
            characterLife -= damage;

            if (characterLife <= 0) {
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

        isRunning = false;
        isBusy = false;

        Destroy(gameObject.GetComponent<CharacterMovement>());
        gameObject.GetComponent<CharacterManager>().enabled = false;
        gameObject.GetComponent<InventoryManager>().enabled = false;

        Destroy(gameObject.GetComponent<CharacterController>());
        Destroy(gameObject.GetComponent<CapsuleCollider>());

        //Destroy(_characterAnimator);
        _characterAnimator.StopPlayback();
        _characterAnimator.enabled = false;

        if(!isPlayer) {
            Destroy(gameObject.GetComponent<NavMeshAgent>());

            Role role = gameObject.GetComponent<CharacterRole>().role;

            if(role == Role.Enemy) {

                Destroy(gameObject.GetComponent<EnemyNPCBehaviour>());
                //gameObject.GetComponent<EnemyNPCBehaviour>().enabled = false;

            } else if (role == Role.Civilian) {
                Destroy(gameObject.GetComponent<CivilianNPCBehaviour>());
                //gameObject.GetComponent<CivilianNPCBehaviour>().enabled = false;
            }
        } else {

        }

        gameObject.GetComponent<RagdollManager>().enableRagdoll();
    }
}