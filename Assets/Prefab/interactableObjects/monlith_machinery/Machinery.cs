using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class Machinery : MonoBehaviour
{
    private const float DEFAULT_PITCH_VALUE = 1;
    private const int BULLET_LAYER = 14;

    [Header("Machinery Config")]
    [SerializeField] private int machineryHealth = 100;
    [SerializeField] private int machineryLoad = 15;
    [SerializeField] private int ammunitionReleased = 1;
    private bool _machinerEnabled = true;
    [SerializeField] private float deloadFrequency = 0.2f;
    [SerializeField] private int machineryDeloadValue = 2;
    [SerializeField] private bool deloadActive = false;


    [Header("Refs")]
    [SerializeField] private Slider machineryUISlider;
    [SerializeField] private ParticleSystem sparks;
    [SerializeField] private Animator machineryAnimator;
    [SerializeField] private Transform spawnTransform;
    [SerializeField] private Image machinerySliderFill;
    [SerializeField] private Material disabledMachinertSliderFill;
    [SerializeField] private GenericUnaryInteractable machineryConsole;


    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip generatorClip;
    [SerializeField] private AudioClip generatorOverloadClip;

    [Header("Prefab refs")]
    [SerializeField] private GameObject poweAmmoPrefab;



    private void Start() {
        audioSource.clip = generatorClip;
    }


    private void OnCollisionEnter(Collision collision) {


        if(collision.gameObject.layer == BULLET_LAYER) {
            applyMachineryLoad();
        }
    }

    /// <summary>
    /// Applica energia al machinery
    /// </summary>
    public void applyMachineryLoad() {

        if(_machinerEnabled) {

            machineryHealth = machineryHealth - machineryLoad;

            if(machineryHealth <= 0) {

                machineryHealth = 0;
                generatorOverload();
            }


            // refresh UI
            refreshUI();
            

            // pitch mod
            audioSource.pitch = audioSource.pitch + 0.2f;


            // start deload
            if(!deloadActive) {
                deloadLoopAsync();
            }
        }
    }

    private async void deloadLoopAsync() {
        deloadActive = true;

        while(machineryHealth < 100) {

            float endTime = Time.time + deloadFrequency;

            while (Time.time < endTime) {
                await Task.Yield();

                machineryHealth = machineryHealth + machineryDeloadValue;
                refreshUI();

                if (machineryHealth > 100) {
                    
                    machineryHealth = 100;
                    refreshUI();
                    break;
                }
                
            }

        }


        deloadActive = false;

    }

    /// <summary>
    /// machinery overload
    /// </summary>
    private void generatorOverload() {
        _machinerEnabled = false;

        sparksEffect();

        openMachineryAnim();

        spawnPowerAmmo();

        audioSource.clip = generatorOverloadClip;
        audioSource.pitch = DEFAULT_PITCH_VALUE;
        audioSource.Play();

        // slider material
        machinerySliderFill.material = disabledMachinertSliderFill;

        // disable console
        machineryConsole.isInteractableDisabled = false;
    }

    private void sparksEffect() {

        sparks.gameObject.SetActive(true);
    }


    /// <summary>
    /// Animazione apertura machinery
    /// </summary>
    private void openMachineryAnim() {
        machineryAnimator.SetTrigger("open");
    }

    /// <summary>
    /// Animazione chiusura machinery
    /// </summary>
    private void closeMachinery() {
        machineryAnimator.SetTrigger("close");
    }

    private void spawnPowerAmmo() {
        // istanzia nella scena
        GameObject newCharacter;
        for (int i = 0; i < ammunitionReleased; i++) {
            newCharacter = Instantiate(poweAmmoPrefab, spawnTransform.position, Quaternion.identity);
        }
        
    }
    private void refreshUI() {
        machineryUISlider.value = Mathf.Abs(machineryHealth - 100);
    }

}
