using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputController : MonoBehaviour
{

    CharacterMovement characterMovement;
    PlayerInputAction playerActions;

    [SerializeField] private Vector2 vec2Movement; // vettore input movimento joypad(left analog stick)
    [SerializeField] private Vector2 vec2Rotation; // vettore input rotazione joypad(right analog stick)

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
        // abilita [isRun] se viene premuto il pulsante corsa
        inputIsRun = playerActions.Player.Run.ReadValue<float>();
        if(inputIsRun == 1) {
            isRun = true;
        }

        
        // disabilita [isRun] se viene il magnitude dell'input [vec2Movement] è 0
        if (vec2Movement.magnitude == 0) {
            isRun = false;
        }


        
        if(isRun) { // se [isRun] allora l'input del movimento coinciderà con quello della rotazione
            vec2Movement = playerActions.Player.AnalogMovement.ReadValue<Vector2>();
            characterMovement.moveCharacter(vec2Movement);
            characterMovement.rotateCharacter(vec2Movement);
        } else { // altrimenti cammina

            // ottieni valore input controller analogico movimento
            vec2Movement = playerActions.Player.AnalogMovement.ReadValue<Vector2>();
            characterMovement.moveCharacter(vec2Movement);


            // ottieni valore input controller analogico rotazione
            vec2Rotation = playerActions.Player.AnalogRotation.ReadValue<Vector2>();
            characterMovement.rotateCharacter(vec2Rotation);
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
