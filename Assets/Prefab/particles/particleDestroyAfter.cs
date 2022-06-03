using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class particleDestroyAfter : MonoBehaviour
{

    [SerializeField] private float destroyTime = 0.5f;
    void Start()
    {
        StartCoroutine(startCountdown());
    }

    IEnumerator startCountdown() {
        yield return new WaitForSeconds(destroyTime);
        Destroy(gameObject);
    }
}
