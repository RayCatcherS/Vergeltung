using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlIconManager : MonoBehaviour {
    
    private const string STACKED_CONTROLLED = "controlIconOnStackedControlled";
    private const string STACKED_NOT_CONTROLLED = "controlIconOnStackedNotControlled";
    private const string NOT_CONTROLLED = "controlIconOff";


    [SerializeField] private Animator iconAnimator;
    public Transform characterControlIconTransfom {
        get {
            return iconAnimator.gameObject.transform;
        }
    }

    public void setAsUnstackedNotControlled() {
        iconAnimator.SetTrigger(NOT_CONTROLLED);
    }

    public void setAsStackedNotControlled() {
        iconAnimator.SetTrigger(STACKED_NOT_CONTROLLED);
    }

    public void setAsStackedControlled() {
        iconAnimator.SetTrigger(STACKED_CONTROLLED);
    }

    public void setAnimatorMultiplierSpeed(int m) {
        iconAnimator.speed = 1 * m;
    }
}
