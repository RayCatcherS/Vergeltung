using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSourcesScript : MonoBehaviour
{
    // effetto luce volumetrica se esiste
    [SerializeField] private GameObject lightCone;
    [SerializeField] private Light light;
    private bool _lightDisabled = false;
    public bool lightDisabled {
        set {
            _lightDisabled = value;
        }
    }

    private void Start() {
    }

    public void turnOffLigth() {

        if(!_lightDisabled) {
            StartCoroutine(lightOffTransition());
        }
        
    }

    public void turnOnLigth() {

        if(!_lightDisabled) {
            StartCoroutine(lightOnTransition());
        }
            
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


    public void setLightOff() {
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
