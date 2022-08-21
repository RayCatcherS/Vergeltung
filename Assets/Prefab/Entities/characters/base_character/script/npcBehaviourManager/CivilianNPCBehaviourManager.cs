using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CivilianNPCBehaviourManager : BaseNPCBehaviourManager {
    static public GameObject initCivilianNPCComponent(GameObject gameObject, CharacterSpawnPoint spawnPoint) {

        CivilianNPCBehaviourManager enemyNPCNewComponent = gameObject.GetComponent<CivilianNPCBehaviourManager>();
        enemyNPCNewComponent.initNPCComponent(spawnPoint);

        return gameObject;
    }

    public override async void suspiciousAlertBehaviour() {

        if (!mainBehaviourProcess.processTaskFinished) {

            await mainBehaviourProcess.runBehaviourAsyncProcess();

            // inizializza punti casuali
            if (mainBehaviourProcess.processTaskFinished) {
                simulateSearchingPlayerSubBehaviourProcess.initBehaviourProcess();
            }

        } else {

            if (!simulateSearchingPlayerSubBehaviourProcess.processTaskFinished) {
                await simulateSearchingPlayerSubBehaviourProcess.runBehaviourAsyncProcess();
            }

        }

    }
    public override async void hostilityAlertBehaviourAsync() {
        await mainBehaviourProcess.runBehaviourAsyncProcess();
    }

    public override async void warnOfSuspiciousAlertBehaviour() {

        
        if (!mainBehaviourProcess.processTaskFinished) {

            await mainBehaviourProcess.runBehaviourAsyncProcess();

            // inizializza punti casuali
            if (mainBehaviourProcess.processTaskFinished) {
                simulateSearchingPlayerSubBehaviourProcess.initBehaviourProcess();
            }

        } else {

            if (!simulateSearchingPlayerSubBehaviourProcess.processTaskFinished) {
                await simulateSearchingPlayerSubBehaviourProcess.runBehaviourAsyncProcess();
            }

        }
    }

    public override async void suspiciousCorpseFoundAlertBehaviour() {
        await mainBehaviourProcess.runBehaviourAsyncProcess();
    }
    public override async void corpseFoundConfirmedAlertBehaviour() {
        await mainBehaviourProcess.runBehaviourAsyncProcess();
    }
    public override async void instantOnCurrentPositionWarnOfSouspiciousAlertBehaviour() {
        await mainBehaviourProcess.runBehaviourAsyncProcess();
    }
    public override async void playerLoudRunSuspiciousAlertBehaviour() {
        await mainBehaviourProcess.runBehaviourAsyncProcess();
    }





    protected override void startSuspiciousTimer(Vector3 _lastSeenFocusAlarmPosition) {
        stopAgent();

        // start behaviour process
        mainBehaviourProcess = new GenericSuspiciousProcess(_lastSeenFocusAlarmPosition, _agent, this);
        simulateSearchingPlayerSubBehaviourProcess
            = new MoveNPCBetweenRandomPointsProcess(agent, this, characterManager, 7, 4, 1);

        suspiciousTimerLoop();
    }
    protected override void startHostilityTimer(Vector3 _lastSeenFocusAlarmPosition, bool checkedByHimself) {
        stopAgent();

        // start behaviour process
        mainBehaviourProcess = new HostilityCivilianProcess(_lastSeenFocusAlarmPosition, _agent, this, _characterFOV, _characterManager, checkedByHimself);

        hostilityTimerLoop();
    }
    protected override void startWarnOfSouspiciousTimer(Vector3 _lastSeenFocusAlarmPosition) {
        stopAgent();

        mainBehaviourProcess = new WarnOfSospiciousEnemyProcess(_lastSeenFocusAlarmPosition, _agent, this);
        simulateSearchingPlayerSubBehaviourProcess
            = new MoveNPCBetweenRandomPointsProcess(agent, this, characterManager);

        warnOfSouspiciousTimerLoop();
    }
    protected override void startSuspiciousCorpseFoundTimer(Vector3 _lastSeenFocusAlarmPosition) {
        stopAgent();

        mainBehaviourProcess = new GenericSuspiciousCorpseFoundProcess(_lastSeenFocusAlarmPosition, _agent, this);

        suspiciousCorpseFoundTimerLoop();
    }
    protected override void startCorpseFoundConfirmedTimer(Vector3 _lastSeenFocusAlarmPosition) {
        stopAgent();

        mainBehaviourProcess = new CorpseFoundConfirmedCivilianProcess(_lastSeenFocusAlarmPosition, _agent, this, _characterFOV, _characterManager);

        corpseFoundConfirmedTimerLoop();
    }
    protected override void startInstantOnCurrentPositionWarnOfSouspiciousTimer(Vector3 _lastSeenFocusAlarmPosition) {
        stopAgent();

        mainBehaviourProcess = new MoveNPCBetweenRandomPointsProcess(
            agent, this, characterManager,
            areaRadius: 4,
            sampleToReach: 5,
            waitingOnPointTime: 0.5f,
            lastSeenFocusAlarmPosition: _lastSeenFocusAlarmPosition,
            lastSeenFocusAlarmPositionIsFirstPoint: true
        );
        mainBehaviourProcess.initBehaviourProcess();

        instantOnCurrentPositionWarnOfSouspiciousTimerLoopAsync();
    }
    protected override void startStayOnPositionSuspiciousTimer(Vector3 _lastSeenFocusAlarmPosition) {
        stopAgent();

        mainBehaviourProcess = new GenericStayOnPositionSuspiciousProcess(_lastSeenFocusAlarmPosition, _agent, this);
        mainBehaviourProcess.initBehaviourProcess();

        stayOnPositionSuspiciousTimerLoopAsync();
    }




    protected override void resetSuspiciousBehaviour(Vector3 _lastSeenFocusAlarmPosition) {
        mainBehaviourProcess = new GenericSuspiciousProcess(_lastSeenFocusAlarmPosition, _agent, this);
        simulateSearchingPlayerSubBehaviourProcess
            = new MoveNPCBetweenRandomPointsProcess(agent, this, characterManager, 7, 4, 1);

        // reset time of loop
        suspiciousTimerEndStateValue = Time.time + suspiciousTimerValue;
    }
    protected override void resetHostilityBehaviour(Vector3 _lastSeenFocusAlarmPosition) {

        hostilityTimerEndStateValue = Time.time + hostilityTimerValue;
    }



    /// <summary>
    /// Timer loop usato per gestire la durata dello stato suspiciousAlert
    /// </summary>
    protected async void suspiciousTimerLoop() {

        while (!mainBehaviourProcess.processTaskFinished) {
            await Task.Yield();
            if (characterBehaviourStopped) {
                break;
            }

            if (suspiciousTimerEndStateValue == 0) { //indica che il timer loop è stato stoppato
                break;
            }
        }

        suspiciousTimerEndStateValue = Time.time + suspiciousTimerValue;
        while (!simulateSearchingPlayerSubBehaviourProcess.processTaskFinished) {
            await Task.Yield();
            if (characterBehaviourStopped) {
                break;
            }

            if (suspiciousTimerEndStateValue == 0) { //indica che il timer loop è stato stoppato
                break;
            }
        }


        while (Time.time < suspiciousTimerEndStateValue) {
            await Task.Yield();

            if (characterBehaviourStopped) {
                break;
            }
        }

        if (!characterBehaviourStopped) {
            stopAgent();
        }

        if (characterAlertState != CharacterAlertState.HostilityAlert && characterAlertState == CharacterAlertState.SuspiciousAlert) {
            setAlert(CharacterAlertState.Unalert, true);
        }

    }
    /// <summary>
    /// Timer loop usato per gestire la durata dello stato hostilityAlert
    /// </summary>
    protected async void hostilityTimerLoop() {


        while (!mainBehaviourProcess.processTaskFinished) {
            await Task.Yield();

            if (characterBehaviourStopped) {
                break;
            }
            if (hostilityTimerEndStateValue == 0) {
                break;
            }
        }



        hostilityTimerEndStateValue = Time.time + hostilityTimerValue; // setta
        // una volta raggiunta la posizione esaurisci l'[hostilityTimerEndStateValue]
        while (Time.time < hostilityTimerEndStateValue) { 
            await Task.Yield();

            if (characterBehaviourStopped) {
                break;
            }
        }

        if (!characterBehaviourStopped) {
            stopAgent();
        }



        if (characterAlertState == CharacterAlertState.HostilityAlert) {
            setAlert(CharacterAlertState.Unalert, true);
        }
    }
    /// <summary>
    /// Timer loop usato per gestire la durata dello stato suspiciousCorpseFoundAlert
    /// </summary>
    /// <summary>
    /// Timer loop usato per gestire la durata dello stato warnOfSouspicious
    /// </summary>
    protected async void warnOfSouspiciousTimerLoop() {


        while (!mainBehaviourProcess.processTaskFinished) {
            await Task.Yield();
            if (characterBehaviourStopped) {
                break;
            }

            if (warnOfSouspiciousTimerEndStateValue == 0) { //indica che il timer loop è stato stoppato
                break;
            }
        }

        warnOfSouspiciousTimerEndStateValue = Time.time + warningOfSouspiciousTimerValue;
        while (!simulateSearchingPlayerSubBehaviourProcess.processTaskFinished) {
            await Task.Yield();
            if (characterBehaviourStopped) {
                break;
            }

            if (suspiciousTimerEndStateValue == 0) { //indica che il timer loop è stato stoppato
                break;
            }
        }

        while (Time.time < warnOfSouspiciousTimerEndStateValue) {
            await Task.Yield();

            if (characterBehaviourStopped) {
                break;
            }
        }

        if (!characterBehaviourStopped) {
            stopAgent();
        }


        if (characterAlertState == CharacterAlertState.WarnOfSuspiciousAlert) {
            setAlert(CharacterAlertState.Unalert, true);
        }
    }
    protected async void suspiciousCorpseFoundTimerLoop() {


        while (!mainBehaviourProcess.processTaskFinished) {
            await Task.Yield();
            if (characterBehaviourStopped) {
                break;
            }

            if (suspiciousCorpseFoundTimerEndStateValue == 0) { //indica che il timer loop è stato stoppato
                break;
            }
        }

        suspiciousCorpseFoundTimerEndStateValue = Time.time + suspiciousCorpseFoundTimerValue;
        while (Time.time < suspiciousCorpseFoundTimerEndStateValue) {
            await Task.Yield();

            if (characterBehaviourStopped) {
                break;
            }
        }

        if (!characterBehaviourStopped) {
            stopAgent();
        }


        if (characterAlertState == CharacterAlertState.SuspiciousCorpseFoundAlert) {
            setAlert(CharacterAlertState.Unalert, true);
        }
    }
    /// <summary>
    /// Timer loop usato per gestire la durata dello stato corpseFoundConfirmedAlert
    /// </summary>
    protected async void corpseFoundConfirmedTimerLoop() {


        while (!mainBehaviourProcess.processTaskFinished) {

            await Task.Yield();

            if (characterBehaviourStopped) {
                break;
            }
            if (corpseFoundConfirmedTimerEndStateValue == 0) {
                break;
            }
        }


        corpseFoundConfirmedTimerEndStateValue = Time.time + corpseFoundConfirmedTimerValue;
        while (Time.time < corpseFoundConfirmedTimerEndStateValue) {
            await Task.Yield();

            if (characterBehaviourStopped) {
                break;
            }
        }

        if (!characterBehaviourStopped) {
            stopAgent();
        }

        if (characterAlertState == CharacterAlertState.CorpseFoundConfirmedAlert) {
            setAlert(CharacterAlertState.Unalert, true);
        }


    }
    
    /// <summary>
    /// Timer loop usato per gestire la durata dello stato SuspiciousHitReceived
    /// </summary>
    protected async void instantOnCurrentPositionWarnOfSouspiciousTimerLoopAsync() {
        while(!mainBehaviourProcess.processTaskFinished) {

            await Task.Yield();

            if(characterBehaviourStopped) {
                break;
            }
            if(instantOnCurrentPositionWarnOfSouspiciousTimerEndStateValue == 0) {
                break;
            }
        }


        instantOnCurrentPositionWarnOfSouspiciousTimerEndStateValue = Time.time + instantOnCurrentPositionWarnOfSouspiciousTimerValue;
        while(Time.time < instantOnCurrentPositionWarnOfSouspiciousTimerEndStateValue) {
            await Task.Yield();

            if(characterBehaviourStopped) {
                break;
            }

        }

        if(!characterBehaviourStopped) {
            stopAgent();
        }

        if(characterAlertState == CharacterAlertState.instantOnCurrentPositionWarnOfSouspiciousAlert) {
            setAlert(CharacterAlertState.Unalert, true);
        }
    }

    /// <summary>
    /// Timer loop usato per gestire la durata dello stato stayOnPositionSuspicious
    /// </summary>
    protected async void stayOnPositionSuspiciousTimerLoopAsync() {
        while(!mainBehaviourProcess.processTaskFinished) {

            await Task.Yield();

            if(characterBehaviourStopped) {
                break;
            }
            if(playerLoudRunSuspiciousTimerEndStateValue == 0) {
                break;
            }
        }


        playerLoudRunSuspiciousTimerEndStateValue = Time.time + playerLoudRunSuspiciousTimerValue;
        while(Time.time < playerLoudRunSuspiciousTimerEndStateValue) {
            await Task.Yield();

            if(characterBehaviourStopped) {
                break;
            }

            if(playerLoudRunSuspiciousTimerEndStateValue == 0) {
                break;
            }
        }

        if(!characterBehaviourStopped) {
            stopAgent();
        }

        if(characterAlertState == CharacterAlertState.playerLoudRunAlert) {
            setAlert(CharacterAlertState.Unalert, true);
        }
    }
}
