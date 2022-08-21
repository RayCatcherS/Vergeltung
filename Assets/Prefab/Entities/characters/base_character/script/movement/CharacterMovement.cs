using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public enum RotationLerpSpeedValue {
    slow = 1,
    normal = 3,
    fast = 10,
}
public class CharacterMovement : MonoBehaviour {
    private const int DOOR_LAYER = 10;
    private const int CHARACTER_LAYER = 7;

    [Header("References")]
    public Animator animator; //animator del character
    public CharacterController characterController;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private CharacterManager characterManager;

    private const int ALL_LAYERS = -1;
    private const float NEGATIVE_ROTATION_CLAMP = -1f;
    private const float POSITIVE_ROTATION_CLAMP = 1f;
    private const float NEGATIVE_MOVEMENT_CLAMP = -1f;
    private const float POSITIVE_MOVEMENT_CLAMP = 1f;

    [Header("Config")]
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float runMovementSpeed = 10f;


    [SerializeField] private Vector3 rotationAimInput;
    public Vector3 getRotationAimInput { get { return rotationAimInput; } }
    [SerializeField] private Vector3 rotationAimTarget;
    public Vector3 getRotationAimTarget { get { return rotationAimTarget; } }
    [SerializeField] private Vector2 characterModelRotation;
    public Vector3 getCharacterModelRotation { get { return characterModelRotation; } }
    [SerializeField] private float gravity = 0.15f;

    [Header("LoudArea")]
    [SerializeField] private GameObject loudAreaAsset;







    // states
    private bool isGrounded = false;
    private Vector3 _movement; // vettore movimento character
    
    

    // ref getters 
    public GameObject characterModel {
        get { return gameObject; }
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
                case WeaponType.controlWeapon: {

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
    public void moveCharacter(Vector2 _2Dmove, bool isRun, bool autoRotationOnRun = true, bool onlyAnimation = false) {
        
        Vector3 _movementAnimationVelocity; // velocity input analogico

        
        _movementAnimationVelocity = _movement = new Vector3(_2Dmove.x, 0f, _2Dmove.y);





        // setta traslazione utilizzando il deltaTime(differisce dalla frequenza dei fotogrammi)
        // evitando che il movimento del character dipenda dai fotogrammi
        if(!onlyAnimation) {
            if(_movement.magnitude > 0) {


                if(isRun) {

                    if(characterManager.isPlayer) {
                        generateLoudArea();
                    }
                    

                    _movement = _movement * runMovementSpeed * Time.deltaTime;

                    if(autoRotationOnRun) {
                        rotateCharacter(_2Dmove, true);
                    }
                } else {

                    _movement = _movement * movementSpeed * Time.deltaTime;
                }

                characterManager.isRunning = isRun;
            }
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
    public void rotateCharacter(Vector2 _2Drotate, bool _istantRotation, RotationLerpSpeedValue rotationLerpSpeedValue = RotationLerpSpeedValue.normal) {
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


                float speedValue = (float)rotationLerpSpeedValue;
                characterModel.transform.rotation = Quaternion.Lerp(fromRotation, toRotation, Time.deltaTime * speedValue);
            } else {

                characterModel.transform.rotation = Quaternion.Euler(0, 360 - (Mathf.Atan2(_2Drotate.x, _2Drotate.y) * Mathf.Rad2Deg * -1), 0);
            }

        }
    }

    void Update() {

        // muovi character solo se il character è il giocatore
        // caso in cui il character è slegato dal nav mesh agent (sei un player)
        if (characterManager.isPlayer) {

            if(characterManager.globalGameState.gameState != GlobalGameState.pause && characterManager.globalGameState.gameState != GlobalGameState.gameover) {

                characterController.SimpleMove(Vector3.zero); // utile per rilevare le collisioni
                if(!isGrounded) {

                    characterController.Move(_movement + (-gameObject.transform.up * gravity)); // muovi e applica gravità 
                } else {
                    characterController.Move(_movement); // muovi player
                }
                groundCheck();
            }
            
        
        
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
    /// Genera loud area provocata dalla corsa del character
    /// </summary>
    private void generateLoudArea() {
        GameObject loudGameObject = Instantiate(loudAreaAsset, gameObject.transform.position, Quaternion.identity);
        loudGameObject.GetComponent<LoudArea>().initLoudArea(LoudAreaType.characterLoudRun, characterThatGenerateLA: characterManager);
        loudGameObject.GetComponent<LoudArea>().startLoudArea();
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

    void OnCollisionEnter(Collision collision) {
        
    }


    void OnControllerColliderHit(ControllerColliderHit hit) {


        // We dont want to push objects below us
        if(hit.moveDirection.y < -0.3) {
            return;
        }


        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

        

        if(hit.gameObject.layer == CHARACTER_LAYER) {
            hit.gameObject.transform.Translate(pushDir * Time.deltaTime, Space.World);

            //hit.gameObject.GetComponent<CharacterMovement>().moveCharacter(_movement * 3, false, onlyAnimation: true);
        }
    }

}