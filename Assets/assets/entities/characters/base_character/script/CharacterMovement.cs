using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class CharacterMovement : MonoBehaviour {
    private const int DOOR_LAYER = 10;

    public Animator animator; //animator del character
    public CharacterController characterController;

    private const int ALL_LAYERS = -1;
    private const float NEGATIVE_ROTATION_CLAMP = -1f;
    private const float POSITIVE_ROTATION_CLAMP = 1f;
    private const float NEGATIVE_MOVEMENT_CLAMP = -1f;
    private const float POSITIVE_MOVEMENT_CLAMP = 1f;

    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float runMovementSpeed = 10f;


    [SerializeField] private Vector3 rotationAimInput;
    [SerializeField] private Vector3 rotationAimTarget;
    [SerializeField] private Vector2 characterModelRotation;


    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private CharacterManager characterManager;



    public Vector3 getRotationAimInput { get { return rotationAimInput; } }
    public Vector3 getRotationAimTarget { get { return rotationAimTarget; } }
    public Vector3 getCharacterModelRotation { get { return characterModelRotation; } }


    private Vector3 _movement; // vettore movimento character
    [SerializeField] private float gravity = 9.8f;
    private bool isGrounded = false;

    // ref getters 
    public GameObject characterModel {
        get { return gameObject; }
    }

    void Awake() {

    }

    // Start is called before the first frame update
    void Start() {
        initCharacterMovement();
    }


    /// <summary>
    /// avvia transizione Bleend tree animation in base
    /// al tipo di arma impugnata
    /// </summary>
    public void updateAnimatorStateByInventoryWeaponType(WeaponType weaponType, InventoryManager inventoryManager) {
        animator.ResetTrigger("MeleeLocomotion");
        animator.ResetTrigger("PistolLocomotion");
        animator.ResetTrigger("RifleLocomotion");

        animator.Rebind();


        //Debug.Log(inventoryManager.getSelectedWeaponType);
        if(!inventoryManager.weaponPuttedAway) {
            switch (inventoryManager.getSelectedWeaponType) {
                case WeaponType.melee: {

                        animator.SetTrigger("MeleeLocomotion");
                    }
                    break;
                case WeaponType.pistol: {
                        animator.SetTrigger("PistolLocomotion");
                    }
                    break;
                case WeaponType.rifle: {
                        animator.SetTrigger("RifleLocomotion");

                    }
                    break;
            }
        } else {
            animator.SetTrigger("MeleeLocomotion");
        }
        
    }

    void initCharacterMovement() {


        updateAnimatorStateByInventoryWeaponType(inventoryManager.getSelectedWeaponType, inventoryManager);
    }


    /// <summary>
    /// Metodo con cui viene applicato uno spostamento sul character
    /// </summary>
    /// <param name="_2Dmove">Coppia di valori che rappresenta i valori
    /// in input del movimento del character.</param>
    public void moveCharacter(Vector2 _2Dmove, bool isRun, bool autoRotationOnRun = true) {
        
        Vector3 _movementAnimationVelocity; // velocity input analogico

        
        _movementAnimationVelocity = _movement = new Vector3(_2Dmove.x, 0f, _2Dmove.y);





        // setta traslazione utilizzando il deltaTime(differisce dalla frequenza dei fotogrammi)
        // evitando che il movimento del character dipenda dai fotogrammi
        if (_movement.magnitude > 0) {

            if (isRun) {


                _movement = _movement * runMovementSpeed * Time.deltaTime;

                if(autoRotationOnRun) {
                    rotateCharacter(_2Dmove, true);
                }
            } else {

                _movement = _movement * movementSpeed * Time.deltaTime;

            }
            characterManager.isRunning = isRun;
        }




        // setta valori animazione partendo dal _movementAnimationVelocity
        float velocityX = Vector3.Dot(_movementAnimationVelocity, characterModel.transform.right);
        float velocityZ;
        if (isRun) {


            animator.SetBool("isRunning", isRun);
            animator.SetFloat("VelocityZ", 2, 0.05f, Time.deltaTime);
        } else {

            animator.SetBool("isRunning", isRun);
            velocityZ = Vector3.Dot(_movementAnimationVelocity, characterModel.transform.forward);
            animator.SetFloat("VelocityZ", velocityZ, 0.05f, Time.deltaTime);
        }


        animator.SetFloat("VelocityX", velocityX, 0.05f, Time.deltaTime);
        
        
    }


    /// <summary>
    /// Metodo con cui viene applicato uno spostamento sul character
    /// </summary>
    /// <param name="_2Drotate">Coppia di valori che rappresenta i valori
    /// in input della rotazione dell'aim del character. I valori(x, y)</param>
    /// vengono inoltre calcolati i range delle direzioni dell'aim per ruotare l'intero character
    public void rotateCharacter(Vector2 _2Drotate, bool _istantRotation) {
        Vector3 rotationAimTargetInput; // vettore rotazione target


        // clamp dei valori passati 
        rotationAimTargetInput = new Vector3(
        _2Drotate.normalized.x,
        _2Drotate.normalized.y,
        0f);

        rotationAimInput = rotationAimTargetInput;




        if (rotationAimTargetInput.magnitude > 0) {


            if (!_istantRotation) {

                // lerp rotation
                Quaternion fromRotation = gameObject.transform.rotation;
                Quaternion toRotation = Quaternion.Euler(0, 360 - (Mathf.Atan2(_2Drotate.x, _2Drotate.y) * Mathf.Rad2Deg * -1), 0);


                characterModel.transform.rotation = Quaternion.Lerp(fromRotation, toRotation, Time.deltaTime * 3f);
            } else {

                characterModel.transform.rotation = Quaternion.Euler(0, 360 - (Mathf.Atan2(_2Drotate.x, _2Drotate.y) * Mathf.Rad2Deg * -1), 0);
            }

        }
    }

    void Update() {

        // muovi character solo se il character è il giocatore
        // caso in cui il character è slegato dal nav mesh agent (sei un player)
        if (characterManager.isPlayer) {

            characterController.SimpleMove(Vector3.zero); // utile per rilevare le collisioni
            if (!isGrounded) {
                
                characterController.Move(_movement + (- gameObject.transform.up * gravity)); // muovi e applica gravità 
            } else {
                characterController.Move(_movement); // muovi player
            }

            groundCheck();
        }

        
        
    }


    // check per verificare se il character "isGrounded" 
    private void groundCheck() {
        RaycastHit hit;

        if (Physics.Raycast(gameObject.transform.position, - gameObject.transform.up, out hit, 0.1f, ALL_LAYERS, QueryTriggerInteraction.Ignore)) {
            isGrounded = true;
        } else {
            isGrounded = false;
        }
    }

    /// <summary>
    /// Stoppa il character resettando il vettore _movement
    /// Stoppa animazione character
    /// </summary>
    public void stopCharacter() {
        characterManager.isRunning = false;
        animator.SetBool("isRunning", false);
        animator.SetFloat("VelocityX", 0, 0f, Time.deltaTime);
        animator.SetFloat("VelocityZ", 0, 0f, Time.deltaTime);
        _movement = Vector3.zero;
    }
}



