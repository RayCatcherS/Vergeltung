using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GameModeController : MonoBehaviour {
    [Header("Refs")]
    [SerializeField] private VerticalLayoutGroup _gameGoalsGroup;

    [Header("Prefab")]
    [SerializeField] private GameObject gameGoalUIItemPrefab;

    [Header("Data")]
    [SerializeField] private List<GameGoal> _gameGoals = new List<GameGoal>();


    private void Start() {
        int enemyGGCount = searchTargetEnemyGameGoals();
        updateGoals("Kill the guardians", enemyGGCount);

        Machinery[] machineries = FindObjectsOfType(typeof(Machinery)) as Machinery[];
        updateGoals("Disable the monolith machines", machineries.Length);
        

        drawGameGoals();
    }

    private void drawGameGoals(int goalToPop = -1) {

        for(int i = 0; i < _gameGoals.Count; i++) {
            GameObject nGG = Instantiate(gameGoalUIItemPrefab);
            nGG.transform.SetParent(_gameGoalsGroup.transform);

            nGG.GetComponent<GameGoalItem>().initGameGoalItem(_gameGoals[i]);

            if(goalToPop != -1 && goalToPop == i) {
                nGG.GetComponent<GameGoalItem>().imagePopAnimation();
            }
        }
    }

    private int searchTargetEnemyGameGoals() {

        int goalsCount = 0;

        // cerca character target
        List<CharacterManager> characters = gameObject.GetComponent<SceneEntitiesController>().getAllNPC();

        for(int i = 0; i < characters.Count; i++) {
            if(characters[i].isTarget) {
                goalsCount++;
            }
        }

        return goalsCount;
    }

    private void updateGoals(string goalId, int numberOfGoal) {

        for(int i = 0; i < _gameGoals.Count; i++) {
            if(_gameGoals[i].goalName == goalId) {
                _gameGoals[i].setGoalToComplete(numberOfGoal);
            }
        }
    }

    public void updateGameGoalsStatus(string goalId, GameGoal.GameGoalOperation operation) {

        int goalToPop = -1;

        if(operation == GameGoal.GameGoalOperation.addGoal) {

            for(int i = 0; i < _gameGoals.Count; i++) {

                if(_gameGoals[i].goalName == goalId) {

                    _gameGoals[i].incrementCompleteGoals(1);
                    goalToPop = i;
                }
            }
        }

        drawGameGoals(goalToPop);
    }
}


[System.Serializable]
public class GameGoal {

    public enum GameGoalOperation {
        addGoal,
        removeGoal
    }

    [SerializeField]
    private string _goalName;
    public string goalName {
        get { return _goalName;  }
    }

    [SerializeField]
    private Sprite _goalCompleteSprite;
    public Sprite goalCompleteSprite {
        get { return _goalCompleteSprite; }
    }

    [SerializeField]
    private Sprite _goalToCompleteSprite;
    public Sprite goalToCompleteSprite {
        get { return _goalToCompleteSprite; }
    }

    [SerializeField]
    private int _goalsToComplete = 0;
    public int goalsToComplete {
        get { return _goalsToComplete; }
    }

    public void setGoalToComplete(int gToC) {
        _goalsToComplete = gToC;
    }

    [SerializeField]
    private int _completeGoals = 0;
    public int completeGoals {
        get { return _completeGoals; }
    }
    public void incrementCompleteGoals(int value) {
        _completeGoals = _completeGoals + value;
    }

    public bool isGameGoalComplete() {
        bool res = false;

        if(_completeGoals >= _goalsToComplete) {
            res = true;
        } else {
            res = false;
        }

        return res;
    }
}