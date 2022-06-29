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
    public override async void suspiciousCorpseFoundAlertBehaviour() {
        await mainBehaviourProcess.runBehaviourAsyncProcess();
    }
    public override async void corpseFoundConfirmedAlertBehaviour() {
        await mainBehaviourProcess.runBehaviourAsyncProcess();
    }
    public override async void suspiciousHitReceivedAlertBehaviour() {
        await mainBehaviourProcess.runBehaviourAsyncProcess();
    }





    protected override void startSuspiciousTimer() {

        // start behaviour process
        mainBehaviourProcess = new GenericSuspiciousProcess(_agent, this);
        simulateSearchingPlayerSubBehaviourProcess
            = new MoveNPCBetweenRandomPointsProcess(agent, this, characterManager, 7, 4, 1);

        suspiciousTimerLoop();
    }
    protected override void startHostilityTimer(bool checkedByHimself) {

        // start behaviour process
        mainBehaviourProcess = new HostilityCivilianProcess(_agent, this, _characterFOV, _characterManager, checkedByHimself);

        hostilityTimerLoop();
    }
    protected override void startSuspiciousCorpseFoundTimer() {
        
        mainBehaviourProcess = new GenericSuspiciousCorpseFoundProcess(_agent, this);

        suspiciousCorpseFoundTimerLoop();
    }
    protected override void startCorpseFoundConfirmedTimer() {

        mainBehaviourProcess = new CorpseFoundConfirmedCivilianProcess(_agent, this, _characterFOV, _characterManager);

        corpseFoundConfirmedTimerLoop();
    }
    protected override void startSuspiciousHitReceivedTimer() {

        mainBehaviourProcess = new MoveNPCBetweenRandomPointsProcess(
            agent, this, characterManager,
            areaRadius: 4,
            sampleToReach: 5,
            waitingOnPointTime: 0.5f
        );
        mainBehaviourProcess.initBehaviourProcess();

        suspiciousHitReceivedTimerLoopAsync();
    }


    protected override void resetSuspiciousBehaviour() {
        mainBehaviourProcess = new GenericSuspiciousProcess(_agent, this);
        simulateSearchingPlayerSubBehaviourProcess
            = new MoveNPCBetweenRandomPointsProcess(agent, this, characterManager, 7, 4, 1);

        // reset time of loop
        suspiciousTimerEndStateValue = Time.time + suspiciousTimerValue;
    }
    protected override void resetHostilityBehaviour() {

        hostilityTimerEndStateValue = Time.time + hostilityTimerValue;
    }



    /// <summary>
    /// Timer loop usato per gestire la durata dello stato suspiciousAlert
    /// </summary>
    protected override async void suspiciousTimerLoop() {

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
    protected override async void hostilityTimerLoop() {


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
    protected override async void suspiciousCorpseFoundTimerLoop() {


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
    protected override async void corpseFoundConfirmedTimerLoop() {


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
    protected override async void suspiciousHitReceivedTimerLoopAsync() {
        while (!mainBehaviourProcess.processTaskFinished) {

            await Task.Yield();

            if (characterBehaviourStopped) {
                break;
            }
            if (suspiciousHitReceivedTimerEndStateValue == 0) {
                break;
            }
        }


        suspiciousHitReceivedTimerEndStateValue = Time.time + suspiciousHitReceivedTimerValue;
        while (Time.time < suspiciousHitReceivedTimerEndStateValue) {
            await Task.Yield();

            if (characterBehaviourStopped) {
                break;
            }
        }

        if (!characterBehaviourStopped) {
            stopAgent();
        }

        if (characterAlertState == CharacterAlertState.SuspiciousHitReceivedAlert) {
            setAlert(CharacterAlertState.Unalert, true);
        }
    }
}
