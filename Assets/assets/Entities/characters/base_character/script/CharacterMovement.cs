using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    private Animator animator; //animator del character


    const float NEGATIVE_ROTATION_CLAMP = -1f;
    const float POSITIVE_ROTATION_CLAMP = 1f;
    const float NEGATIVE_MOVEMENT_CLAMP = -1f;
    const float POSITIVE_MOVEMENT_CLAMP = 1f;

    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float rotationSpeed = 20f;
    public GameObject characterModel;
    public Transform aimTransform;


    [SerializeField] private Vector3 rotationAimInput;
    [SerializeField] private Vector3 rotationAimTarget;
    [SerializeField] private Vector2 characterModelRotation;


    public Vector3 getRotationAimInput { get { return rotationAimInput; } }
    public Vector3 getRotationAimTarget { get { return rotationAimTarget; } }
    public Vector3 getCharacterModelRotation { get { return characterModelRotation; } }


    void Awake() {
        animator = characterModel.gameObject.GetComponent<Animator>();
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
        float velocityX = Vector3.Dot(_movementAnimationVelocity, characterModel.transform.right);
        float velocityZ = Vector3.Dot(_movementAnimationVelocity, characterModel.transform.forward);

        animator.SetFloat("VelocityX", velocityX, 0.1f, Time.deltaTime);
        animator.SetFloat("VelocityZ", velocityZ, 0.1f, Time.deltaTime);
    }


    /// <summary>
    /// Metodo con cui viene applicato uno spostamento sul character
    /// </summary>
    /// <param name="_2Drotate">Coppia di valori che rappresenta i valori
    /// in input della rotazione dell'aim del character. I valori(x, y) sono Clampati tra -1 e 1</param>
    /// vengono inoltre calcolati i range per ruotare l'intero character
    public void rotateCharacter(Vector2 _2Drotate) {
        Vector3 rotationAimTargetInput; // vettore rotazione target

        // clamp dei valori passati 
        rotationAimTargetInput = new Vector3(
            Mathf.Clamp(_2Drotate.x, NEGATIVE_ROTATION_CLAMP, POSITIVE_ROTATION_CLAMP),
            Mathf.Clamp(_2Drotate.y, NEGATIVE_ROTATION_CLAMP, POSITIVE_ROTATION_CLAMP),
            0f);

        rotationAimInput = rotationAimTargetInput;


        rotationAimTarget = aimTransform.transform.rotation * Vector3.forward;
        if (rotationAimTargetInput.magnitude > 0) {
            // ruota aim character partendo dalle coordinate rotazione (utilizzando la funzione lerp)
            aimTransform.rotation = Quaternion.Lerp(aimTransform.rotation, Quaternion.Euler(0, 360 - (Mathf.Atan2(rotationAimTargetInput.x, rotationAimTargetInput.y) * Mathf.Rad2Deg * -1), 0), Time.deltaTime * rotationSpeed);

            rotationAimTarget = aimTransform.transform.rotation * Vector3.forward;

            Vector2 characterRotation = new Vector2();

            if (
                rotationAimTarget.x > -0.707f && rotationAimTarget.z > 0.707f &&
                rotationAimTarget.x < 0.707f && rotationAimTarget.z > 0.707f
            ) {
                characterRotation.x = 0;
                characterRotation.y = 1;

                characterModelRotation = characterRotation;
                characterModel.transform.rotation =
                    Quaternion.Lerp(characterModel.transform.rotation,
                    Quaternion.Euler(0, 360 - (Mathf.Atan2(characterRotation.x, characterRotation.y) * Mathf.Rad2Deg * -1), 0),
                    Time.deltaTime * rotationSpeed);
            } else if (
                rotationAimTarget.x > 0.707f && rotationAimTarget.z < 0.707f &&
                rotationAimTarget.x > 0.707f && rotationAimTarget.z > -0.707f
            ) {
                characterRotation.x = 1f;
                characterRotation.y = 0f;

                characterModelRotation = characterRotation;
                characterModel.transform.rotation =
                    Quaternion.Lerp(characterModel.transform.rotation,
                    Quaternion.Euler(0, 360 - (Mathf.Atan2(characterRotation.x, characterRotation.y) * Mathf.Rad2Deg * -1), 0),
                    Time.deltaTime * rotationSpeed);
            } else if (
                rotationAimTarget.x < 0.707f && rotationAimTarget.z < -0.707f &&
                rotationAimTarget.x > -0.707f && rotationAimTarget.z < -0.707f
            ) {
                characterRotation.x = 0f;
                characterRotation.y = -1f;

                characterModelRotation = characterRotation;
                characterModel.transform.rotation =
                    Quaternion.Lerp(characterModel.transform.rotation,
                    Quaternion.Euler(0, 360 - (Mathf.Atan2(characterRotation.x, characterRotation.y) * Mathf.Rad2Deg * -1), 0),
                    Time.deltaTime * rotationSpeed);
            } else if (
                rotationAimTarget.x < -0.707f && rotationAimTarget.z > -0.707f &&
                rotationAimTarget.x < -0.707f && rotationAimTarget.z < 0.707f
            ) {
                characterRotation.x = -1f;
                characterRotation.y = 0f;

                characterModelRotation = characterRotation;
                characterModel.transform.rotation =
                    Quaternion.Lerp(characterModel.transform.rotation,
                    Quaternion.Euler(0, 360 - (Mathf.Atan2(characterRotation.x, characterRotation.y) * Mathf.Rad2Deg * -1), 0),
                    Time.deltaTime * rotationSpeed);
            }
        }

        




        /*if(rotationAimTarget.x == -1 && rotationAimTarget.y == 0) { // (-1, 0)

            characterRotation.x = -1f;
            characterRotation.y = 0f;

            characterModelrotation = characterRotation;
            characterModelrotationTransform.rotation = Quaternion.Lerp(characterModelrotationTransform.rotation, Quaternion.Euler(0, 360 - (Mathf.Atan2(characterRotation.x, characterRotation.y) * Mathf.Rad2Deg * -1), 0), Time.deltaTime * rotationSpeed);
        } else if (// 1 quadrante, prima porzione del quadrante 
            rotationAimTarget.x < -0.707f && rotationAimTarget.y < 0.707f &&
            rotationAimTarget.x >= -1 && rotationAimTarget.y > 0
         ) {

            characterRotation.x = -0.866f;
            characterRotation.y = 0.5f;

            characterModelrotation = characterRotation;
            characterModelrotationTransform.rotation = Quaternion.Lerp(characterModelrotationTransform.rotation, Quaternion.Euler(0, 360 - (Mathf.Atan2(characterRotation.x, characterRotation.y) * Mathf.Rad2Deg * -1), 0), Time.deltaTime * rotationSpeed);
        } else if (// 1 quadrante, seconda porzione del quadrante 
            rotationAimTarget.x < 0f && rotationAimTarget.y <= 1f &&
            rotationAimTarget.x > -0.707f && rotationAimTarget.y > 0.707f
        ) {

            characterRotation.x = -0.5f;
            characterRotation.y = 0.66f;

            characterModelrotation = characterRotation;
            characterModelrotationTransform.rotation = Quaternion.Lerp(characterModelrotationTransform.rotation, Quaternion.Euler(0, 360 - (Mathf.Atan2(characterRotation.x, characterRotation.y) * Mathf.Rad2Deg * -1), 0), Time.deltaTime * rotationSpeed);
        } else if(rotationAimTarget.x == 0 && rotationAimTarget.y == 1) { // (0,1)
            
            characterRotation.x = 0f;
            characterRotation.y = 1f;

            characterModelrotation = characterRotation;
            characterModelrotationTransform.rotation = Quaternion.Lerp(characterModelrotationTransform.rotation, Quaternion.Euler(0, 360 - (Mathf.Atan2(characterRotation.x, characterRotation.y) * Mathf.Rad2Deg * -1), 0), Time.deltaTime * rotationSpeed);
        } else if(
            rotationAimTarget.x > 0 && rotationAimTarget.y <= 1 &&
            rotationAimTarget.x < 0.707f && rotationAimTarget.y > 0.707f
        ) {// 2 quadrante, prima porzione del quadrante

            characterRotation.x = 0.5f;
            characterRotation.y = 0.866f;

            characterModelrotation = characterRotation;
            characterModelrotationTransform.rotation = Quaternion.Lerp(characterModelrotationTransform.rotation, Quaternion.Euler(0, 360 - (Mathf.Atan2(characterRotation.x, characterRotation.y) * Mathf.Rad2Deg * -1), 0), Time.deltaTime * rotationSpeed);
        } else if(
            rotationAimTarget.x > 0.707f && rotationAimTarget.y < 0.707f &&
            rotationAimTarget.x <= 1f && rotationAimTarget.y > 0
        ) {// 2 quadrante, seconda porzione del quadrante
            characterRotation.x = 0.866f;
            characterRotation.y = 0.5f;

            characterModelrotation = characterRotation;
            characterModelrotationTransform.rotation = Quaternion.Lerp(characterModelrotationTransform.rotation, Quaternion.Euler(0, 360 - (Mathf.Atan2(characterRotation.x, characterRotation.y) * Mathf.Rad2Deg * -1), 0), Time.deltaTime * rotationSpeed);
        } else if(rotationAimTarget.x == 1 && rotationAimTarget.y == 0) {
            characterRotation.x = 1;
            characterRotation.y = 0f;

            characterModelrotation = characterRotation;
            characterModelrotationTransform.rotation = Quaternion.Lerp(characterModelrotationTransform.rotation, Quaternion.Euler(0, 360 - (Mathf.Atan2(characterRotation.x, characterRotation.y) * Mathf.Rad2Deg * -1), 0), Time.deltaTime * rotationSpeed);
        }*/


    }


    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 5);
    }
}

    
    
