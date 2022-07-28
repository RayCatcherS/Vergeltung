using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootSteps : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip[] clips;
    [SerializeField] private CharacterManager characterManager;
    private void step() {

        if(!characterManager.isPlayer) {
            source.clip = clips[0];
            source.pitch = 1;
            source.Play();
        } else {

            if(characterManager.isRunning) {
                source.clip = clips[0];
                source.pitch = 1;
                source.Play();
            }
        }
        
    }

    private void fastStep() {

        if(!characterManager.isPlayer) {
            source.clip = clips[0];
            source.pitch = 1.25f;
            source.Play();
        } else {

            if(characterManager.isRunning) {
                source.clip = clips[0];
                source.pitch = 1.25f;
                source.Play();
            }
        }
        
    }
}
