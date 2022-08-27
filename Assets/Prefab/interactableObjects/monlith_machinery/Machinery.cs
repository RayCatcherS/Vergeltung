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
    [SerializeField] private TargetIconManager targetIconManager;

    [Header("Manager Refs")]
    [SerializeField] private GameModeController gameModeController;

    [Header("Enviroment Refs")]
    [SerializeField] private List<LightSourcesScript> machineryLights = new List<LightSourcesScript>();


    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip machineryClip;
    [SerializeField] private AudioClip machineryOverloadClip;

    [Header("Prefab refs")]
    [SerializeField] private GameObject poweAmmoPrefab;



    private void Start() {

        // abilita target icon
        targetIconManager.enableTargetUI();

        audioSource.clip = machineryClip;
        audioSource.Play();
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
            updateMachinerySFXPitch();


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

                // pitch mod
                updateMachinerySFXPitch();

                if (machineryHealth > 100) {
                    
                    machineryHealth = 100;
                    refreshUI();

                    // pitch mod
                    updateMachinerySFXPitch();

                    break;
                }
                
            }

        }


        deloadActive = false;

    }

    void updateMachinerySFXPitch() {
        
        float value = Mathf.Abs(machineryHealth - 100);
        value = value / 100;

        audioSource.pitch = 1 + value;
    }

    void machineryOffSFX() {
        audioSource.pitch = 1;
        audioSource.loop = false;

        audioSource.clip = machineryOverloadClip;
        audioSource.pitch = DEFAULT_PITCH_VALUE;
        audioSource.Play();
    }

    /// <summary>
    /// machinery overload
    /// </summary>
    private void generatorOverload() {
        _machinerEnabled = false;

        sparksEffect();

        openMachineryAnim();

        spawnPowerAmmo();

        machineryOffSFX();

        // slider material
        machinerySliderFill.material = disabledMachinertSliderFill;

        // disable console
        machineryConsole.isInteractableDisabled = true;

        // disabilita machinery lights
        foreach(LightSourcesScript machineryLight in machineryLights) {
            
            machineryLight.setLightOff();
            machineryLight.lightDisabled = true;
        }

        // disabilita target icon
        targetIconManager.disableTargetUI();

        // invia evento
        sendGameGoalEvent();

        // avvia vibrazione pad
        gameModeController.gameObject.GetComponent<GamePadVibrationController>().sendImpulse(0.7f, 2);
    }

    private void sendGameGoalEvent() {
        const string gameGoalName = "Disable the monolith machines";

        gameModeController
            .updateGameGoalsStatus(gameGoalName, GameGoal.GameGoalOperation.addGoal);
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
        GameObject ammo;
        for (int i = 0; i < ammunitionReleased; i++) {
            ammo = Instantiate(poweAmmoPrefab, spawnTransform.position, Quaternion.identity);
        }
        
    }
    private void refreshUI() {
        machineryUISlider.value = Mathf.Abs(machineryHealth - 100);
    }

}
