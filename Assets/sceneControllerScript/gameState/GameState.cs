using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    [SerializeField] private int lifePower = 5;

    [SerializeField] private int powerOffTimer = 15;

    [SerializeField] private List<LightSourcesScript> lightSources = new List<LightSourcesScript>();


    [SerializeField] private bool powerOn = true;

    // getter
    public bool getPowerOn() {
        return powerOn;
    }

    /// <summary>
    /// disattiva momentaneamente la corrente se ci sono ancora lifePower
    /// Altrimenti se lifePower == 0 disattiva permanentemente la corrente
    /// </summary>
    public void turnOffPower() {

        lifePower--;
        StartCoroutine(turnOffPowerTimed());
    }


    

    private IEnumerator turnOffPowerTimed() {

        // wait iniziale
        yield return new WaitForSeconds(1f);

        // disattiva tutte le luci
        for (int i = 0; i < lightSources.Count; i++) {
            lightSources[i].turnOffLigth();
        }

        powerOn = false;

        yield return new WaitForSeconds(powerOffTimer);

        
        if (lifePower > 0) { // se le life power sono > 0

            // riattiva tutte le luci
            for (int i = 0; i < lightSources.Count; i++) {
                lightSources[i].turnOnLigth();
            }

            powerOn = true;
        }
    }


    
}
