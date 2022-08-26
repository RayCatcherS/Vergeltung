using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// La goal area è un'area che se raggiunta permette chiamare un game goal
/// </summary>
public class GoalArea : MonoBehaviour {

    [Header("Config")]
    [SerializeField] private string goalId = "";

    [Header("Refs")]
    [SerializeField] private GameModeController gameModeController;


    /// <summary>
    /// L'evento può essere attivato dal solo player e non dai character controllati dal player
    /// </summary>
    [SerializeField] private bool isPlayerStartEvent;

    /// <summary>
    /// L'evento può essere attivato dal player e dai character controllati dal player
    /// </summary>
    [SerializeField] private bool isStackedControlledStartEvent;


    private bool eventStarted = false;


    private void OnTriggerEnter(Collider col) {

        if(!eventStarted) {
            CharacterManager character = col.gameObject.GetComponent<CharacterManager>();

            if(character != null) {

                if(isPlayerStartEvent) {

                    if(character.chracterRole == Role.Player) {
                        startEvent();

                        return;
                    }
                }

                if(isStackedControlledStartEvent) {

                    if(character.isStackControlled) {
                        startEvent();

                        return;
                    }
                }
            }
        }
        
    }


    private void startEvent() {

        gameModeController
            .updateGameGoalsStatus(goalId, GameGoal.GameGoalOperation.addGoal);

        eventStarted = true;
    }
}
