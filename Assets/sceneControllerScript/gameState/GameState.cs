using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Game state di gioco, utilizzato per accedere a stati e metodi globali che hanno ripercussioni sull'intero gioco
/// </summary>
public class GameState : MonoBehaviour
{
    [Header("Power settings and states")]
    [SerializeField] private int energyPowerLife = 5;
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

        energyPowerLife--;
        StartCoroutine(turnOffPowerTimed());
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

        powerOn = false;

        yield return new WaitForSeconds(powerOffTimer);

        
        if (energyPowerLife > 0) { // se le life power sono > 0

            // riattiva tutte le luci
            for (int i = 0; i < lightSources.Length; i++) {
                lightSources[i].turnOnLigth();
            }


            // chiudi tutti i cancelli
            for (int i = 0; i < electricGateControllers.Length; i++) {
                electricGateControllers[i].closeGate();
            }

            powerOn = true;
        }
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
            character.wantedHostileCharacters = globalWantedHostileCharacters;
        }

    }

    
}
