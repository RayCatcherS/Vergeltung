using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSourcesScript : MonoBehaviour
{
    // effetto luce volumetrica se esiste
    [SerializeField] private GameObject lightCone;
    [SerializeField] private Light light;

    private void Start() {
    }

    public void turnOffLigth() {
        StartCoroutine(lightOffTransition());
    }

    public void turnOnLigth() {
        StartCoroutine(lightOnTransition());
    }


    private IEnumerator lightOffTransition() {
        

        float timeWaitLightOff = Random.Range(0.05f, 0.5f);
        yield return new WaitForSeconds(timeWaitLightOff);
        setLightOff();

    }

    private IEnumerator lightOnTransition() {


        float timeWaitLightOff = Random.Range(0.05f, 0.5f);
        yield return new WaitForSeconds(timeWaitLightOff);
        setLightOn();
    }


    private void setLightOff() {
        light.enabled = false;

        if(lightCone != null) {
            lightCone.SetActive(false);
        }
    }

    private void setLightOn() {
        light.enabled = true;

        if (lightCone != null) {
            lightCone.SetActive(true);
        }
    }
}
