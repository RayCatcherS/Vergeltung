using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    // ray cast const
    
    private const int COLLISION_RAYCAST_DISTANCE = 2;
    private const int ALL_LAYERS = -1;
    private const int CHARACTER_LAYER = 7;
    private const int RAGDOLLBONE_LAYER = 15;
    private const int SHATTERABLEGLASS_LAYER = 17;


    [Header("Bullet configuration")]
    [SerializeField] private float moveSpeed = 100;
    [SerializeField] private float deadTime = 3;
    [SerializeField] private Vector3 _bulletDirection;
    [SerializeField] private int bulletDamage = 20;

    [Header("Bullet particle impact")]
    [SerializeField] private GameObject particleBloodImpact;
    [SerializeField] private GameObject collisionWallImpact;


    [SerializeField] private bool destroyOnImpact = true;
    [SerializeField] private bool isImpact = false;

    [Header("Weapon sounds")]
    [SerializeField] private AudioClip genericSoundCollision;
    [SerializeField] private AudioClip characterSoundCollision;

    [Header("Weapon audio loud object")]
    /// questo oggetto emette suoni e scatenare eventi all'interno della sua area e viene generato quando il proiettile collide
    [SerializeField] private GameObject loudArea;
    [SerializeField] private LoudAreaType loudIntensity;

    void Start()
    {
        StartCoroutine(startBulletDeadTime(deadTime));
    }

    public void setupBullet(Vector3 bulletDirection) {
        _bulletDirection = bulletDirection;
    }

    void FixedUpdate() {

        if(!isImpact) {
            gameObject.GetComponent<Rigidbody>().MovePosition(transform.position + _bulletDirection * moveSpeed * Time.fixedDeltaTime);

            // raycaster bullet collision
            RaycastHit hit;
            Ray ray = new Ray(gameObject.transform.position, (_bulletDirection));

            if (Physics.Raycast(ray, out hit, COLLISION_RAYCAST_DISTANCE, ALL_LAYERS, QueryTriggerInteraction.Ignore)) {

                if (!destroyOnImpact)
                    isImpact = true;


                Debug.DrawLine(gameObject.transform.position, hit.point, Color.cyan);
                if (destroyOnImpact)
                    Destroy(gameObject);


                if (hit.transform.gameObject.layer == CHARACTER_LAYER) {

                    characterCollision(hit.transform.gameObject.GetComponent<CharacterManager>(), hit.point);
                } else if (hit.transform.gameObject.layer == RAGDOLLBONE_LAYER) {

                    ragdollBoneCollision(hit.point);
                } else if (hit.transform.gameObject.layer == SHATTERABLEGLASS_LAYER) {

                    glassCollision(hit);
                    wallCollision(hit.point, hit.normal);
                } else {

                    wallCollision(hit.point, hit.normal);
                }


            } else {
                Debug.DrawLine(gameObject.transform.position, gameObject.transform.position + (_bulletDirection) * COLLISION_RAYCAST_DISTANCE, Color.cyan);
            }
        }
    }

    private IEnumerator startBulletDeadTime(float t) {
        yield return new WaitForSeconds(t);
        Destroy(gameObject);
    }

    private void characterCollision(CharacterManager character, Vector3 collisionPoint) {
        character.applyCharacterDamage(bulletDamage, Vector3.zero);
        Instantiate(particleBloodImpact, collisionPoint, Quaternion.identity);

        // loud area
        GameObject loudGameObject = Instantiate(loudArea, collisionPoint, Quaternion.identity);
        loudGameObject.GetComponent<LoudArea>().initLoudArea(loudIntensity, characterSoundCollision);
        loudGameObject.GetComponent<LoudArea>().startLoudArea();
    }

    private void wallCollision(Vector3 collisionPoint, Vector3 collisionNormal) {
        Instantiate(collisionWallImpact, collisionPoint, Quaternion.LookRotation(collisionNormal));

        // loud area
        GameObject loudGameObject = Instantiate(loudArea, collisionPoint, Quaternion.identity);
        loudGameObject.GetComponent<LoudArea>().initLoudArea(loudIntensity, genericSoundCollision);
        loudGameObject.GetComponent<LoudArea>().startLoudArea();
    }

    private void ragdollBoneCollision(Vector3 collisionPoint) {
        Instantiate(particleBloodImpact, collisionPoint, Quaternion.identity);

        // loud area
        GameObject loudGameObject = Instantiate(loudArea, collisionPoint, Quaternion.identity);
        loudGameObject.GetComponent<LoudArea>().initLoudArea(loudIntensity, characterSoundCollision);
        loudGameObject.GetComponent<LoudArea>().startLoudArea();
    }

    private void glassCollision(RaycastHit hit) {
        hit.transform.gameObject.GetComponent<ShatterableGlass>().shatterGlass();

    }

}
