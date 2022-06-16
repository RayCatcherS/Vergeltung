using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Game state di gioco, utilizzato per accedere a stati e metodi globali che hanno ripercussioni sull'intero gioco
/// </summary>
public class GameState : MonoBehaviour
{

    [Header("Ref")]
    [SerializeField] private PlayerWarpController playerWarpController;
    private PlayerInputAction playerActions;

    [Header("UI Ref")]
    [SerializeField] private AlarmAlertUIController alarmAlertUIController;

    [Header("UI screen ref")]
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private MenuScreen gameOverUIScreen;
    [SerializeField] private MenuScreen LoadingUIScreen;
    [SerializeField] private Slider loadingSlider;

    [Header("Power settings and states")]
    [SerializeField] private int powerOffTimer = 15;
    [SerializeField] private LightSourcesScript[] lightSources;
    [SerializeField] private ElectricGateController[] electricGateControllers;
    [SerializeField] private bool powerOn = true;


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
        lightSources = FindObjectsOfType(typeof(LightSourcesScript)) as LightSourcesScript[];
        electricGateControllers = FindObjectsOfType(typeof(ElectricGateController)) as ElectricGateController[];

        gameOverUIScreen.gameObject.SetActive(false);
        LoadingUIScreen.gameObject.SetActive(false);
    }

    // getter
    public bool getPowerOn() {
        return powerOn;
    }

    /// <summary>
    /// disattiva momentaneamente la corrente se ci sono ancora lifePower
    /// Altrimenti se lifePower == 0 disattiva permanentemente la corrente
    /// </summary>
    public void turnOffPower() {

        if(powerOn) {
            StartCoroutine(turnOffPowerTimed());
        }
    }

    private IEnumerator turnOffPowerTimed() {

        // wait iniziale
        yield return new WaitForSeconds(1f);

        // apri tutti i cancelli
        for (int i = 0; i < electricGateControllers.Length; i++) {
            electricGateControllers[i].openGate();
        }
        


        // disattiva tutte le luci
        for (int i = 0; i < lightSources.Length; i++) {
            lightSources[i].turnOffLigth();
        }


        // applica FOV malus a tutti i character della scena
        List<CharacterManager> characterManagers = gameObject.GetComponent<SceneEntitiesController>().getAllNPC();
        for (int i = 0; i < characterManagers.Count; i++) {

            characterManagers[i].applyFOVMalus();
        }

        powerOn = false;

        yield return new WaitForSeconds(powerOffTimer);


        // riattiva tutte le luci
        for (int i = 0; i < lightSources.Length; i++) {
            lightSources[i].turnOnLigth();
        }

        // rimuovi FOV malus a tutti i character della scena
        for (int i = 0; i < characterManagers.Count; i++) {

            _ = characterManagers[i].restoreFOVMalus();
        }

        powerOn = true;
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
        List<BaseNPCBehaviour> allCharactersBehaviour = gameObject.GetComponent<SceneEntitiesController>().allNpcList;
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
            updateWantedUICharacter();
        }
        

    }


    /// <summary>
    /// Imposta l'icona di ricercato nell'UI
    /// Verifica se il character attualmente in utilizzo è ricercato o meno
    /// </summary>
    public void updateWantedUICharacter() {
        CharacterManager characterManager = playerWarpController.getUsingCharacter();


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
    public async Task initGameOverGameState() {
        // start canzone fine partita
        gameOverAudioSource.Play();

        // switch input 
        playerActions = new PlayerInputAction();
        playerActions.Player.Disable();
        playerActions.MainMenu.Enable();

        await gameObject.GetComponent<SceneEntitiesController>().stopAllCharacterBehaviourInSceneAsync(); // attendi e disattiva behaviour di tutti i character 

        // game over UI

        // setta comando action event system
        eventSystem.gameObject.GetComponent<InputSystemUIInputModule>().submit = InputActionReference.Create(playerActions.MainMenu.Action);

        LoadingUIScreen.gameObject.SetActive(false);
        gameOverUIScreen.gameObject.SetActive(true);
        eventSystem.SetSelectedGameObject(gameOverUIScreen.firtButton.gameObject);
    }

    public void initLoadingScreen(int sceneToLoad) {

        gameOverUIScreen.gameObject.SetActive(false);
        LoadingUIScreen.gameObject.SetActive(true);


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
}
