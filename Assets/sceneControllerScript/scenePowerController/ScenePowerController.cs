using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenePowerController : MonoBehaviour
{
    [Header("Power settings and states")]
    [SerializeField] private int powerOffTimer = 20;
    [SerializeField] private LightSourcesScript[] lightSources;
    [SerializeField] private ElectricGateController[] electricGateControllers;
    [SerializeField] private GeneratorInteractable[] generatorInteractables;
    [SerializeField] private bool powerOn = true;

    [Header("Audio Ref")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip powerOffClip;
    [SerializeField] private AudioClip powerOnClip;

    void Start() {

        lightSources = FindObjectsOfType(typeof(LightSourcesScript)) as LightSourcesScript[];
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
    public void turnOffPower() {

        if(powerOn) {
            StartCoroutine(turnOffPowerTimed());
        }
    }

    private IEnumerator turnOffPowerTimed() {

        // clip audio power on
        audioSource.clip = powerOffClip;
        audioSource.Play();

        // wait iniziale
        yield return new WaitForSeconds(1f);

        // apri tutti i cancelli
        for(int i = 0; i < electricGateControllers.Length; i++) {
            electricGateControllers[i].openGateByPowerOff();
        }



        // disattiva tutte le luci
        for(int i = 0; i < lightSources.Length; i++) {
            lightSources[i].turnOffLigth();
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


        powerOn = true;
    }
}
