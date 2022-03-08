using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputController : MonoBehaviour
{

    CharacterMovement characterMovement;
    PlayerInputAction playerActions;

    [SerializeField] private Vector2 vec2Movement; // vettore input movimento joypad(left analog stick)
    [SerializeField] private Vector2 vec2Rotation; // vettore input rotazione joypad(right analog stick)

    [SerializeField] float rotationInputStickDeadZone = 0.125f;
    [SerializeField] float movementInputStickDeadZone = 0.125f;

    private float inputIsRun = 0;
    private bool isRun = false;

    void Awake() {
        playerActions = new PlayerInputAction();
        playerActions.Player.Enable();
        characterMovement = GetComponent<CharacterMovement>();

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {


        vec2Movement = playerActions.Player.AnalogMovement.ReadValue<Vector2>(); // ottieni valore input controller analogico movimento
        vec2Rotation = playerActions.Player.AnalogRotation.ReadValue<Vector2>(); // ottieni valore input controller analogico rotazione

        if(vec2Rotation.magnitude < rotationInputStickDeadZone) {
            vec2Rotation = Vector2.zero;
        }
        if(vec2Movement.magnitude < movementInputStickDeadZone) {
            vec2Movement = Vector2.zero;
        }

        if (isRun) { // se [isRun] allora l'input del movimento coinciderà con quello della rotazione
            
            characterMovement.moveCharacter(vec2Movement, isRun);
        } else { // altrimenti tornerà al movimento default

            characterMovement.moveCharacter(vec2Movement, isRun);
            characterMovement.rotateCharacter(vec2Rotation, isRun);
        }

        // abilita [isRun] se viene premuto il pulsante corsa
        inputIsRun = playerActions.Player.Run.ReadValue<float>();
        if (inputIsRun == 1) {
            isRun = true;
        }

        // disabilita [isRun] se viene il magnitude dell'input [vec2Movement]
        // � < 0(l'analogico movimento si avvicina al centro)
        // oppure se viene usato l'analogico per la rotazione
        if (vec2Movement.magnitude < 0.75f || vec2Rotation.magnitude > 0) {

            isRun = false;
        }
    }

    void OnGUI() {
        GUI.TextArea(new Rect(0, 0, 200, 100), "Direzioni vettori: \n" + "input rotazione \n" + characterMovement.getRotationAimInput.ToString() + "\n" +
            "rotazione target \n" + characterMovement.getRotationAimTarget.ToString()
            , 200);
        GUI.TextArea(new Rect(0, 100, 200, 40), "rotazione character \n" + characterMovement.getCharacterModelRotation.ToString(), 200);
        GUI.TextArea(new Rect(0, 140, 200, 40), "input is run: \n" + isRun.ToString(), 200);
    }
}
