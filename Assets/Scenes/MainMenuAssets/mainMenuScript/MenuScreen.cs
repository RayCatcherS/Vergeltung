using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuScreen : MonoBehaviour {
    [SerializeField] private Button _firtButton;

    public Button firtButton {
        get { return _firtButton; }
    }
}
