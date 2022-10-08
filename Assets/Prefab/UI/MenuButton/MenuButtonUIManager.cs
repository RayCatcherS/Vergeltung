using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuButtonUIManager : MonoBehaviour, ISelectHandler, IDeselectHandler {

    [SerializeField] private Text buttonText;
    [SerializeField] private Image buttonIconImage;


    public void OnSelect(BaseEventData eventData) {


        buttonSelected();
    }

    public void OnDeselect(BaseEventData eventData) {

        buttonNotSelected();
    }

    public void buttonNotSelected() {
        buttonIconImage.enabled = false;


        Color color = Color.white;
        color.a = 0.75f;

        buttonText.color = color;
    }

    public void buttonSelected() {
        buttonIconImage.enabled = true;


        Color color = Color.white;
        color.a = 1f;

        buttonText.color = color;
    }
}
