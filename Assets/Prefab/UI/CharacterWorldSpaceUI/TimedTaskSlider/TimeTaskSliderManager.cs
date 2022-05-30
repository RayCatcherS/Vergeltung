using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeTaskSliderManager : MonoBehaviour
{
    private const string TASK_ON_ANIMATOR_TRIGGER = "taskOn";
    private const string TASK_OFF_ANIMATOR_TRIGGER = "taskOff";
    private const string TASK_EXIT_ANIMATOR_TRIGGER = "exit";

    [Header("Refereces")]
    [SerializeField] private Animator timeTaskSliderAnimator;
    [SerializeField] private CharacterManager characterManager;
    [SerializeField] private Slider timeTaskSlider;
    [SerializeField] private Text sliderText;
    void Start()
    {
        timeTaskSliderAnimator.gameObject.SetActive(false);
    }

    /// <summary>
    /// attiva e inizializza slider del timer
    /// </summary>
    /// <param name="minValue">Valore di partenza</param>
    /// <param name="maxValue">Valore di arrivo</param>
    public void enableAndInitializeTimerSlider(float minValue, float maxValue, string sliderTitle) {
        timeTaskSliderAnimator.gameObject.SetActive(true);
        timeTaskSliderAnimator.SetTrigger(TASK_ON_ANIMATOR_TRIGGER);

        sliderText.text = sliderTitle;
        timeTaskSlider.minValue = minValue;
        timeTaskSlider.maxValue = maxValue;
        timeTaskSlider.transform.forward = Camera.main.transform.forward;
    }

    public void setSliderValue(float sliderValue) {
        timeTaskSlider.value = sliderValue;
    }

    public void disableTimeSlider() {
        timeTaskSliderAnimator.SetTrigger(TASK_OFF_ANIMATOR_TRIGGER);
        timeTaskSliderAnimator.SetTrigger(TASK_EXIT_ANIMATOR_TRIGGER);
    }
}
