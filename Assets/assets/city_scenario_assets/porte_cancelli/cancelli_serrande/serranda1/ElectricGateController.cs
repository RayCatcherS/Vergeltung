using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricGateController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Animator gateAnimator;
    [SerializeField] private Animator ligthAnim;
    [SerializeField] private AudioSource audioSource;


    [Header("Conf")]
    [SerializeField] private bool _gateClosed = true;
    [SerializeField] private float gateClosingTime = 7;
    [SerializeField] private float busyGateTimer = 2; // tempo per cui il cancello resta occupato(usato per evitare interazioni durante l'animazione
    [SerializeField] private bool busy = false;

    [SerializeField] private bool _gateAutomaticClose = true;
    [SerializeField] private bool _gateCanBeOpenByPowerOff = true;





    [Header("Asset Refs")]
    [SerializeField] private AudioClip gateAudioClip;

    public bool gateClosed {
        get { return _gateClosed; }
    }

    public void openGateByPowerOff(int customClosingTime = -1) {

        if(_gateCanBeOpenByPowerOff) {

            if(customClosingTime != -1) {

                openGate(customClosingTime);
            } else {

                openGate();
            }
            
        }
        
    }

    /// <summary>
    /// Di default -1 significa che il gate si richiuderà dopo [gateClosingTime]
    /// altrimenti equivale alla durata prima della chiusura
    /// </summary>
    /// <param name="customClosingTime"></param>
    public void openGate(int customClosingTime = -1) {
        playGateSound();

        // resetta stato animazioni cancello e coroutine penzolanti
        StopAllCoroutines();
        gateAnimator.ResetTrigger("openDirection1");
        gateAnimator.ResetTrigger("close");



        _gateClosed = false;
        StartCoroutine(gateLightOn());

        if(_gateAutomaticClose) {

            if(customClosingTime != -1) {
                StartCoroutine(gateClosingTimeOut(customClosingTime));
            } else {
                StartCoroutine(gateClosingTimeOut());
            }
            
        }
        
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


    IEnumerator gateClosingTimeOut(int customClosingTime = -1) {

        if(customClosingTime != -1) {

            yield return new WaitForSeconds(customClosingTime);
        } else {

            yield return new WaitForSeconds(gateClosingTime);
        }
        

        if (!gateClosed) {
            closeGate(); // chiudi cancello
        }


    }

    private void playGateSound() {
        audioSource.clip = gateAudioClip;
        audioSource.Play();
    }
}
