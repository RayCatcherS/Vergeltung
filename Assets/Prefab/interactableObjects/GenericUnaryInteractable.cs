using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericUnaryInteractable : Interactable {

    [Header("Config")]
    [SerializeField] string genericEventName = "ACTION";
    [SerializeField] UnityEventCharacter genericEvent = new UnityEventCharacter();
    [SerializeField] private bool repetableInteraction = false;
    private bool unRepetableInteractionStateActive = true;
    [SerializeField] private bool _isInteractableDisabled = false;

    [SerializeField] private bool _interactionEnabledOnlyForPlayer = false;
    public bool isInteractableDisabled {
        set {

            if(value) {
                List<Interaction> interactions = new List<Interaction>();
                rebuildInteractableMeshEffect(interactions);
                unFocusInteractableOutline();
                interactableMeshEffectSetEnebled(false);
            }
            _isInteractableDisabled = value;
        }
    }


    [Header("Asset Refs")]
    [SerializeField] private AudioClip interactAudioClip;

    [Header("Refs")]
    [SerializeField] private AudioSource audioSource;

    public override void Start() {
        initInteractable();

        genericEvent.AddListener((CharacterManager c) => {

            playSounds();
        });
    }

    public override Interaction getMainInteraction() {

        

        if (!repetableInteraction && !_isInteractableDisabled) {

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

    public override List<Interaction> getInteractions(CharacterManager character = null) {

        List<Interaction> eventRes = new List<Interaction>();




        if(!_isInteractableDisabled) {

            if(_interactionEnabledOnlyForPlayer) {
                if(character.chracterRole == Role.Player) {

                    if(!repetableInteraction) {

                        if(unRepetableInteractionStateActive) {
                            
                            genericEvent.AddListener((CharacterManager c) => {

                                interactableMeshEffectSetEnebled(false);
                                unRepetableInteractionStateActive = false;
                            });
                            eventRes.Add(new Interaction(genericEvent, genericEventName, this));
                        }

                    } else {

                        eventRes.Add(new Interaction(genericEvent, genericEventName, this));
                    }
                }
            } else {
                if(!repetableInteraction) {

                    if(unRepetableInteractionStateActive) {
                        genericEvent.AddListener((CharacterManager c) => {

                            interactableMeshEffectSetEnebled(false);
                            unRepetableInteractionStateActive = false;
                        });
                        eventRes.Add(new Interaction(genericEvent, genericEventName, this));
                    }

                } else {

                    eventRes.Add(new Interaction(genericEvent, genericEventName, this));
                }
            }

            
        }
        


        return eventRes;
    }

    private void playSounds() {
        audioSource.clip = interactAudioClip;
        audioSource.Play();
    }

    public void disableInteractable() {
        _isInteractableDisabled = true;
    }
}
