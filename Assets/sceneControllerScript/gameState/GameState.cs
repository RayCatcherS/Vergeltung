using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Game state di gioco, utilizzato per accedere a stati e metodi globali che hanno ripercussioni sull'intero gioco
/// </summary>
public class GameState : MonoBehaviour
{

    [Header("Ref")]
    [SerializeField] private PlayerWarpController playerWarpController;
    [SerializeField] private AlarmAlertUIController alarmAlertUIController;

    [Header("Power settings and states")]
    [SerializeField] private int powerOffTimer = 15;
    [SerializeField] private LightSourcesScript[] lightSources;
    [SerializeField] private ElectricGateController[] electricGateControllers;
    [SerializeField] private bool powerOn = true;

    [Header("Game global value state")]
    Dictionary<int, CharacterManager> globalWantedHostileCharacters = new Dictionary<int, CharacterManager>();

    private void Start() {
        lightSources = FindObjectsOfType(typeof(LightSourcesScript)) as LightSourcesScript[];
        electricGateControllers = FindObjectsOfType(typeof(ElectricGateController)) as ElectricGateController[];
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


        // chiudi tutti i cancelli
        for (int i = 0; i < electricGateControllers.Length; i++) {
            electricGateControllers[i].closeGate();
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

        updateWantedUICharacter();

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
}
