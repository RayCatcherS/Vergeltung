using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class GameModeController : MonoBehaviour {
    [Header("Refs")]
    [SerializeField] private GameState _gameState;
    [SerializeField] private VerticalLayoutGroup _gameGoalsGroup;
    private List<GameObject> gameGoalsItemUI = new List<GameObject>();


    [Header("Prefab")]
    [SerializeField] private GameObject gameGoalUIItemPrefab;

    [Header("Data")]
    [SerializeField] private List<GameGoal> _gameGoals = new List<GameGoal>();
    [SerializeField] private List<UnityEvent> _eventIds = new List<UnityEvent>();


    private void Start() {
        int enemyGGCount = searchTargetEnemyGameGoals();
        updateGoals("Kill the guardians", enemyGGCount);

        Machinery[] machineries = FindObjectsOfType(typeof(Machinery)) as Machinery[];
        updateGoals("Disable the monolith machines", machineries.Length);
        

        drawGameGoals();
    }

    private void drawGameGoals(int goalToPop = -1) {

        // elimina vecchia UI
        if(gameGoalsItemUI.Count > 0) {
            foreach(GameObject gg in gameGoalsItemUI) {
                Destroy(gg);
            }
        }


        for(int i = 0; i < _gameGoals.Count; i++) {
            GameObject nGG = Instantiate(gameGoalUIItemPrefab);
            nGG.transform.SetParent(_gameGoalsGroup.transform);

            nGG.GetComponent<GameGoalItem>().initGameGoalItem(_gameGoals[i]);

            if(goalToPop != -1 && goalToPop == i) {
                nGG.GetComponent<GameGoalItem>().imagePopAnimation();
            }

            gameGoalsItemUI.Add(nGG);
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

    /// <summary>
    /// Aggiorna obiettivi missione
    /// Da chiamare quando il character porta a compimento un certo evento
    /// </summary>
    /// <param name="goalId"></param>
    /// <param name="operation"></param>
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
        checkIfEventIDAreComplete();
        checkIfAllEventsAreComplete();
    }

    /// <summary>
    /// Verifica se tutti gli eventi che hanno un certo id sono tutti completi, se si esegui l'evento corrispondente all'id
    /// </summary>
    private void checkIfEventIDAreComplete() {

        for(int i = 0; i < _eventIds.Count; i++) {

            bool eventUnlock = true;

            for(int j = 0; j < _gameGoals.Count; j++) {
                if(i == _gameGoals[j].unlockEventID) {

                    if(!_gameGoals[j].isGameGoalComplete()) {
                        eventUnlock = false;
                    }
                }
            }

            if(eventUnlock) {
                _eventIds[i].Invoke();
            }
        }
    }

    private void checkIfAllEventsAreComplete() {
        bool res = true; 

        for(int j = 0; j < _gameGoals.Count; j++) {


            if(!_gameGoals[j].isGameGoalComplete()) {
                res = false;
            }
        }


        if(res) {
            // avvia vincita
            _gameState.initWinState();
        }
    }
}


[System.Serializable]
public class GameGoal {

    public enum GameGoalOperation {
        addGoal,
        removeGoal
    }

    [SerializeField] private string _goalName;
    public string goalName {
        get { return _goalName;  }
    }

    /// <summary>
    /// Sblocca un certo evento se tutti i goal che hanno lo stesso eventID sono completi
    /// </summary>
    [SerializeField] private int _unlockEventID = -1;
    public int unlockEventID {
        get { return _unlockEventID; }
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