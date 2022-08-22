using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSoundtrackController : MonoBehaviour
{
    [SerializeField] private AudioSource unalertAS;
    [SerializeField] private AudioSource suspiciousAS;
    [SerializeField] private AudioSource hostilityAS;

    void Start()
    {
        unalertAS.Play();
        suspiciousAS.Play();
        hostilityAS.Play();
    }
}
