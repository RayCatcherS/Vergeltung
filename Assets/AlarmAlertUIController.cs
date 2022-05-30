using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlarmAlertUIController : MonoBehaviour
{
    private const string ALARM_ON_TRIGGER = "alarmOn";
    private const string ALARM_OFF_TRIGGER = "alarmOff";
    private const string ALARM_EXIT = "exit";


    [Header("Ref")]
    [SerializeField] private Animator wantedAnimator;
    [SerializeField] private Animator visiblyArmedAnimator;
    [SerializeField] private Animator prohibitedAreaAnimator;


    public void wantedAlarmOn() {
        wantedAnimator.ResetTrigger(ALARM_ON_TRIGGER);
        wantedAnimator.ResetTrigger(ALARM_OFF_TRIGGER);
        wantedAnimator.ResetTrigger(ALARM_EXIT);

        wantedAnimator.gameObject.SetActive(true);
        wantedAnimator.SetTrigger(ALARM_ON_TRIGGER);
    }
    public void wantedAlarmOff() {
        wantedAnimator.ResetTrigger(ALARM_ON_TRIGGER);
        wantedAnimator.ResetTrigger(ALARM_OFF_TRIGGER);
        wantedAnimator.ResetTrigger(ALARM_EXIT);

        wantedAnimator.SetTrigger(ALARM_OFF_TRIGGER);
        wantedAnimator.SetTrigger(ALARM_EXIT);
    }



    public void visiblyArmedAlarmOn() {
        visiblyArmedAnimator.ResetTrigger(ALARM_ON_TRIGGER);
        visiblyArmedAnimator.ResetTrigger(ALARM_OFF_TRIGGER);
        visiblyArmedAnimator.ResetTrigger(ALARM_EXIT);

        visiblyArmedAnimator.gameObject.SetActive(true);
        visiblyArmedAnimator.SetTrigger(ALARM_ON_TRIGGER);
    }
    public void visiblyArmedAlarmOff() {
        visiblyArmedAnimator.ResetTrigger(ALARM_ON_TRIGGER);
        visiblyArmedAnimator.ResetTrigger(ALARM_OFF_TRIGGER);
        visiblyArmedAnimator.ResetTrigger(ALARM_EXIT);

        visiblyArmedAnimator.SetTrigger(ALARM_OFF_TRIGGER);
        visiblyArmedAnimator.SetTrigger(ALARM_EXIT);
    }



    public void prohibitedAreaAlarmOn() {
        prohibitedAreaAnimator.ResetTrigger(ALARM_ON_TRIGGER);
        prohibitedAreaAnimator.ResetTrigger(ALARM_OFF_TRIGGER);
        prohibitedAreaAnimator.ResetTrigger(ALARM_EXIT);

        prohibitedAreaAnimator.gameObject.SetActive(true);
        prohibitedAreaAnimator.SetTrigger(ALARM_ON_TRIGGER);
    }
    public void prohibitedAreaAlarmOff() {
        prohibitedAreaAnimator.ResetTrigger(ALARM_ON_TRIGGER);
        prohibitedAreaAnimator.ResetTrigger(ALARM_OFF_TRIGGER);
        prohibitedAreaAnimator.ResetTrigger(ALARM_EXIT);

        prohibitedAreaAnimator.SetTrigger(ALARM_OFF_TRIGGER);
        prohibitedAreaAnimator.SetTrigger(ALARM_EXIT);
    }
}
