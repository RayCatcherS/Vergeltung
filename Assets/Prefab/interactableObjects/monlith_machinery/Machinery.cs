using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Machinery : MonoBehaviour
{

    private const int BULLET_LAYER = 14;

    [Header("Machinery Config")]
    [SerializeField] private int machineryHealth = 100;
    [SerializeField] private int machineryLoad = 15;
    [SerializeField] private int ammunitionReleased = 1;
    private bool _machinerEnabled = true;

    [Header("Refs")]
    [SerializeField] private Slider machineryUISlider;
    [SerializeField] private ParticleSystem sparks;

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
        }
    }

    /// <summary>
    /// machinery overload
    /// </summary>
    private void generatorOverload() {
        _machinerEnabled = false;

        sparksEffect();
    }

    private void sparksEffect() {

        sparks.gameObject.SetActive(true);
    }
}
