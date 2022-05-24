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

        InventoryManager iM = gameObject.GetComponent<InventoryManager>();

        if (iM == null) {
            gameObject.AddComponent<InventoryManager>();
            iM = gameObject.GetComponent<InventoryManager>();
        }
        inventoryManager = iM;


        updateAnimatorStateByInventoryWeaponType(inventoryManager.getSelectedWeaponType, inventoryManager);
    }


    /// <summary>
    /// Metodo con cui viene applicato uno spostamento sul character
    /// </summary>
    /// <param name="_2Dmove">Coppia di valori che rappresenta i valori
    /// in input del movimento del character.</param>
    public void moveCharacter(Vector2 _2Dmove, bool isRun) {
        
        Vector3 _movementAnimationVelocity; // velocity input analogico


        _movementAnimationVelocity = _movement = new Vector3(_2Dmove.x, 0f, _2Dmove.y);





        // setta traslazione utilizzando il deltaTime(differisce dalla frequenza dei fotogrammi)
        // evitando che il movimento del character dipenda dai fotogrammi
        if (_movement.magnitude > 0) {

            if (isRun) {
                

                _movement = _movement * runMovementSpeed * Time.deltaTime;

                rotateCharacter(_2Dmove, isRun, true);
            } else {

                _movement = _movement * movementSpeed * Time.deltaTime;

                /*if(gameObject.GetComponent<CharacterManager>().isPlayer) {
                    if (inventoryManager.getSelectedWeaponType == WeaponType.melee) {
                        rotateCharacter(_2Dmove, isRun, true);
                    }
                }*/
                
            }
            gameObject.GetComponent<CharacterManager>().isRunning = isRun;
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
    public void rotateCharacter(Vector2 _2Drotate, bool _isRun, bool _istantRotation) {
        Vector3 rotationAimTargetInput; // vettore rotazione target

        // clamp dei valori passati 
        rotationAimTargetInput = new Vector3(
            _2Drotate.x,
            _2Drotate.y,
            0f);

        rotationAimInput = rotationAimTargetInput;




        if (rotationAimTargetInput.magnitude > 0) {


            if (!_istantRotation) {


                characterModel.transform.rotation = Quaternion.Euler(0, 360 - (Mathf.Atan2(_2Drotate.x, _2Drotate.y) * Mathf.Rad2Deg * -1), 0);
            } else {

                characterModel.transform.rotation = Quaternion.Euler(0, 360 - (Mathf.Atan2(_2Drotate.x, _2Drotate.y) * Mathf.Rad2Deg * -1), 0);
            }

        }

    }

    void Update() {

        // muovi character solo se il character � il giocatore
        // caso in cui il character � slegato dal nav mesh agent (sei un player)
        if (gameObject.GetComponent<CharacterManager>().isPlayer) {

            characterController.SimpleMove(Vector3.zero); // utile per rilevare le collisioni
            if (!isGrounded) {
                
                characterController.Move(_movement + (- gameObject.transform.up * gravity)); // muovi e applica gravit� 
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


}



