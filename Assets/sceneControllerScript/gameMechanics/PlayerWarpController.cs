using UnityEngine.AI;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("Settings")]
    [SerializeField] private bool firstCharacterPlayerIsWanted = true;


    /// <summary>
    /// warp to a new character player and push to stack the new warped character
    /// </summary>
    public void warpPlayerToCharacter(CharacterManager character) {

        // controllo primo character (è il primo character usato dal player)
        if(warpedCharacterManagerStack.Count == 0) {

            // aggiungi ref
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


            // configurazione comandi
            gameObject.GetComponent<PlayerInputController>().characterMovement = character.GetComponent<CharacterMovement>();
            gameObject.GetComponent<PlayerInputController>().inventoryManager = character.GetComponent<CharacterManager>().inventoryManager;
            gameObject.GetComponent<PlayerInputController>().characterManager = character.GetComponent<CharacterManager>();

            // configurazione camera
            gameCamera.GetComponent<CoutoutObject>().targetObject = character.occlusionTargetTransform;
            gameCamera.GetComponent<FollowPlayer>().objectToFollow = character.occlusionTargetTransform;


            // setta primo character controllato come ricercato
            if(firstCharacterPlayerIsWanted) {
                Dictionary<int, CharacterManager> wanted = new Dictionary<int, CharacterManager>();
                wanted.Add(character.GetInstanceID(), character);
                gameState.updateGlobalWantedHostileCharacters(wanted);
            }
            

            // avvia coroutines character player
            StartCoroutine(character.GetComponent<CharacterAreaManager>().belongAreaCoroutine());
        }

        // Rebuild UI
        gameState.updateWantedUICharacter();
    }

    public CharacterManager getUsingCharacter() {
        return warpedCharacterManagerStack[warpedCharacterManagerStack.Count - 1];
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
            // warp del character precedente
            warpPlayerToCharacter(warpedCharacterManagerStack[warpedCharacterManagerStack.Count - 1]);
        }
    }

    
}
