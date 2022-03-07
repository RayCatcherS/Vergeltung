using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{

    const float NEGATIVE_ROTATION_CLAMP = -1f;
    const float POSITIVE_ROTATION_CLAMP = 1f;
    const float NEGATIVE_MOVEMENT_CLAMP = -1f;
    const float POSITIVE_MOVEMENT_CLAMP = 1f;

    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float rotationSpeed = 20f;
    public Transform characterModelrotationTransform;
    public Transform aimTransform;

    [SerializeField] private Vector3 _rotationTarget;


    private Animator animator; //animator del character

    void Awake() {
        animator = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start() {
        
    }


    /// <summary>
    /// Metodo con cui viene applicato uno spostamento sul character
    /// </summary>
    /// <param name="_2Dmove">Coppia di valori che rappresenta i valori
    /// in input del movimento del character. I valori(x, y) sono Clampati tra -1 e 1</param>
    public void moveCharacter(Vector2 _2Dmove) {
        Vector3 _movement; // vettore movimento character
        Vector3 _movementAnimationVelocity; // velocity input analogico

        // clamp dei valori passati 
        _movementAnimationVelocity = _movement = new Vector3(
            Mathf.Clamp(_2Dmove.x, NEGATIVE_MOVEMENT_CLAMP, POSITIVE_MOVEMENT_CLAMP),
            0f,
            Mathf.Clamp(_2Dmove.y, NEGATIVE_MOVEMENT_CLAMP, POSITIVE_MOVEMENT_CLAMP));

        
        // setta traslazione utilizzando il deltaTime(differisce dalla frequenza dei fotogrammi)
        // evitando che il movimento del character dipenda dai fotogrammi
        if (_movement.magnitude > 0) {
            _movement = _movement * movementSpeed * Time.deltaTime;

            // applica movimento al character
            transform.Translate(_movement, Space.World);
        }



        // setta valori animazione partendo dal _movementAnimationVelocity
        float velocityX = Vector3.Dot(_movementAnimationVelocity, transform.right);
        float velocityZ = Vector3.Dot(_movementAnimationVelocity, transform.forward);

        animator.SetFloat("VelocityX", velocityX, 0.1f, Time.deltaTime);
        animator.SetFloat("VelocityZ", velocityZ, 0.1f, Time.deltaTime);
    }


    /// <summary>
    /// Metodo con cui viene applicato uno spostamento sul character
    /// </summary>
    /// <param name="_2Drotate">Coppia di valori che rappresenta i valori
    /// in input della rotazione del character. I valori(x, y) sono Clampati tra -1 e 1</param>
    public void rotateCharacter(Vector2 _2Drotate) {
        Vector3 rotationTarget; // vettore rotazione target

        // clamp dei valori passati 
        rotationTarget = new Vector3(
            Mathf.Clamp(_2Drotate.x, NEGATIVE_ROTATION_CLAMP, POSITIVE_ROTATION_CLAMP),
            Mathf.Clamp(_2Drotate.y, NEGATIVE_ROTATION_CLAMP, POSITIVE_ROTATION_CLAMP),
            0f);

        if (rotationTarget.magnitude > 0) {
            // ruota player partendo dalle coordinate rotazione (utilizzando la funzione lerp)
            aimTransform.rotation = Quaternion.Lerp(aimTransform.rotation, Quaternion.Euler(0, 360 - (Mathf.Atan2(rotationTarget.x, rotationTarget.y) * Mathf.Rad2Deg * -1), 0), Time.deltaTime * rotationSpeed);
        }
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 5);
    }
}
