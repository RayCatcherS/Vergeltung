using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AimUIManager : MonoBehaviour
{
    [SerializeField] private Canvas aimUITarget;
    [SerializeField] private Image uiImage;
    [SerializeField] private Sprite defaultAimSprite;
    [SerializeField] private Sprite controlOnAimSprite;
    [SerializeField] private Sprite controlOffAimSprite;

    public void updateUIWorldPosition(Vector3 pos) {
        aimUITarget.gameObject.transform.forward = Camera.main.transform.forward;
        aimUITarget.gameObject.transform.position = pos;
    }

    public void setAimTargetEnabled(bool value) {
        aimUITarget.gameObject.SetActive(value);
        
    }

    public void defaultAimLowOpacity() {
        Color color = Color.white;
        color.a = 0.05f;

        uiImage.color = color;

        uiImage.sprite = defaultAimSprite;
    }

    public void defaultAimMediumOpacity() {
        Color color = Color.white;
        color.a = 0.5f;

        uiImage.color = color;

        uiImage.sprite = defaultAimSprite;
    }

    public void aimHighOpacity() {
        Color color = Color.white;
        color.a = 1f;

        uiImage.color = color;

        uiImage.sprite = defaultAimSprite;
    }

    public void setControlAimSpriteOn() {
        uiImage.sprite = controlOnAimSprite;

        Color color = Color.white;
        color.a = 1f;

        uiImage.color = color;
    }

    public void setControlAimSpriteOff() {
        uiImage.sprite = controlOffAimSprite;

        Color color = Color.white;
        color.a = 1f;

        uiImage.color = color;
    }
}
