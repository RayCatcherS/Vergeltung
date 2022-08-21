using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CharacterFlashLight : MonoBehaviour
{
    
    [SerializeField] private bool flashLightOn = false;
    [SerializeField] private MeshRenderer lightCone;
    [SerializeField] private new GameObject light;

    /// <summary>
    /// Accendi luce con ritardo casuale
    /// </summary>
    /// <returns></returns>
    public async Task<bool> lightOnFlashLight() {
        float timeWaitLightOff = Random.Range(0.05f, 0.5f);
        timeWaitLightOff = timeWaitLightOff + Time.time;

        while (Time.time < timeWaitLightOff) {
            await Task.Yield();
        }

        flashLightOn = true;
        lightCone.enabled = true;
        light.SetActive(true);

        return true;
    }

    /// <summary>
    /// Spegni luce con ritardo casuale
    /// </summary>
    /// <returns></returns>
    public async Task<bool> lightOffFlashLight() {

        float timeWaitLightOff = Random.Range(0.05f, 0.5f);
        timeWaitLightOff = timeWaitLightOff + Time.time;

        while (Time.time < timeWaitLightOff) {
            await Task.Yield();
        }

        flashLightOn = false;
        lightCone.enabled = false;
        light.SetActive(false);

        return true;
    }

    /// <summary>
    /// spegni flashlight instantaneamente
    /// </summary>
    public void instantLightOffFlashLight() {

        

        flashLightOn = false;
        lightCone.enabled = false;
        light.SetActive(false);
    }
}
