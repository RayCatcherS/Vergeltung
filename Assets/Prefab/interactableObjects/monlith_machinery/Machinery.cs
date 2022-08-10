using System.Collections;
using System.Collections.Generic;
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


    [Header("Refs")]
    [SerializeField] private Slider machineryUISlider;
    [SerializeField] private ParticleSystem sparks;
    [SerializeField] private Animator machineryAnimator;
    [SerializeField] private Transform spawnTransform;


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


            machineryUISlider.value = Mathf.Abs(machineryHealth - 100);


            // pitch mod
            audioSource.pitch = audioSource.pitch + 0.2f;
        }
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
}
