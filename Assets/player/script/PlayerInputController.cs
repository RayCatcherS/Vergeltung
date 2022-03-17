using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{

    [SerializeField] private CharacterMovement characterMovement;
    PlayerInputAction playerActions;

    private Vector2 vec2Movement; // vettore input movimento joypad(left analog stick)
    private Vector2 vec2Rotation; // vettore input rotazione joypad(right analog stick)

    [SerializeField] float rotationInputStickDeadZone = 0.125f;
    [SerializeField] float movementInputStickDeadZone = 0.125f;

    private float inputIsRun = 0;
    private bool isRunPressed = false;

    private void OnEnable() {
        playerActions.Player.Enable();
    }
    private void OnDisable() {
        playerActions.Player.Disable();
    }

    void Awake() {
        playerActions = new PlayerInputAction();

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update() {


        vec2Movement = playerActions.Player.AnalogMovement.ReadValue<Vector2>(); // ottieni valore input controller analogico movimento
        vec2Rotation = playerActions.Player.AnalogRotation.ReadValue<Vector2>(); // ottieni valore input controller analogico rotazione
        inputIsRun = playerActions.Player.Run.ReadValue<float>();

        
    }

    // input movimento più efficaci nei FixedUpdate
    private void FixedUpdate() {
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
            characterMovement.rotateCharacter(vec2Rotation, isRunPressed, false);
        }

    }
    

    void OnGUI() {
        GUI.TextArea(new Rect(0, 0, 200, 100), "Direzioni vettori: \n" + "input rotazione \n" + characterMovement.getRotationAimInput.ToString() + "\n" +
            "rotazione target \n" + characterMovement.getRotationAimTarget.ToString()
            , 200);
        GUI.TextArea(new Rect(0, 100, 200, 40), "rotazione character \n" + characterMovement.getCharacterModelRotation.ToString(), 200);
        GUI.TextArea(new Rect(0, 140, 200, 40), "input is run: \n" + isRunPressed.ToString(), 200);
    }
}
