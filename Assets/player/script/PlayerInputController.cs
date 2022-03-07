using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputController : MonoBehaviour
{

    CharacterMovement _characterMovement;
    PlayerInputAction _playerActions;

    Vector2 vec2Movement; // vettore input movimento joypad(left analog stick)
    Vector2 vec2Rotation; // vettore input rotazione joypad(right analog stick)

    void Awake() {
        _playerActions = new PlayerInputAction();
        _playerActions.Player.Enable();
        _characterMovement = GetComponent<CharacterMovement>();

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // ottieni valore input controller analogico movimento
        vec2Movement = _playerActions.Player.AnalogMovement.ReadValue<Vector2>();
        _characterMovement.moveCharacter(vec2Movement);


        // ottieni valore input controller analogico rotazione
        vec2Rotation = _playerActions.Player.AnalogRotation.ReadValue<Vector2>();
        _characterMovement.rotateCharacter(vec2Rotation);
    }
}
