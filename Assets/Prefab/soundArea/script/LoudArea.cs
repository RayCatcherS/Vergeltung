using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public enum LoudAreaIntensity {
    nothing = 0,
    low = 2,
    medium = 10,
    high = 20
}

public class LoudArea : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private AudioSource _audioSource;

    private float _areaRadius = 10;

    public void initLoudArea(LoudAreaIntensity intensity, AudioClip clip = null) {

        if(clip != null) {
            _audioSource.clip = clip;
        }

        _areaRadius = (float)intensity;
    }

    public void startLoudArea() {

        if(_audioSource.clip != null) {
            _audioSource.Play();
        }
    }


#if UNITY_EDITOR
    void OnDrawGizmosSelected() {
        Handles.color = Color.red;
        Handles.DrawWireDisc(gameObject.transform.position, Vector3.up, _areaRadius); //debug radius
    }
#endif
}
