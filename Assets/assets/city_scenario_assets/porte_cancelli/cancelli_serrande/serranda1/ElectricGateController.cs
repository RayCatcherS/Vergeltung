using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricGateController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Animator gateAnimator;
    [SerializeField] private Animator ligthAnim;
    [SerializeField] private float busyGateTimer = 2; // tempo per cui il cancello resta occupato(usato per evitare interazioni durante l'animazione
    [SerializeField] private bool busy = false;

    [SerializeField] private bool _gateClosed = true;
    [SerializeField] private float gateClosingTime = 7;
    [SerializeField] private AudioSource audioSource;

    [Header("Asset Refs")]
    [SerializeField] private AudioClip gateAudioClip;

    public bool gateClosed {
        get { return _gateClosed; }
    }



    public void openGate() {
        playGateSound();

        // resetta stato animazioni cancello e coroutine penzolanti
        StopAllCoroutines();
        gateAnimator.ResetTrigger("openDirection1");
        gateAnimator.ResetTrigger("close");



        _gateClosed = false;
        StartCoroutine(gateLightOn());
        StartCoroutine(gateClosingTimeOut());
        gateAnimator.SetTrigger("openDirection1");
    }

    public void closeGate() {
        playGateSound();

        gateAnimator.ResetTrigger("openDirection1");
        gateAnimator.ResetTrigger("close");

        _gateClosed = true;
        StartCoroutine(gateLightOn());
        gateAnimator.SetTrigger("close");
    }

    public void operateGate() {

        if(!busy) {

            if (gateClosed) {
                openGate();
            } else {
                closeGate();
            }
        }
        
    }




    IEnumerator gateLightOn() {
        // resetta stato animazione luci
        ligthAnim.ResetTrigger("lightOn");
        ligthAnim.ResetTrigger("lightOff");

        busy = true;
        ligthAnim.SetTrigger("lightOn");

        yield return new WaitForSeconds(busyGateTimer);
        ligthAnim.SetTrigger("lightOff");

        busy = false;
    }


    IEnumerator gateClosingTimeOut() {
        yield return new WaitForSeconds(gateClosingTime);

        if (!gateClosed) {
            closeGate(); // chiudi cancello
        }


    }

    private void playGateSound() {
        audioSource.clip = gateAudioClip;
        audioSource.Play();
    }
}
