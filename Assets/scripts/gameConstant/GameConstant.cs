using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConstant {
    static private float r = 1;  // red component
    static private float g = 0.5490196f;  // green component
    static private float b = 0;  // blue component
    [SerializeField] public static Color outlineInteractableColor = new Color(r, g, b);


    [SerializeField] public static Color outlineEnemyColor = new Color(r, g, b);
}
