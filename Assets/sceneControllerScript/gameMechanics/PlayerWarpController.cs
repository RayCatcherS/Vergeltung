using UnityEngine.AI;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.EventSystems;

public class PlayerWarpController : MonoBehaviour
{
    private const int TIME_MULTIPLIER_SC_MODE= 10;
    private const int STANDART_TIME_MULTIPLIER = 1;

    [Header("Assets")]
    [SerializeField] private AudioClip warpCharacterClip;
    [SerializeField] private AudioSource warpCharacterSource;

    [Header("Refs")]
    [SerializeField] private Camera gameCamera;
    [SerializeField] private GameState gameState;
    [SerializeField] private WarpUIController warpUIController;
    [SerializeField] private LineRenderer controlCharacterChainLR;
    [SerializeField] private Animator switchCharacterModeEffect;

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
    private CharacterManager firstPlayerCharacter; // primo character usato dal player, se muore si fallisce
    private CharacterManager _currentPlayedCharacter; // character attualmente usato 
    private CharacterManager _currentSwitchCharacterMode; // character attualmente selezionato durante la switch mode
    public CharacterManager currentPlayedCharacter {
        get { return _currentPlayedCharacter; }
    }

    [Header("Settings")]
    [SerializeField] private bool firstCharacterPlayerIsWanted = true;

    

    /// <summary>
    /// warp to a new character player and push to stack the new warped character
    /// </summary>
    public void addCharacterToWarpStack(CharacterManager character) {

        // aggiungi ref del character allo stack
        warpedCharacterManagerStack.Add(character);

        
    }


    public async Task warpToCharacter(CharacterManager character, bool isPlayer = false) {
        Ammunition controlAmmoT = new Ammunition(WeaponType.controlWeapon, 0);
        

        if(!isPlayer) {
            //disabilita character precedente
            disableControlledCharacter(_currentPlayedCharacter);
        }


        if(warpedCharacterManagerStack.Count > 1) {
            

            // istanza munizioni controllo
            controlAmmoT = _currentPlayedCharacter.inventoryManager.inventoryAmmunitions[WeaponType.controlWeapon];
        }


        _currentPlayedCharacter = character.GetComponent<CharacterManager>();

        // controllo primo character (� il primo character usato dal player)
        if(isPlayer) {
            
            // aggiungi primo character giocato dal player
            firstPlayerCharacter = character;

            

            // setta primo character controllato come ricercato
            if(firstCharacterPlayerIsWanted) {

                Dictionary<int, CharacterManager> wanted = new Dictionary<int, CharacterManager>();

                wanted.Add(character.GetInstanceID(), character);
                gameState.updateGlobalWantedHostileCharacters(wanted);

            }

            
        } else {


            // passa munizioni control weapon al character che si vuole controllare
            if(warpedCharacterManagerStack.Count > 1) {
                character.inventoryManager.inventoryAmmunitions[WeaponType.controlWeapon]
                = controlAmmoT;
            }



            gameObject.GetComponent<GameInputManager>().characterMovement = null;
            // stop character behaviour
            // stop behaviour
            if(character.baseNPCBehaviourManager != null) {
                await character.baseNPCBehaviourManager.forceStopCharacterAndAwaitStopProcess();
            }
            
            
        }

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
        character.healthBarController = gameObject.GetComponent<HealthBarUIController>();
        character.damageAnimation = gameObject.GetComponent<DamageScreenEffect>();
        character.buildUIHealthBar(); // rebuild UI health character


        // configurazione comandi
        gameObject.GetComponent<GameInputManager>().characterMovement = character.GetComponent<CharacterMovement>();
        gameObject.GetComponent<GameInputManager>().inventoryManager = character.GetComponent<CharacterManager>().inventoryManager;
        gameObject.GetComponent<GameInputManager>().characterManager = character.GetComponent<CharacterManager>();

        // configurazione camera
        gameCamera.GetComponent<CoutoutObject>().targetObject = character.occlusionTargetTransform;
        gameCamera.GetComponent<FollowPlayer>().objectToFollow = character.occlusionTargetTransform;

        // setta icona character controllo (solo characters non player)
        if(character.GetInstanceID() != firstPlayerCharacter.GetInstanceID()) {
            // setta icona character controllo
            character.gameObject.GetComponent<ControlIconManager>().setAsStackedControlled();
        }
        

        // configurazione audio listener
        character.GetComponent<AudioListener>().enabled = true;

        // avvia coroutines character 
        character.GetComponent<CharacterAreaManager>().startAreaCheckMemberShipCoroutine();

        
        character.isStackControlled = true;
        
        // Rebuild UI
        gameState.updateWantedUICharacter();

        warpUIController.rebuildWarpUI(warpedCharacterManagerStack, _currentPlayedCharacter);

        // rebuild list of interactions
        character.buildListOfInteraction();
        
        // forza interactable obj detection
        character.detectTrigger();

        // sound effect
        warpCharacterSource.clip = warpCharacterClip;
        warpCharacterSource.Play();

        
        // rebuild dell'interfaccia weapons
        character.weaponUIController.buildUI(character.inventoryManager);
        

        
    }

