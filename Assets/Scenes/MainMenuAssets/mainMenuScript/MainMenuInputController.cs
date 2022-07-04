using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuInputController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputAction playerActions;
    [SerializeField] private MainMenuManager mainMenuManager;

    [Header("Config")]
    [SerializeField] float leftStickDeadZone = 0.4f;

    private void OnEnable() {
        playerActions.MainMenu.Enable();
    }
    private void OnDisable() {
        playerActions.MainMenu.Disable();
    }

    void Awake() {
        playerActions = new PlayerInputAction();

    }
    
    void Start()
    {
        
    }


    void Update()
    {
        inputBackButton();
    }
    private void inputBackButton() {

        float backButtonInput = playerActions.MainMenu.ButtonBack.ReadValue<float>();
        Vector2 menuScrollInput = playerActions.MainMenu.MenuScroll.ReadValue<Vector2>();

        if (backButtonInput == 1f) {
            mainMenuManager.handleBackMenu();
        }

        if (menuScrollInput.magnitude < leftStickDeadZone) {
            menuScrollInput = Vector2.zero;
        } else {
            mainMenuManager.scrollStory(menuScrollInput);
        }
    }
}
