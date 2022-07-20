using UnityEngine.AI;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class PlayerWarpController : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] private Camera gameCamera;
    [SerializeField] private GameState gameState;

    [Header("Warp state")]
    [SerializeField] private List<CharacterManager> warpedCharacterManagerStack = new List<CharacterManager>();
    public bool iswarpedCharacterManagerStackEmpty {
        get {
            if(warpedCharacterManagerStack.Count == 0) {
                return true;
            } else {
                return false;
            }
        }
    }
    [SerializeField] private CharacterManager firstPlayerCharacter; // primo character usato dal player, se muore si fallisce
    [SerializeField] private CharacterManager _currentPlayedCharacter; // character attualmente usato 
    public CharacterManager currentPlayedCharacter {
        get { return _currentPlayedCharacter; }
    }

    [Header("Settings")]
    [SerializeField] private bool firstCharacterPlayerIsWanted = true;

    

    /// <summary>
    /// warp to a new character player and push to stack the new warped character
    /// </summary>
    public async Task warpPlayerToCharacterAsync(CharacterManager character) {

        Ammunition controlAmmoT = new Ammunition(WeaponType.controlWeapon, 0);

        if(warpedCharacterManagerStack.Count != 0) {
            //disabilita character precedente
            disablePreviewControlledCharacter(_currentPlayedCharacter);

            // salva munizioni controllo
            controlAmmoT = _currentPlayedCharacter.inventoryManager.inventoryAmmunitions[WeaponType.controlWeapon];
        }
        _currentPlayedCharacter = character.GetComponent<CharacterManager>();

        

        // controllo primo character (è il primo character usato dal player)
        if(warpedCharacterManagerStack.Count == 0) {
            

            // aggiungi ref del character allo stack
            warpedCharacterManagerStack.Add(character);

            // aggiungi primo character giocato dal player
            firstPlayerCharacter = character;

            // configura character
            character.resetCharacterStates();

            //disabilita componenti non necessari
            character.characterOutline.enabled = false;
            character.gameObject.GetComponent<NavMeshAgent>().enabled = false;
            character.gameObject.GetComponent<CharacterFOV>().enabled = false;

            //abilita componenti necessari
            character.gameObject.GetComponent<NavMeshObstacle>().enabled = true;

            // configurazione character controllato dall'utente
            character.isPlayer = true;

            // configurazione UI
            character.interactionUIController = gameObject.GetComponent<InteractionUIController>();
            character.weaponUIController = gameObject.GetComponent<WeaponUIController>();
            character.alarmAlertUIController = gameObject.GetComponent<AlarmAlertUIController>();
            character.inventoryManager.aimTargetImage = gameObject.GetComponent<AimUIManager>();


            // configurazione comandi
            gameObject.GetComponent<GameInputManager>().characterMovement = character.GetComponent<CharacterMovement>();
            gameObject.GetComponent<GameInputManager>().inventoryManager = character.GetComponent<CharacterManager>().inventoryManager;
            gameObject.GetComponent<GameInputManager>().characterManager = character.GetComponent<CharacterManager>();

            // configurazione camera
            gameCamera.GetComponent<CoutoutObject>().targetObject = character.occlusionTargetTransform;
            gameCamera.GetComponent<FollowPlayer>().objectToFollow = character.occlusionTargetTransform;

            // configurazione audio listener
            character.GetComponent<AudioListener>().enabled = true;


            // setta primo character controllato come ricercato
            if(firstCharacterPlayerIsWanted) {
                Dictionary<int, CharacterManager> wanted = new Dictionary<int, CharacterManager>();

                wanted.Add(character.GetInstanceID(), character);
                gameState.updateGlobalWantedHostileCharacters(wanted);
                
            }
            

            // avvia coroutines character player
            StartCoroutine(character.GetComponent<CharacterAreaManager>().belongAreaCoroutine());
        } else {
            // controlla character


            // passa munizioni control weapon al character che si vuole controllare
            character.inventoryManager.inventoryAmmunitions[WeaponType.controlWeapon]
                = controlAmmoT;


            gameObject.GetComponent<GameInputManager>().characterMovement = null;
            // stop character behaviour
            // stop behaviour
            if(character.baseNPCBehaviourManager != null) {
                await character.baseNPCBehaviourManager.forceStopCharacterAndAwaitStopProcess();
            }
            

            // aggiungi ref del character allo stack
            warpedCharacterManagerStack.Add(character);

            // configura character
            character.resetCharacterStates();

            //disabilita componenti non necessari
            character.characterOutline.enabled = false;
            character.gameObject.GetComponent<NavMeshAgent>().enabled = false;
            character.gameObject.GetComponent<CharacterFOV>().enabled = false;

            //abilita componenti necessari
            character.gameObject.GetComponent<NavMeshObstacle>().enabled = true;

            // configurazione character controllato dall'utente
            character.isPlayer = true;

            // configurazione UI
            character.interactionUIController = gameObject.GetComponent<InteractionUIController>();
            character.weaponUIController = gameObject.GetComponent<WeaponUIController>();
            character.alarmAlertUIController = gameObject.GetComponent<AlarmAlertUIController>();
            character.inventoryManager.aimTargetImage = gameObject.GetComponent<AimUIManager>();

            // configurazione comandi
            gameObject.GetComponent<GameInputManager>().characterMovement = character.GetComponent<CharacterMovement>();
            gameObject.GetComponent<GameInputManager>().inventoryManager = character.GetComponent<CharacterManager>().inventoryManager;
            gameObject.GetComponent<GameInputManager>().characterManager = character.GetComponent<CharacterManager>();

            // configurazione camera
            gameCamera.GetComponent<CoutoutObject>().targetObject = character.occlusionTargetTransform;
            gameCamera.GetComponent<FollowPlayer>().objectToFollow = character.occlusionTargetTransform;

            // configurazione audio listener
            character.GetComponent<AudioListener>().enabled = true;
        }
        

        character.weaponUIController.buildUI(character.inventoryManager);



        // Rebuild UI
        gameState.updateWantedUICharacter();
    }


    /// <summary>
    /// Unstack del character morto 
    /// </summary>
    /// <param name="character"></param>
    public void unstackDeadCharacterAndControlPreviewCharacter(CharacterManager character) {
        
        if(character.GetInstanceID() == firstPlayerCharacter.GetInstanceID()) {

            // game over
            Debug.Log("player dead");
            gameState.initGameOverGameState();
        }


        // rimozione del character dallo stack
        for (int i = 0; i < warpedCharacterManagerStack.Count; i++) {

            if (warpedCharacterManagerStack[i].GetInstanceID() == character.GetInstanceID()) {
                warpedCharacterManagerStack.RemoveAt(i);
            }
        }

        
        if (warpedCharacterManagerStack.Count > 0) {
            print("unstack");

            // warp del character precedente
            warpPlayerToCharacterAsync(warpedCharacterManagerStack[warpedCharacterManagerStack.Count - 1]);
        }
    }

    private void disablePreviewControlledCharacter(CharacterManager previewCharacter) {


        // configurazione audio listener
        previewCharacter.GetComponent<AudioListener>().enabled = false;

        // configura character
        previewCharacter.resetCharacterStates();

        // configurazione character controllato dall'utente
        previewCharacter.isPlayer = false;

        // disabilita line rendere
        previewCharacter.inventoryManager.setActiveLineRenderer(false);
        
        // stoppa animazione character
        previewCharacter.characterMovement.stopCharacter();

        previewCharacter.aimedCharacter = null;
    }
}