    /// <summary>
    /// Unstack del character morto 
    /// </summary>
    /// <param name="character"></param>
    public async void unstackDeadCharacterAndControlPreviewCharacter(CharacterManager character) {

        Ammunition controlAmmoT = new Ammunition(WeaponType.controlWeapon, 0);
        // salva munizioni controllo
        controlAmmoT = character.inventoryManager.inventoryAmmunitions[WeaponType.controlWeapon];



        // se muore il player, muoiono tutti gli stacked characters
        if(character.GetInstanceID() == firstPlayerCharacter.GetInstanceID()) {

            // game over
            Debug.Log("player dead");

            for(int i = 0; i < warpedCharacterManagerStack.Count; i++) {

                warpedCharacterManagerStack[i].isStackControlled = false;
                await warpedCharacterManagerStack[i].killCharacterAsync(Vector3.zero);


                warpedCharacterManagerStack[i].gameObject.GetComponent<ControlIconManager>().setAsUnstackedNotControlled();
                
            }
            gameState.initGameOverGameState();
        }  else {

            // rimozione del character dallo stack
            for(int i = 0; i < warpedCharacterManagerStack.Count; i++) {

                if(warpedCharacterManagerStack[i].GetInstanceID() == character.GetInstanceID()) {
                    warpedCharacterManagerStack.RemoveAt(i);
                }


            }



            if(warpedCharacterManagerStack.Count > 0) {

                // warp del character precedente
                warpToCharacter(warpedCharacterManagerStack[warpedCharacterManagerStack.Count - 1]);
            }

            // setta icona character controllo
            character.gameObject.GetComponent<ControlIconManager>().setAsUnstackedNotControlled();
        }

    }

    private void disableControlledCharacter(CharacterManager previewCharacter) {


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

        // stoppa coroutines
        previewCharacter.GetComponent<CharacterAreaManager>().stopAreaCheckMemberShipCoroutine();

        // UI
        // unref UI
        previewCharacter.interactionUIController = null;
        previewCharacter.weaponUIController = null;
        previewCharacter.alarmAlertUIController = null;
        previewCharacter.inventoryManager.aimTargetImage = null;
        previewCharacter.healthBarController = null;
        // clear interactions e rebuild UI
        previewCharacter.emptyAllInteractableDictionaryObjects();
        


        // setta icona character controllo (solo characters non player)
        if(previewCharacter.GetInstanceID() != firstPlayerCharacter.GetInstanceID()) {
            // setta icona character controllo
            previewCharacter.gameObject.GetComponent<ControlIconManager>().setAsStackedNotControlled();
        }
    }

    
    // Avvia la modalit� per switchare i characters controllati,
    // scegliendone quale controllare
    public void startSwitchCharacterMode() {

        if(gameState.gameState == GlobalGameState.play) {


            if(warpedCharacterManagerStack.Count > 1) {

                // cambio game state
                gameState.initSwitchCharacterMode();

                // draw della catena di controllo
                _ = drawControlCharacterChain();

                Time.timeScale = 0.1f;

                // disabilita controllo sul character attualmente controllato
                disableControlledCharacter(_currentPlayedCharacter);

                // seleziona character per la switch mode
                selectSwitchCharacter(_currentPlayedCharacter);

                // start effetto post processing
                switchCharacterModeEffect.SetTrigger("start");
                switchCharacterModeEffect.speed = switchCharacterModeEffect.speed * TIME_MULTIPLIER_SC_MODE;


            }
            
        }
        
    }


