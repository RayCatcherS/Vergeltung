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
    [SerializeField] private Animator lockPickingAnimator;


    public void potentialWantedAlarmOn() {
        wantedAnimator.gameObject.SetActive(true);


        wantedAnimator.ResetTrigger(ALARM_ON_TRIGGER);
        wantedAnimator.ResetTrigger(ALARM_OFF_TRIGGER);
        wantedAnimator.ResetTrigger(ALARM_EXIT);
        
        wantedAnimator.SetTrigger(ALARM_ON_TRIGGER);
    }
    public void potentialWantedAlarmOff() {

        if (lockPickingAnimator.gameObject.activeSelf) {

            wantedAnimator.ResetTrigger(ALARM_ON_TRIGGER);
            wantedAnimator.ResetTrigger(ALARM_OFF_TRIGGER);
            wantedAnimator.ResetTrigger(ALARM_EXIT);

            wantedAnimator.SetTrigger(ALARM_OFF_TRIGGER);
            wantedAnimator.SetTrigger(ALARM_EXIT);
        }
        
    }



    public void potentialVisiblyArmedAlarmOn() {
        visiblyArmedAnimator.gameObject.SetActive(true);


        visiblyArmedAnimator.ResetTrigger(ALARM_ON_TRIGGER);
        visiblyArmedAnimator.ResetTrigger(ALARM_OFF_TRIGGER);
        visiblyArmedAnimator.ResetTrigger(ALARM_EXIT);
        
        visiblyArmedAnimator.SetTrigger(ALARM_ON_TRIGGER);
    }
    public void potentialVisiblyArmedAlarmOff() {
        if (lockPickingAnimator.gameObject.activeSelf) {

            visiblyArmedAnimator.ResetTrigger(ALARM_ON_TRIGGER);
            visiblyArmedAnimator.ResetTrigger(ALARM_OFF_TRIGGER);
            visiblyArmedAnimator.ResetTrigger(ALARM_EXIT);

            visiblyArmedAnimator.SetTrigger(ALARM_OFF_TRIGGER);
            visiblyArmedAnimator.SetTrigger(ALARM_EXIT);
        }
        
    }



    public void potentialProhibitedAreaAlarmOn() {

        

        prohibitedAreaAnimator.gameObject.SetActive(true);

        prohibitedAreaAnimator.ResetTrigger(ALARM_ON_TRIGGER);
        prohibitedAreaAnimator.ResetTrigger(ALARM_OFF_TRIGGER);
        prohibitedAreaAnimator.ResetTrigger(ALARM_EXIT);

        prohibitedAreaAnimator.SetTrigger(ALARM_ON_TRIGGER);
    }
    public void potentialProhibitedAreaAlarmOff() {

        if (prohibitedAreaAnimator.gameObject.activeSelf) {
            prohibitedAreaAnimator.ResetTrigger(ALARM_ON_TRIGGER);
            prohibitedAreaAnimator.ResetTrigger(ALARM_OFF_TRIGGER);
            prohibitedAreaAnimator.ResetTrigger(ALARM_EXIT);

            prohibitedAreaAnimator.SetTrigger(ALARM_OFF_TRIGGER);
            prohibitedAreaAnimator.SetTrigger(ALARM_EXIT);
        }
        
    }

    public void potentialLockPickingAlarmOn() {
        lockPickingAnimator.gameObject.SetActive(true);


        lockPickingAnimator.ResetTrigger(ALARM_ON_TRIGGER);
        lockPickingAnimator.ResetTrigger(ALARM_OFF_TRIGGER);
        lockPickingAnimator.ResetTrigger(ALARM_EXIT);
        
        lockPickingAnimator.SetTrigger(ALARM_ON_TRIGGER);
    }
    public void potentialLockPickingAlarmOff() {

        if (lockPickingAnimator.gameObject.activeSelf) {

            lockPickingAnimator.ResetTrigger(ALARM_ON_TRIGGER);
            lockPickingAnimator.ResetTrigger(ALARM_OFF_TRIGGER);
            lockPickingAnimator.ResetTrigger(ALARM_EXIT);

            lockPickingAnimator.SetTrigger(ALARM_OFF_TRIGGER);
            lockPickingAnimator.SetTrigger(ALARM_EXIT);
        }
    }
}
