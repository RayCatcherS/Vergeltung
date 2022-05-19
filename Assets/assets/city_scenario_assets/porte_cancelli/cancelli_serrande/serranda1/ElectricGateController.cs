using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricGateController : MonoBehaviour
{
    [SerializeField] private Animator gateAnimator;
    [SerializeField] private Animator ligthAnim;
    [SerializeField] private float lightTimer = 2;


    
    public void openGate() {

        StartCoroutine(gateLightOn());
        gateAnimator.SetTrigger("gateOpen");
    }

    public void closeGate() {

        StartCoroutine(gateLightOn());
        gateAnimator.SetTrigger("gateClose");
    }




    IEnumerator gateLightOn() {

        ligthAnim.SetTrigger("lightOn");

        yield return new WaitForSeconds(lightTimer);
        ligthAnim.SetTrigger("lightOff");
    }
}