    public void endSwitchCharacterMode() {
        Time.timeScale = STANDART_TIME_MULTIPLIER;

        for(int i = 0; i < warpedCharacterManagerStack.Count; i++) {
            _currentSwitchCharacterMode.gameObject.GetComponent<ControlIconManager>().setAnimatorMultiplierSpeed(STANDART_TIME_MULTIPLIER);
            _currentSwitchCharacterMode.gameObject.GetComponent<ControlIconManager>().setAsStackedNotControlled();
        }


        // start effetto post processing
        switchCharacterModeEffect.SetTrigger("end");
        switchCharacterModeEffect.speed = switchCharacterModeEffect.speed * STANDART_TIME_MULTIPLIER;
    }

    public void nextSwitchCharacter() {
        

        int nextCharacterPos = getCharacterPositionInStack(_currentSwitchCharacterMode);
        nextCharacterPos++;

        if(nextCharacterPos > warpedCharacterManagerStack.Count - 1) {

            nextCharacterPos = 0;
        }

        selectSwitchCharacter(warpedCharacterManagerStack[nextCharacterPos]);
    }

    public void previousSwitchCharacter() {


        int previousCharacterPos = getCharacterPositionInStack(_currentSwitchCharacterMode);
        previousCharacterPos--;

        if(previousCharacterPos < 0) {
            previousCharacterPos = warpedCharacterManagerStack.Count - 1;
        }

        selectSwitchCharacter(warpedCharacterManagerStack[previousCharacterPos]);
    }

    private void selectSwitchCharacter(CharacterManager characterManager) {

        if(_currentSwitchCharacterMode != null) {
            _currentSwitchCharacterMode.GetComponent<AudioListener>().enabled = false;


            if(characterManager.GetInstanceID() != firstPlayerCharacter.GetInstanceID()) {
                _currentSwitchCharacterMode.gameObject.GetComponent<ControlIconManager>().setAnimatorMultiplierSpeed(TIME_MULTIPLIER_SC_MODE);
                _currentSwitchCharacterMode.gameObject.GetComponent<ControlIconManager>().setAsStackedNotControlled();
            }
                
        }

        _currentSwitchCharacterMode = characterManager;

        // setta icona 
        if(characterManager.GetInstanceID() != firstPlayerCharacter.GetInstanceID()) {
            _currentSwitchCharacterMode.gameObject.GetComponent<ControlIconManager>().setAnimatorMultiplierSpeed(TIME_MULTIPLIER_SC_MODE);
            _currentSwitchCharacterMode.gameObject.GetComponent<ControlIconManager>().setAsStackedControlled();
        }
        

        // abilita audio listener
        _currentSwitchCharacterMode.GetComponent<AudioListener>().enabled = true;


        // configurazione camera
        gameCamera.GetComponent<CoutoutObject>().targetObject = _currentSwitchCharacterMode.occlusionTargetTransform;
        gameCamera.GetComponent<FollowPlayer>().objectToFollow = _currentSwitchCharacterMode.occlusionTargetTransform;
        gameCamera.GetComponent<FollowPlayer>().cameraSwitchCharacterModeEnable = true;
    }




    private async Task drawControlCharacterChain() {
        

        while(gameState.gameState == GlobalGameState.switchCharacterMode) {
            await Task.Yield();
            

            if(warpedCharacterManagerStack.Count > 1) {


                controlCharacterChainLR.positionCount = warpedCharacterManagerStack.Count + 1;
                for(int i = 0; i < warpedCharacterManagerStack.Count; i++) {

                    Vector3 warpIconPosT = warpedCharacterManagerStack[i].gameObject.GetComponent<ControlIconManager>().characterControlIconTransfom.position;
                    controlCharacterChainLR.SetPosition(i, warpIconPosT);

                }


                // line che unisce character finale con quello iniziale
                if(warpedCharacterManagerStack.Count > 2) { // drawata solo quando i character controllati # > 2
                    Vector3 pos = controlCharacterChainLR.GetPosition(0);
                    controlCharacterChainLR.SetPosition(warpedCharacterManagerStack.Count, pos);
                }
                
            }
        }
    }


    private int getCharacterPositionInStack(CharacterManager character) {
        int pos = 0;

        for(int i = 0; i < warpedCharacterManagerStack.Count; i++) {

            if(warpedCharacterManagerStack[i].GetInstanceID() == character.GetInstanceID()) {
                pos = i;
                break;
            }
        }


        return pos;
    }
}
