using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarpCharacterElement : MonoBehaviour {
    [Header("Assets refs")]
    [SerializeField] private Sprite controlledCharacter;
    [SerializeField] private Sprite notControlledCharacter;

    [Header("GameObj refs")]
    [SerializeField] private Image characterImage;
    [SerializeField] private Image characterCursorImage;


    public void initWarpCharacterElement(bool isCharacterControlled, bool controlCursorActive) {

        if(isCharacterControlled) {
            characterImage.sprite = controlledCharacter;
        } else {
            characterImage.sprite = notControlledCharacter;
        }

        if(controlCursorActive) {
            characterCursorImage.gameObject.SetActive(true);
        } else {
            characterCursorImage.gameObject.SetActive(false);
        }
    }
}
