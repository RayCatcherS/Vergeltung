using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    [SerializeField] private int energyPowerLife = 5;

    [SerializeField] private int powerOffTimer = 15;

    [SerializeField] private LightSourcesScript[] lightSources;


    [SerializeField] private bool powerOn = true;

    private void Start() {
        lightSources = FindObjectsOfType(typeof(LightSourcesScript)) as LightSourcesScript[];
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

            powerOn = true;
        }
    }


    
}
