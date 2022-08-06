using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericUnaryInteractable : Interactable {

    [Header("Config")]
    [SerializeField] string genericEventName = "ACTION";
    [SerializeField] UnityEventCharacter genericEvent = new UnityEventCharacter();
    [SerializeField] private bool repetableInteraction = false;
    private bool unRepetableInteractionStateActive = true;

    [Header("Asset Refs")]
    [SerializeField] private AudioClip interactAudioClip;

    [Header("Asset Refs")]
    [SerializeField] private AudioSource audioSource;

    public override void Start() {
        initInteractable();

        genericEvent.AddListener((CharacterManager c) => {

            playSounds();
        });
    }

    public override Interaction getMainInteraction() {

        

        if (!repetableInteraction) {

            if (unRepetableInteractionStateActive) {
                unRepetableInteractionStateActive = false;
                return new Interaction(genericEvent, genericEventName, this);
            } else {
                return new Interaction(new UnityEventCharacter(), "", this);
            }

        } else {

            return new Interaction(genericEvent, genericEventName, this);
        }

    }

    public override List<Interaction> getInteractions() {

        List<Interaction> eventRes = new List<Interaction>();

        if (!repetableInteraction) {

            if (unRepetableInteractionStateActive) {
                unRepetableInteractionStateActive = false;
                eventRes.Add(new Interaction(genericEvent, genericEventName, this));
            }

        } else {

            eventRes.Add(new Interaction(genericEvent, genericEventName, this));
        }


        return eventRes;
    }

    private void playSounds() {
        audioSource.clip = interactAudioClip;
        audioSource.Play();
    }
}
