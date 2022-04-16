using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private const int INTERACTALBLE_LAYER = 3;

    [SerializeField] private float moveSpeed = 100;
    [SerializeField] private float deadTime = 3;
    [SerializeField] private Vector3 _bulletDirection;

    void Start()
    {
        StartCoroutine(startBulletDeadTime(deadTime));
    }

    public void setupBullet(Vector3 bulletDirection) {
        _bulletDirection = bulletDirection;
    }

    void FixedUpdate()
    {
        gameObject.GetComponent<Rigidbody>().MovePosition(transform.position + _bulletDirection * moveSpeed * Time.fixedDeltaTime);
    }

    private IEnumerator startBulletDeadTime(float t) {
        yield return new WaitForSeconds(t);
        Destroy(gameObject);
    }


    private void OnTriggerEnter(Collider other) {
        Debug.Log(other.gameObject.layer);
        if(other.gameObject.layer == INTERACTALBLE_LAYER) {
            
        } else {
            Destroy(gameObject);
        }
    }
}
