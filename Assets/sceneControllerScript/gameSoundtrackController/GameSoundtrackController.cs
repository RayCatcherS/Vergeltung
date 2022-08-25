using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterBehaviourSoundtrackState {
    Unalert,
    Suspicious,
    Hostility,
    noSoundtrack
}

public class GameSoundtrackController : MonoBehaviour
{


    [Header("Refs")]
    [SerializeField] private AudioSource unalertAS;
    [SerializeField] private AudioSource suspiciousAS;
    [SerializeField] private AudioSource hostilityAS;

    [SerializeField] private Animator soundTrackAnimator;

    
    private CharacterBehaviourSoundtrackState _soundTrackState = CharacterBehaviourSoundtrackState.Unalert;
    public CharacterBehaviourSoundtrackState soundTrackState {
        get { return _soundTrackState; }
    }

    void Start()
    {
        unalertAS.Play();
        suspiciousAS.Play();
        hostilityAS.Play();
    }

    public void setSoundTrackState(CharacterBehaviourSoundtrackState state) {

        if(state == CharacterBehaviourSoundtrackState.Unalert) {

            if(_soundTrackState != CharacterBehaviourSoundtrackState.Unalert) {
                soundTrackAnimator.ResetTrigger("unalert");
                soundTrackAnimator.ResetTrigger("suspicious");
                soundTrackAnimator.ResetTrigger("hostility");
                soundTrackAnimator.ResetTrigger("noSoundtrack");

                soundTrackAnimator.SetTrigger("unalert");

            }
            
        } else if(state == CharacterBehaviourSoundtrackState.Suspicious) {

            if(_soundTrackState != CharacterBehaviourSoundtrackState.Suspicious) {
                soundTrackAnimator.ResetTrigger("unalert");
                soundTrackAnimator.ResetTrigger("suspicious");
                soundTrackAnimator.ResetTrigger("hostility");
                soundTrackAnimator.ResetTrigger("noSoundtrack");

                soundTrackAnimator.SetTrigger("suspicious");
            }
            
        } else if(state == CharacterBehaviourSoundtrackState.Hostility) {

            if(_soundTrackState != CharacterBehaviourSoundtrackState.Hostility) {
                soundTrackAnimator.ResetTrigger("unalert");
                soundTrackAnimator.ResetTrigger("suspicious");
                soundTrackAnimator.ResetTrigger("hostility");
                soundTrackAnimator.ResetTrigger("noSoundtrack");

                soundTrackAnimator.SetTrigger("hostility");
            }
        } else if(state == CharacterBehaviourSoundtrackState.noSoundtrack) {

            if(_soundTrackState != CharacterBehaviourSoundtrackState.noSoundtrack) {
                soundTrackAnimator.ResetTrigger("unalert");
                soundTrackAnimator.ResetTrigger("suspicious");
                soundTrackAnimator.ResetTrigger("hostility");
                soundTrackAnimator.ResetTrigger("noSoundtrack");

                soundTrackAnimator.SetTrigger("noSoundtrack");
            }
        }

        _soundTrackState = state;
    }
}
