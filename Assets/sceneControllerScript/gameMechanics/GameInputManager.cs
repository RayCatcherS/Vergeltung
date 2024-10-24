using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInputManager : MonoBehaviour
{
    private const float INVENTARY_WAIT_TIME = 0.2f;

    [Header("Ref")]
    [SerializeField] private CharacterMovement _characterMovement;
    [SerializeField] private InventoryManager _inventoryManager;
    [SerializeField] private CharacterManager _characterManager;
    [SerializeField] private GameState _gameState;
    PlayerInputAction playerActions;

    private Vector2 vec2Movement; // vettore input movimento joypad(left analog stick)
    private Vector2 vec2Rotation; // vettore input rotazione joypad(right analog stick)

    [SerializeField] float rotationInputStickDeadZone = 0.7f;
    [SerializeField] float movementInputStickDeadZone = 0.45f;

    private float inputIsRun = 0;
    private bool isRunPressed = false;


    // inventary input
    private float inputIsNextWeaponPressed;
    private float inputIsPreviousWeaponPressed;
    private float inputIsUseWeaponItemPressed;
    private float inputIsPutAwayWeapon;
    private float inputIsExtractedWeapon;

    private bool isNextWeaponPressed = false;
    private bool isPreviousWeaponPressed = false;
    private bool isPutAwayWeapon = false;
    private bool isExtractedWeapon = false;

    // discard input
    private float inputIsDiscardActionPressed;

    // pause input
    private float inputIsPausePressed;
    private bool isPausePressed = false;

    // switchCharacterMode
    private float inputIsSwitchCharacterModePressed;
    private bool isSwitchCharacterModePressed = false;

    private float inputIsPreviousCharacterPressed;
    private bool isPreviousCharacterPressed = false;

    private float inputIsNextCharacterPressed;
    private bool isNextCharacterPressed = false;

    private float inputIsCharacterConfirmedInSCM;
    private bool isCharacterConfirmedInSCM = false;



    // getters and setters ref
    public CharacterMovement characterMovement {
        get { return _characterMovement; }
        set {
            _characterMovement = value;
        }
    }
    public InventoryManager inventoryManager {
        get { return _inventoryManager; }
        set {
            _inventoryManager = value;
        }
    }
    public CharacterManager characterManager {
        get { return _characterManager; }
        set {
            _characterManager = value;
        }
    }

    private void OnEnable() {
        playerActions.Player.Enable();
        playerActions.SwitchCharacterMode.Enable();
    }
    private void OnDisable() {
        playerActions.Player.Disable();
        playerActions.SwitchCharacterMode.Disable();
    }

    void Awake() {
        playerActions = new PlayerInputAction();

    }

    


    private void Update() {


        if(_gameState.gameState != GlobalGameState.switchCharacterMode) {
            if(_gameState.gameState != GlobalGameState.gameover && _gameState.gameState != GlobalGameState.pause) {


                if(!_characterManager.isDead) {

                    if(!_characterManager.isBusy) {
                        moveAndRotateInput();

                    } else {

                        _characterMovement.stopCharacter();
                    }
                    inventaryInput();
                    onDiscardPressed();
                }


            }

            startSwitchCharacterModeInput();
            pauseInput();
        } else if(_gameState.gameState == GlobalGameState.switchCharacterMode) {
            
            switchCharacterModeInput();
        }
        
    }

    /// <summary>
    /// Input move and rotate character
    /// </summary>
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
        if (vec2Movement.magnitude < 0.9f || vec2Rotation.magnitude > 0) {

            isRunPressed = false;
        }


        if (isRunPressed) { // if [isRun] allora non sarà possibile cambiare la rotazione del character

            characterMovement.moveCharacter(vec2Movement, isRunPressed);

        } else { // altrimenti movimento default

            if(characterMovement != null) {
                characterMovement.moveCharacter(vec2Movement, isRunPressed);

                characterMovement.rotateCharacter(vec2Rotation, true);
            }
            
        }
    }

    /// <summary>
    /// Input inventario
    /// </summary>
    private void inventaryInput() {

        inputIsNextWeaponPressed = playerActions.Player.InventoryNextWeapon.ReadValue<float>();
        inputIsPreviousWeaponPressed = playerActions.Player.InventoryPreviousWeapon.ReadValue<float>();
        inputIsUseWeaponItemPressed = playerActions.Player.InventoryUseWeaponItem.ReadValue<float>();
        inputIsPutAwayWeapon = playerActions.Player.PutAwayWeapon.ReadValue<float>();
        inputIsExtractedWeapon = playerActions.Player.ExtractWeapon.ReadValue<float>();


        // next weapon input
        if (inputIsNextWeaponPressed == 1) {

            if (!isNextWeaponPressed) {
                _inventoryManager.selectNextWeapon();
                isNextWeaponPressed = true;
            }

        } else {
            isNextWeaponPressed = false;
        }

        // preview weapon input
        if (inputIsPreviousWeaponPressed == 1) {

            if (!isPreviousWeaponPressed) {
                _inventoryManager.selectPreviousWeapon();

                isPreviousWeaponPressed = true;
            }

        } else {
            isPreviousWeaponPressed = false;
        }

        // putAwayWeapon input
        if (inputIsPutAwayWeapon == 1) {
            if (!isPutAwayWeapon) {
                _inventoryManager.putAwayWeapon();

                isPutAwayWeapon = true;
            }
        } else {
            isPutAwayWeapon = false;
        }

        // extractWeapon input
        if(inputIsExtractedWeapon == 1) {
            if(!isExtractedWeapon) {
                _inventoryManager.extractWeapon();

                isExtractedWeapon = true;
            }
        } else {
            isExtractedWeapon = false;
        }



        WeaponItem usedWeapon = _inventoryManager.getSelectWeapon();
        if(usedWeapon != null) {

            if(usedWeapon.automaticWeapon) {
                if (inputIsUseWeaponItemPressed == 1) {

                    _inventoryManager.useSelectedWeapon();
                    usedWeapon.weaponUsed = true;

                } else {
                    usedWeapon.weaponUsed = false;
                }
            } else {
                if (inputIsUseWeaponItemPressed == 1) {

                    if (!usedWeapon.weaponUsed) {
                        _inventoryManager.useSelectedWeapon();
                        usedWeapon.weaponUsed = true;
                    }

                } else {
                    usedWeapon.weaponUsed = false;
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void onDiscardPressed() {
        inputIsDiscardActionPressed = playerActions.Player.DiscardAction.ReadValue<float>();

        if (inputIsDiscardActionPressed == 1) {
            characterManager.discardCharacterAction();
        }
    }

    private void pauseInput() {
        inputIsPausePressed = playerActions.Player.Pause.ReadValue<float>();



        if(inputIsPausePressed == 1) {

            if(!isPausePressed) {
                if(_gameState.gameState == GlobalGameState.play) {

                    gameObject.GetComponent<GameState>().initPauseGameState();

                } else if(_gameState.gameState == GlobalGameState.pause) {

                    gameObject.GetComponent<GameState>().resumeGameState();
                }

                isPausePressed = true;
            }

        } else {
            isPausePressed = false;
        }

    }

    private void startSwitchCharacterModeInput() {
        inputIsSwitchCharacterModePressed = playerActions.Player.SwitchCharacterMode.ReadValue<float>();
        if(inputIsSwitchCharacterModePressed == 1) {

            if(!isSwitchCharacterModePressed) {
                gameObject.GetComponent<PlayerWarpController>().startSwitchCharacterMode();

                isSwitchCharacterModePressed = true;
            }

        } else {
            isSwitchCharacterModePressed = false;
        }
    }

    private void switchCharacterModeInput() {
        
        inputIsPreviousCharacterPressed = playerActions.SwitchCharacterMode.PreviousCharacter.ReadValue<float>();
        inputIsNextCharacterPressed = playerActions.SwitchCharacterMode.NextCharacter.ReadValue<float>();

        inputIsCharacterConfirmedInSCM = playerActions.SwitchCharacterMode.Action.ReadValue<float>();


        // character precedente
        if(inputIsPreviousCharacterPressed == 1) {

            if(!isPreviousCharacterPressed) {
                
                // action 
                gameObject.GetComponent<PlayerWarpController>().previousSwitchCharacter();

                isPreviousCharacterPressed = true;
            }
        } else {
            isPreviousCharacterPressed = false;
        }


        // character successivo
        if(inputIsNextCharacterPressed == 1) {

            if(!isNextCharacterPressed) {

                // action
                gameObject.GetComponent<PlayerWarpController>().nextSwitchCharacter();

                isNextCharacterPressed = true;
            }
        } else {
            isNextCharacterPressed = false;
        }


        // conferma character to warp
        if(inputIsCharacterConfirmedInSCM == 1) {

            if(!isCharacterConfirmedInSCM) {

                // action
                gameObject.GetComponent<PlayerWarpController>().confirmSelectedSwitchCharacterAsync();

                isCharacterConfirmedInSCM = true;
            }
        } else {
            isCharacterConfirmedInSCM = false;
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
