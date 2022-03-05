using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class character_movement : MonoBehaviour
{
    [SerializeField] private float _movementSpeed = 5f;
    [SerializeField] private float _rotationSpeed = 20f;
    Vector3 _movement; // vettore movimento character
    Vector3 _rotation; // vettore rotazione character
    Vector2 vec2Movement; // vettore input movimento joypad(left analog stick)
    Vector2 vec2Rotation; // vettore input rotazione joypad(right analog stick)
    PlayerInputAction _playerActions;


    void Awake() {
        _playerActions = new PlayerInputAction();
       
    }

    // Start is called before the first frame update
    void Start() {
        _playerActions.Player.Enable();
    }

    // Update is called once per frame
    void Update() {
        
        // ottieni valore input controller
        vec2Movement = _playerActions.Player.AnalogMovement.ReadValue<Vector2>();

        vec2Rotation = _playerActions.Player.AnalogRotation.ReadValue<Vector2>();

        // avvalora vettore movimento character
        _movement = new Vector3(vec2Movement.x, 0f, vec2Movement.y);
        _rotation = new Vector3(vec2Rotation.x, vec2Rotation.y, 0f);

        
        if (_movement.magnitude > 0) {
            
            _movement *= _movementSpeed * Time.deltaTime;


            // applica movimento al character
            transform.Translate(_movement, Space.World);
        }


        if(_rotation.magnitude > 0) {
            // ruota player
            //transform.rotation = Quaternion.Euler(0, 360 - (Mathf.Atan2(_rotation.x, _rotation.y) * Mathf.Rad2Deg * -1), 0);

            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 360 - (Mathf.Atan2(_rotation.x, _rotation.y) * Mathf.Rad2Deg * -1), 0), Time.deltaTime * _rotationSpeed);
        }
    }
}
