using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class CharacterMovement : MonoBehaviour
{
    public Animator animator; //animator del character
    public CharacterState characterState;
    public Rigidbody characterRigidBody;
    public MultiAimConstraint characterAimConstrant;

    public CapsuleCollider colliderCharacter;
    public CapsuleCollider jumpColliderCharacter;

    const float NEGATIVE_ROTATION_CLAMP = -1f;
    const float POSITIVE_ROTATION_CLAMP = 1f;
    const float NEGATIVE_MOVEMENT_CLAMP = -1f;
    const float POSITIVE_MOVEMENT_CLAMP = 1f;

    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float runMovementSpeed = 10f;
    [SerializeField] private float rotationSpeed = 20f;
    [SerializeField] private float jumpSpeed = 20f;
    public GameObject characterModel;
    public Transform aimTransform;


    [SerializeField] private Vector3 rotationAimInput;
    [SerializeField] private Vector3 rotationAimTarget;
    [SerializeField] private Vector2 characterModelRotation;


    public Vector3 getRotationAimInput { get { return rotationAimInput; } }
    public Vector3 getRotationAimTarget { get { return rotationAimTarget; } }
    public Vector3 getCharacterModelRotation { get { return characterModelRotation; } }

    

    void Awake() {

    }

    // Start is called before the first frame update
    void Start() {
        
    }


    /// <summary>
    /// Metodo con cui viene applicato uno spostamento sul character
    /// </summary>
    /// <param name="_2Dmove">Coppia di valori che rappresenta i valori
    /// in input del movimento del character.</param>
    public void moveCharacter(Vector2 _2Dmove, bool isRun) {
        Vector3 _movement; // vettore movimento character
        Vector3 _movementAnimationVelocity; // velocity input analogico

        // clamp dei valori passati 
        _movementAnimationVelocity = _movement = new Vector3(
            _2Dmove.x,
            0f,
            _2Dmove.y);

        
        if(characterState.grounded && !characterState.jumping) {
            // setta traslazione utilizzando il deltaTime(differisce dalla frequenza dei fotogrammi)
            // evitando che il movimento del character dipenda dai fotogrammi
            if (_movement.magnitude > 0) {

                if (isRun) {
                    _movement = _movement * runMovementSpeed * Time.deltaTime;
                    
                    rotateCharacter(_2Dmove, isRun, false);
                } else {
                    _movement = _movement * movementSpeed * Time.deltaTime;
                }
                characterState.running = isRun;


                // applica movimento al character
                characterRigidBody.position = _movement + characterRigidBody.position;
            }



            // setta valori animazione partendo dal _movementAnimationVelocity
            float velocityX = Vector3.Dot(_movementAnimationVelocity, characterModel.transform.right);
            float velocityZ;
            if (isRun) {
                animator.SetBool("isRunning", isRun);
                animator.SetFloat("VelocityZ", 2, 0.1f, Time.deltaTime);
            } else {
                animator.SetBool("isRunning", isRun);
                velocityZ = Vector3.Dot(_movementAnimationVelocity, characterModel.transform.forward);
                animator.SetFloat("VelocityZ", velocityZ, 0.1f, Time.deltaTime);
            }


            animator.SetFloat("VelocityX", velocityX, 0.1f, Time.deltaTime);
        } else {

            // azzera velocity animazione e flag isRun
            isRun = false;
            animator.SetBool("isRunning", isRun);
            animator.SetFloat("VelocityX", 0);
            animator.SetFloat("VelocityZ", 0);
        }
    }


    /// <summary>
    /// Metodo con cui viene applicato uno spostamento sul character
    /// </summary>
    /// <param name="_2Drotate">Coppia di valori che rappresenta i valori
    /// in input della rotazione dell'aim del character. I valori(x, y)</param>
    /// vengono inoltre calcolati i range delle direzioni dell'aim per ruotare l'intero character
    public void rotateCharacter(Vector2 _2Drotate, bool _isRun, bool _istantRotation) {
        Vector3 rotationAimTargetInput; // vettore rotazione target

        // clamp dei valori passati 
        rotationAimTargetInput = new Vector3(
            _2Drotate.x,
            _2Drotate.y,
            0f);

        rotationAimInput = rotationAimTargetInput;



        rotationAimTarget = aimTransform.transform.rotation * Vector3.forward; // ottieni direzione 

        if (rotationAimTargetInput.magnitude > 0) {

            // ruota aim character partendo dalle coordinate rotazione (utilizzando la funzione lerp)
            if (!_istantRotation) {
                aimTransform.rotation = Quaternion.Lerp(
                    aimTransform.rotation,
                    Quaternion.Euler(0, 360 - (Mathf.Atan2(rotationAimTargetInput.x, rotationAimTargetInput.y) * Mathf.Rad2Deg * -1), 0),
                    Time.deltaTime * rotationSpeed);
            } else {

                aimTransform.rotation = Quaternion.Euler(0, 360 - (Mathf.Atan2(rotationAimTargetInput.x, rotationAimTargetInput.y) * Mathf.Rad2Deg * -1), 0);
            }


            rotationAimTarget = aimTransform.transform.rotation * Vector3.forward;

            Vector2 characterRotation = new Vector2();


            if (!_istantRotation) {
                if (_isRun) {
                    characterModelRotation = new Vector2(rotationAimTarget.x, rotationAimTarget.z);

                    characterModel.transform.rotation =
                        Quaternion.Lerp(characterModel.transform.rotation,
                        Quaternion.Euler(0, 360 - (Mathf.Atan2(characterModelRotation.x, characterModelRotation.y) * Mathf.Rad2Deg * -1), 0),
                        Time.deltaTime * rotationSpeed);
                } else if (
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
            } else {
                characterModel.transform.rotation = Quaternion.Euler(0, 360 - (Mathf.Atan2(_2Drotate.x, _2Drotate.y) * Mathf.Rad2Deg * -1), 0);
            }

        }

    }

    public void makeJump(Vector2 _2Direction) {

        Vector3 direction = new Vector3(_2Direction.x, 0, _2Direction.y);

        if(_2Direction.magnitude > 0) {

            if (characterState.grounded && characterState.readyToJump && characterState.running) { //deve essere finita anche l'animazione prima di un nuovo salto

                characterState.grounded = false;
                jumpColliderCharacter.enabled = true;
                characterAimConstrant.weight = 0; // rimuovi forza constraint rigging mira personaggio(aim)
                
                rotateCharacter(_2Direction, false, true);
                characterState.jumping = true;
                characterState.readyToJump = false;
                animator.SetBool("isJumping", characterState.jumping);

                characterRigidBody.AddForce(
                    ((transform.up / 2f) + direction.normalized * 2.5f) * jumpSpeed, ForceMode.Impulse);

                colliderCharacter.height = 2.5f;
            }
        }
    }

    IEnumerator waitBeforeJump() {
        
        yield return new WaitForSeconds(0.45f);
        characterState.readyToJump = true;
        characterAimConstrant.weight = 1; // ripristina forza constraint rigging mira personaggio(aim)
        jumpColliderCharacter.enabled = false;

    }

    //make sure u replace "floor" with your gameobject name.on which player is standing
    void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.layer == 6) {

            

            // rallenta player quando entra in contatto con la superficie del pavimento (layer floor)
            characterRigidBody.velocity = characterRigidBody.velocity / 2;

            colliderCharacter.height = 3.1f;

            characterState.grounded = true;
            characterState.jumping = false;
            animator.SetBool("isJumping", characterState.jumping);

            
            StartCoroutine(waitBeforeJump());
        }
    }

   

    private void OnCollisionStay(Collision collision) {
        if (collision.gameObject.layer == 6) {
            jumpColliderCharacter.enabled = false;


            characterState.grounded = true;
            characterState.jumping = false;
            
        }
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 5);
    }
}

    
    
