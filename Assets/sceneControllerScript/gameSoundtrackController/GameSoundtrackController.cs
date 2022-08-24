using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSoundtrackController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private AudioSource unalertAS;
    [SerializeField] private AudioSource suspiciousAS;
    [SerializeField] private AudioSource hostilityAS;

    [SerializeField] private Animator soundTrackAnimator;

    
    private CharacterBehaviourState _soundTrackState = CharacterBehaviourState.Unalert;
    public CharacterBehaviourState soundTrackState {
        get { return _soundTrackState; }
    }

    void Start()
    {
        unalertAS.Play();
        suspiciousAS.Play();
        hostilityAS.Play();
    }

    public void setSoundTrackState(CharacterBehaviourState state) {

        if(state == CharacterBehaviourState.Unalert) {

            if(_soundTrackState != CharacterBehaviourState.Unalert) {
                soundTrackAnimator.ResetTrigger("unalert");
                soundTrackAnimator.ResetTrigger("suspicious");
                soundTrackAnimator.ResetTrigger("hostility");

                soundTrackAnimator.SetTrigger("unalert");

            }
            
        } else if(state == CharacterBehaviourState.Suspicious) {

            if(_soundTrackState != CharacterBehaviourState.Suspicious) {
                soundTrackAnimator.ResetTrigger("unalert");
                soundTrackAnimator.ResetTrigger("suspicious");
                soundTrackAnimator.ResetTrigger("hostility");

                soundTrackAnimator.SetTrigger("suspicious");
            }
            
        } else if(state == CharacterBehaviourState.Hostility) {

            if(_soundTrackState != CharacterBehaviourState.Hostility) {
                soundTrackAnimator.ResetTrigger("unalert");
                soundTrackAnimator.ResetTrigger("suspicious");
                soundTrackAnimator.ResetTrigger("hostility");

                soundTrackAnimator.SetTrigger("hostility");
            }
        }

        _soundTrackState = state;
    }
}
