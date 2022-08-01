using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider healthBar;

    void Start() {
        
    }

    public void setValue(float valueBar, float maxValueBar) {

        healthBar.maxValue = maxValueBar / maxValueBar;
        healthBar.value = valueBar / maxValueBar;
    }
}
