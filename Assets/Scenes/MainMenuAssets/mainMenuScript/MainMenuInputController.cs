using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuInputController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputAction playerActions;
    [SerializeField] private MainMenuManager mainMenuManager;

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
        if (backButtonInput == 1f) {
            mainMenuManager.handleBackMenu();
        }
    }
}
