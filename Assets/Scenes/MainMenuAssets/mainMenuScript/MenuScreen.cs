using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuScreen : MonoBehaviour {
    [SerializeField] private Button _firtButton;
    [SerializeField] private List<MenuButtonUIManager> menuButtons;

    public void initMenuScreen() {
        
        foreach(MenuButtonUIManager button in menuButtons) {
            button.buttonNotSelected();
        }
    }

    public Button firtButton {
        get { return _firtButton; }
    }
}
