using MagicLightmapSwitcher;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenePowerController : MonoBehaviour
{
    [Header("Power settings and states")]
    [SerializeField] private LightSourcesScript[] lightSources;
    [SerializeField] private BuildingWindows[] buildingWindows;
    [SerializeField] private ElectricGateController[] electricGateControllers;
    [SerializeField] private GeneratorInteractable[] generatorInteractables;
    [SerializeField] private bool powerOn = true;

    [Header("Audio Ref")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip powerOffClip;
    [SerializeField] private AudioClip powerOnClip;

    private static ScenePowerController _instance;

    public static ScenePowerController Instance { get { return _instance; } }

    private void Awake() {
        if(_instance != null && _instance != this) {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
    }

    void Start() {

        lightSources = FindObjectsOfType(typeof(LightSourcesScript)) as LightSourcesScript[];
        buildingWindows = FindObjectsOfType(typeof(BuildingWindows)) as BuildingWindows[];
        electricGateControllers = FindObjectsOfType(typeof(ElectricGateController)) as ElectricGateController[];

        generatorInteractables = FindObjectsOfType(typeof(GeneratorInteractable)) as GeneratorInteractable[];
    }
    // getter
    public bool getPowerOn() {
        return powerOn;
    }


    /// <summary>
    /// disattiva momentaneamente la corrente se ci sono ancora lifePower
    /// Altrimenti se lifePower == 0 disattiva permanentemente la corrente
    /// </summary>
    public void turnOffPower(int powerOffTimer) {

        if(powerOn) {
            StartCoroutine(turnOffPowerTimed(powerOffTimer));
        }
    }

    private IEnumerator turnOffPowerTimed(int powerOffTimer) {

        // clip audio power on
        audioSource.clip = powerOffClip;
        audioSource.Play();

        // wait iniziale
        yield return new WaitForSeconds(1f);

        // switch delle light map su light off
        LightMapSwitcher.SwitchToLightmap(LigthMap.noLight);

        // apri tutti i cancelli
        for(int i = 0; i < electricGateControllers.Length; i++) {
            electricGateControllers[i].openGateByPowerOff(powerOffTimer);
        }



        // disattiva tutte le luci
        for(int i = 0; i < lightSources.Length; i++) {
            lightSources[i].turnOffLigth();
        }

        // disattiva tutte le finestre
        for(int i = 0; i < buildingWindows.Length; i++) {
            buildingWindows[i].turnOffLigth();
        }
        

        // applica FOV malus a tutti i character della scena
        List<CharacterManager> characterManagers = gameObject.GetComponent<SceneEntitiesController>().getAllNPC();
        for(int i = 0; i < characterManagers.Count; i++) {

            characterManagers[i].applyFOVMalus();
        }

        
        powerOn = false;

        yield return new WaitForSeconds(powerOffTimer);


        // clip audio power on
        audioSource.clip = powerOnClip;
        audioSource.Play();

        // riattiva tutte le luci
        for(int i = 0; i < lightSources.Length; i++) {
            lightSources[i].turnOnLigth();
        }

        // riattiva tutte le finestre
        for(int i = 0; i < buildingWindows.Length; i++) {
            buildingWindows[i].turnOnLigth();
        }

        // rimuovi FOV malus a tutti i character della scena
        for(int i = 0; i < characterManagers.Count; i++) {

            _ = characterManagers[i].restoreFOVMalus();
        }


        // riattiva tutti i generatori
        // rende i generatori nuovamente riutilizzabili
        foreach(GeneratorInteractable generator in generatorInteractables)
        {
            generator.switchOnGenerator();
        }

        // rebuild warp character attualmente usato per rebuildare le interactions
        // refreshando le interactions possibile
        gameObject.GetComponent<PlayerWarpController>().currentPlayedCharacter.forceTriggerDetection();


        // switch delle light map su light off
        LightMapSwitcher.SwitchToLightmap(LigthMap.light);

        powerOn = true;
    }
}
