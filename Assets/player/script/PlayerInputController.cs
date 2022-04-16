using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    private const float INVENTARY_WAIT_TIME = 0.2f;

    [SerializeField] private CharacterMovement characterMovement;
    [SerializeField] private InventoryManager inventoryManager;
    PlayerInputAction playerActions;

    private Vector2 vec2Movement; // vettore input movimento joypad(left analog stick)
    private Vector2 vec2Rotation; // vettore input rotazione joypad(right analog stick)

    [SerializeField] float rotationInputStickDeadZone = 0.135f;
    [SerializeField] float movementInputStickDeadZone = 0.135f;

    private float inputIsRun = 0;
    private bool isRunPressed = false;


    
    private float inputIsNextWeaponPressed;
    private float inputIsPreviewWeaponPressed;
    private float inputIsUseWeaponItem;

    private bool isNextWeaponPressed = false;
    private bool isPreviewWeaponPressed = false;


    private void OnEnable() {
        playerActions.Player.Enable();
    }
    private void OnDisable() {
        playerActions.Player.Disable();
    }

    void Awake() {
        playerActions = new PlayerInputAction();

    }

    

    // input movimento più efficaci nei FixedUpdate
    private void FixedUpdate() {

        moveAndRotateInput();
        inventaryInput();
    }

    private void moveAndRotateInput() {
        vec2Movement = playerActions.Player.AnalogMovement.ReadValue<Vector2>(); // ottieni valore input controller analogico movimento
        vec2Rotation = playerActions.Player.AnalogRotation.ReadValue<Vector2>(); // ottieni valore input controller analogico rotazione
        
        inputIsRun = playerActions.Player.Run.ReadValue<float>();


        // abilita [isRun] se viene premuto il pulsante 
        if (inputIsRun == 1) {
            isRunPressed = true;
        }




        // calcolo delle dead zone degli stick analogici
        if (vec2Rotation.magnitude < rotationInputStickDeadZone) {
            vec2Rotation = Vector2.zero;
        }
        if (vec2Movement.magnitude < movementInputStickDeadZone) {
            vec2Movement = Vector2.zero;
        }

        // disabilita [isRun] se il magnitude dell'input [vec2Movement]
        // l'analogico movimento si avvicina al centro [vec2Movement.magnitude < 0.75f]
        // oppure se viene usato l'analogico per la rotazione [vec2Rotation.magnitude > 0]
        if (vec2Movement.magnitude < 0.75f || vec2Rotation.magnitude > 0) {

            isRunPressed = false;
        }


        if (isRunPressed) { // if [isRun] allora non sarà possibile cambiare la rotazione del character

            characterMovement.moveCharacter(vec2Movement, isRunPressed);

        } else { // altrimenti movimento default

            characterMovement.moveCharacter(vec2Movement, isRunPressed);

            if (characterMovement.GetComponent<InventoryManager>().getSelectedWeaponType != WeaponType.melee) {
                characterMovement.rotateCharacter(vec2Rotation, isRunPressed, false);
            }
        }
    }

    private void inventaryInput() {

        inputIsNextWeaponPressed = playerActions.Player.InventaryNextWeapon.ReadValue<float>();
        inputIsPreviewWeaponPressed = playerActions.Player.InventaryPreviewWeapon.ReadValue<float>();
        inputIsUseWeaponItem = playerActions.Player.InventaryUseWeaponItem.ReadValue<float>();


        if (inputIsNextWeaponPressed == 1) {

            if (!isNextWeaponPressed) {
                inventoryManager.nextWeapon();
                isNextWeaponPressed = true;
            }

            //Debug.Log("next");
            //StartCoroutine(DisablePreviewNextButtonsForSeconds());
        } else {
            isNextWeaponPressed = false;
        }

        if (inputIsPreviewWeaponPressed == 1) {

            if (!isPreviewWeaponPressed) {
                inventoryManager.previewWeapon();

                isPreviewWeaponPressed = true;
            }

            //Debug.Log("preview");
            //StartCoroutine(DisablePreviewNextButtonsForSeconds());
        } else {
            isPreviewWeaponPressed = false;
        }



        WeaponItem usedWeapon = inventoryManager.getSelectWeapon();
        if(usedWeapon != null) {
            if(inputIsUseWeaponItem == 1) {
                if(!usedWeapon.weaponUsed) {
                    inventoryManager.useSelectedWeapon();
                    usedWeapon.weaponUsed = true;
                }

            } else {
                usedWeapon.weaponUsed = false;
            }
        }
    }


    /*void OnGUI() {
        GUI.TextArea(new Rect(0, 0, 200, 100), "Direzioni vettori: \n" + "input rotazione \n" + characterMovement.getRotationAimInput.ToString() + "\n" +
            "rotazione target \n" + characterMovement.getRotationAimTarget.ToString()
            , 200);
        GUI.TextArea(new Rect(0, 100, 200, 40), "rotazione character \n" + characterMovement.getCharacterModelRotation.ToString(), 200);
        GUI.TextArea(new Rect(0, 140, 200, 40), "input is run: \n" + isRunPressed.ToString(), 200);
    }*/
}
