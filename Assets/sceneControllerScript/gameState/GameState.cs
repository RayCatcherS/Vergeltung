using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GlobalGameState {
    play,
    gameover,
    pause,
    switchCharacterMode
}

/// <summary>
/// Game state di gioco, utilizzato per accedere a stati e metodi globali che hanno ripercussioni sull'intero gioco
/// </summary>
public class GameState : MonoBehaviour {
    private GlobalGameState _gameState = GlobalGameState.play;
    public GlobalGameState gameState {
        get { return _gameState; }
    }

    [Header("Ref")]
    [SerializeField] private PlayerWarpController playerWarpController;
    private PlayerInputAction playerActions;

    [Header("UI Ref")]
    [SerializeField] private AlarmAlertUIController alarmAlertUIController;

    [Header("UI screen ref")]
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private MenuScreen pauseUIScreen;
    [SerializeField] private MenuScreen gameOverUIScreen;
    [SerializeField] private MenuScreen loadingUIScreen;
    [SerializeField] private Slider loadingSlider;




    [Header("Game state songs")]
    [SerializeField] private AudioSource gameOverAudioSource;


    [Header("Game global value state")]
    Dictionary<int, CharacterManager> globalWantedHostileCharacters = new Dictionary<int, CharacterManager>();


    private void OnEnable() {
        playerActions.Player.Enable();
    }
    private void OnDisable() {
        playerActions.Player.Disable();
    }

    void Awake() {
        playerActions = new PlayerInputAction();

    }

    private void Start() {


        gameOverUIScreen.gameObject.SetActive(false);
        loadingUIScreen.gameObject.SetActive(false);
    }


    /// <summary>
    /// Questo metodo (unione tra dictionaryToUseToUpdate e globalWantedHostileCharacters)
    /// aggiorna il dizionario globale degli NPC ostili e aggiorna il dizionario locale di tutti gli NPC
    /// </summary>
    /// <param name="dictionaryToUseToUpdate"></param>
    public void updateGlobalWantedHostileCharacters(Dictionary<int, CharacterManager> dictionaryToUseToUpdate) {


        foreach (var character in dictionaryToUseToUpdate) {
            

            if(!globalWantedHostileCharacters.ContainsKey(character.Key)) {
                globalWantedHostileCharacters.Add(character.Value.GetInstanceID(), character.Value);
            }
        }

       

        // get all game characters
        List<BaseNPCBehaviourManager> allCharactersBehaviour = gameObject.GetComponent<SceneEntitiesController>().allNpcList;
        foreach(var character in allCharactersBehaviour) {


            CharacterManager characterManager = character.gameObject.GetComponent<CharacterManager>();

            if (!characterManager.isDead) {
                character.wantedHostileCharacters = new Dictionary<int, CharacterManager>();

                foreach (var globalWantedHostileCharacter in globalWantedHostileCharacters) {
                    character.wantedHostileCharacters.Add(globalWantedHostileCharacter.Key, globalWantedHostileCharacter.Value);
                }
            }
        }
        

        // se lo stack di characters controllati è vuoto
        if(!playerWarpController.iswarpedCharacterManagerStackEmpty) {

            
            updateWantedUICharacter(playerWarpController.currentPlayedCharacter);
        }
        
    }


    /// <summary>
    /// Imposta l'icona di ricercato nell'UI
    /// Verifica se il character attualmente in utilizzo è ricercato o meno
    /// </summary>
    public void updateWantedUICharacter(CharacterManager characterManager) {


        if (globalWantedHostileCharacters.ContainsKey(characterManager.GetInstanceID())) {

            alarmAlertUIController.potentialWantedAlarmOn();
        } else {
            alarmAlertUIController.potentialWantedAlarmOff();
        }
        
    }







    
    /// <summary>
    /// Setta il gioco in stato di gameover
    /// Avvia l'UI di game over
    /// </summary>
    public async void initGameOverGameState() {

        // disable aim UI
        gameObject.GetComponent<AimUIManager>().hideAimUI();


        // stop musica in-game
        gameObject.GetComponent<GameSoundtrackController>()
            .setSoundTrackState(CharacterBehaviourSoundtrackState.noSoundtrack);


        // start canzone fine partita
        gameOverAudioSource.Play();


        // attendi e disattiva behaviour di tutti i character 
        await gameObject.GetComponent<SceneEntitiesController>()
            .stopAllCharacterBehaviourInSceneAsync(); 

        

        // setta comando action event system
        eventSystem.gameObject.GetComponent<InputSystemUIInputModule>().submit 
            = InputActionReference.Create(playerActions.MainMenu.Action);


        // game over UI
        loadingUIScreen.gameObject.SetActive(false);
        gameOverUIScreen.gameObject.SetActive(true);
        eventSystem.SetSelectedGameObject(gameOverUIScreen.firtButton.gameObject);

        // game state 
        _gameState = GlobalGameState.gameover;
    }

    public void initPauseGameState() {
        // game time
        Time.timeScale = 0;


        // setta comando action event system
        eventSystem.gameObject.GetComponent<InputSystemUIInputModule>().submit = InputActionReference.Create(playerActions.MainMenu.Action);

        // pause UI
        loadingUIScreen.gameObject.SetActive(false);
        gameOverUIScreen.gameObject.SetActive(false);
        pauseUIScreen.gameObject.SetActive(true);
        eventSystem.SetSelectedGameObject(pauseUIScreen.firtButton.gameObject);


        // game state 
        _gameState = GlobalGameState.pause;
    }

    public void resumeGameState() {
        // game time
        Time.timeScale = 1;


        // setta comando action event system
        eventSystem.gameObject.GetComponent<InputSystemUIInputModule>().submit = InputActionReference.Create(playerActions.UI.Action);

        // pause UI
        loadingUIScreen.gameObject.SetActive(false);
        gameOverUIScreen.gameObject.SetActive(false);
        pauseUIScreen.gameObject.SetActive(false);


        // rebuild UI interacion(selezionando il primo elemento (se c'è))
        playerWarpController.currentPlayedCharacter.buildListOfInteraction();

        // game state 
        _gameState = GlobalGameState.play;
    }


    public void initLoadingScreen(int sceneToLoad) {

        pauseUIScreen.gameObject.SetActive(false);
        gameOverUIScreen.gameObject.SetActive(false);
        loadingUIScreen.gameObject.SetActive(true);


        StartCoroutine(LoadSceneAsynchronously(sceneToLoad));

    }

    IEnumerator LoadSceneAsynchronously(int selectedScene) {

        AsyncOperation operation = SceneManager.LoadSceneAsync(selectedScene);

        //TODO Loading screen active

        while (!operation.isDone) {
            float progress = Mathf.Clamp01(operation.progress / .9f);
            loadingSlider.value = progress;

            yield return null;
        }
    }

    public void initSwitchCharacterMode() {
        _gameState = GlobalGameState.switchCharacterMode;

        Time.timeScale = 0.1f;
    }
}
