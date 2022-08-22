using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class GeneratorInteractable : Interactable {

    [Header("References")]
    [SerializeField] private ScenePowerController scenePowerController; // game state per accedere ai metodi dello stato di gioco
    [SerializeField] private AudioSource audioSource;

    [Header("States")]
    [SerializeField] string sabotageGeneratorEventName = "SABOTAGE GENERATOR";
    [SerializeField] UnityEventCharacter sabotageGenerator = new UnityEventCharacter();
    [SerializeField] private GeneratorState generatorState = GeneratorState.GeneratorOn;
    private bool isSabotage = false;
    private int numberOfEnemyToWarn = 1; // count numero nemici da warnare al sabotaggio

    [Header("generator config")]
    [SerializeField] private float sabotageTime = 2f; // tempo per sabotare il generatore

    [Header("Asset Refs")]
    [SerializeField] private AudioClip interactAudioClip;



    public override void Start() {
        initInteractable();

        sabotageGenerator.AddListener(switchOffGenerator);

        if(generatorState == GeneratorState.GeneratorOn) {
            interactableMeshEffectSetEnebled(true);
        }
    }

    private async void switchOffGenerator(CharacterManager characterWhoIsInteracting) {

        isSabotage = true;
        characterWhoIsInteracting.isSuspiciousGenericAction = true; // permette al player di diventare sospetto/ostile
        characterWhoIsInteracting.alarmAlertUIController.potentialSuspiciousGenericActionAlarmOn(); // avvia potenziale stato alert

        // sound 
        playSounds();

        // avvia task sul character che ha avviato il task
        bool playerTaskResultDone = await characterWhoIsInteracting.startTimedInteraction(sabotageTime, "Sabotage");

        characterWhoIsInteracting.isSuspiciousGenericAction = false;

        isSabotage = false;
        if (playerTaskResultDone) {
            generatorState = GeneratorState.GeneratorOff;
            scenePowerController.turnOffPower();
            interactableMeshEffectSetEnebled(false);
        }

        characterWhoIsInteracting.alarmAlertUIController.potentialSuspiciousGenericActionAlarmOff();
        characterWhoIsInteracting.buildListOfInteraction(); // rebuilda UI

        callEnemy();
    }

    /// <summary>
    /// Metodo per rendere generatore disattivabile(sabotaggio)
    /// </summary>
    public void switchOnGenerator() {
        generatorState = GeneratorState.GeneratorOn;
        interactableMeshEffectSetEnebled(true);
    }

    public override Interaction getMainInteraction() {
        return new Interaction(sabotageGenerator, sabotageGeneratorEventName, this);
    }

    public override List<Interaction> getInteractions(CharacterManager character = null) {

        List<Interaction> eventRes = new List<Interaction>();

        if(generatorState == GeneratorState.GeneratorOn && scenePowerController.getPowerOn() && !isSabotage) {
            eventRes.Add(new Interaction(sabotageGenerator, sabotageGeneratorEventName, this));
        }

        return eventRes;
    }

    private void playSounds() {
        audioSource.clip = interactAudioClip;
        audioSource.Play();
    }

    private void callEnemy() {

        List<EnemyNPCBehaviourManager> _enemyNpcList
            = scenePowerController.gameObject.GetComponent<SceneEntitiesController>().enemyNpcList;


        for(int i = 0; i < numberOfEnemyToWarn; i++) {
            EnemyNPCBehaviourManager enemy = SceneEntitiesController.getCloserEnemyCharacterFromPosition(gameObject.transform.position, _enemyNpcList);

            Vector3 nearPos = CharacterManager.getPositionReachebleByAgents(enemy.characterManager, gameObject.transform.position);



            enemy.setAlert(
                CharacterAlertState.WarnOfSuspiciousAlert,
                true,
                lastSeenFocusAlarmPosition: nearPos
            );
        }

        // incrementa numero di guardie da chiamare al sabotaggio successivo
        numberOfEnemyToWarn++;
    }
}

public enum GeneratorState {
    GeneratorOn,
    GeneratorOff,
}