using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    // ray cast const
    private const int COLLISION_RAYCAST_DISTANCE = 2;
    private const int ALL_LAYERS = -1;
    private const int CHARACTER_LAYER = 7;

    [SerializeField] private float moveSpeed = 100;
    [SerializeField] private float deadTime = 3;
    [SerializeField] private Vector3 _bulletDirection;

    [SerializeField] private int bulletDamage = 20;

    void Start()
    {
        StartCoroutine(startBulletDeadTime(deadTime));
    }

    public void setupBullet(Vector3 bulletDirection) {
        _bulletDirection = bulletDirection;
    }

    void FixedUpdate() {
        gameObject.GetComponent<Rigidbody>().MovePosition(transform.position + _bulletDirection * moveSpeed * Time.fixedDeltaTime);

        // raycaster bullet collision
        RaycastHit hit;
        Ray ray = new Ray(gameObject.transform.position, (_bulletDirection));

        if (Physics.Raycast(ray, out hit, COLLISION_RAYCAST_DISTANCE, ALL_LAYERS, QueryTriggerInteraction.Ignore)) {


            
            Debug.DrawLine(gameObject.transform.position, hit.point, Color.cyan);
            Destroy(gameObject);


            if(hit.transform.gameObject.layer == CHARACTER_LAYER) {

                characterCollision(hit.transform.gameObject.GetComponent<CharacterManager>());
            }


        } else {
            Debug.DrawLine(gameObject.transform.position, gameObject.transform.position + (_bulletDirection) * COLLISION_RAYCAST_DISTANCE, Color.cyan);
        }
    }

    private IEnumerator startBulletDeadTime(float t) {
        yield return new WaitForSeconds(t);
        Destroy(gameObject);
    }

    private void characterCollision(CharacterManager character) {
        character.applyCharacterDamage(bulletDamage, Vector3.zero);
    }
}
