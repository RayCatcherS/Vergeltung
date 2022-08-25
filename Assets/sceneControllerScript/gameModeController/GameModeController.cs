using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModeController : MonoBehaviour {
    [SerializeField]
    private List<GameGoal> _gameGoals = new List<GameGoal>();



    private void searchGameGoals() {

    }
}


[System.Serializable]
public class GameGoal {
    [SerializeField]
    private string goalName;

    [SerializeField]
    private Sprite goalSprite;

    [SerializeField]
    private int goalsToComplete = 0;

    [SerializeField]
    private int completeGoals = 0;
}