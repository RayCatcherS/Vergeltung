using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameGoalItem : MonoBehaviour {
    [Header("Refs")]
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image imageIcon;
    [SerializeField] private Animator imageAnimator;


    // variables
    private GameGoal _gameGoal;

    public void initGameGoalItem(GameGoal gameGoal) {
        _gameGoal = gameGoal;

        
    
        if(gameGoal.completeGoals >= gameGoal.goalsToComplete) {

            imageIcon.sprite = gameGoal.goalCompleteSprite;

            text.text = "<s>" + gameGoal.goalName + ": " + gameGoal.completeGoals + "/" + gameGoal.goalsToComplete + "</s>";

            Color color = Color.white;
            color.a = 0.5f;
            text.color = color;
        } else {

            imageIcon.sprite = gameGoal.goalToCompleteSprite;

            text.text = gameGoal.goalName + ": " + gameGoal.completeGoals + "/" + gameGoal.goalsToComplete;

            Color color = Color.white;
            color.a = 1f;
            text.color = color;
        }
    }

    public void imagePopAnimation() {
        imageAnimator.StopPlayback();
        imageAnimator.StartPlayback();
    }
}
