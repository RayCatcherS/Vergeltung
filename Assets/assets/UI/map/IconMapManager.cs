using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class IconMapManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private bool iconBySelectedIcon = false;

    [Header("Refs")]
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Transform meshTransform;

    [Header("Assets Refs")]
    [SerializeField] private Texture playerS;
    [SerializeField] private Texture enemyS;
    [SerializeField] private Texture civilianS;
    [SerializeField] private Texture controlledCharacterS;
    [SerializeField] private Texture targetCharacterS;
    [SerializeField] private Texture genericGoalS;
    [SerializeField] private Texture areaGoalS;

    public CharacterIcon selectedIcon = CharacterIcon.player;


    private Vector3 lowIndicatorScale = new Vector3(2, 2, 2);
    private Vector3 largeIndicatorScale = new Vector3(5, 5, 5);

    private void Start() {
        if(iconBySelectedIcon) {
            changeIcon(selectedIcon);
        }
    }

    public enum CharacterIcon {
        player,
        enemy,
        civilian,
        controlledCharacter,
        targetCharacter,
        genericGoal,
        areaGoal
    }
    public void changeIcon(CharacterIcon icon) {
        
        if(icon == CharacterIcon.enemy) {

            meshRenderer.material.SetTexture("_BaseMap", enemyS);


            meshTransform.localScale = lowIndicatorScale;

        } else if(icon == CharacterIcon.civilian) {

            meshRenderer.material.SetTexture("_BaseMap", civilianS);

            meshTransform.localScale = lowIndicatorScale;

        } else if(icon == CharacterIcon.controlledCharacter) {

            meshRenderer.material.SetTexture("_BaseMap", controlledCharacterS);

            meshTransform.localScale = lowIndicatorScale;

        } else if(icon == CharacterIcon.targetCharacter) {

            meshRenderer.material.SetTexture("_BaseMap", targetCharacterS);

            meshTransform.localScale = largeIndicatorScale;
        } else if(icon == CharacterIcon.player) {

            meshRenderer.material.SetTexture("_BaseMap", playerS);

            meshTransform.localScale = lowIndicatorScale;

        } else if(icon == CharacterIcon.genericGoal) {

            meshRenderer.material.SetTexture("_BaseMap", genericGoalS);

            meshTransform.localScale = largeIndicatorScale;

        } else if(icon == CharacterIcon.areaGoal) {

            meshRenderer.material.SetTexture("_BaseMap", areaGoalS);

            meshTransform.localScale = largeIndicatorScale;

        }
    }

    public void disableIcon() {
        meshRenderer.enabled = false;
    }

}
