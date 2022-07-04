using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class BehaviourProcess {
    public delegate void Delegate();

    protected BaseNPCBehaviourManager _baseNPCBehaviour;
    protected NavMeshAgent _behaviourAgent;
    protected Vector3 _lastSeenFocusAlarmPosition; // ultima posizione d'allarme comunicata
    protected Delegate onProcessEnd; // evento da eseguire una volta che il processo è completo

    protected string processIdName = "";


    // global states
    protected bool _processTaskFinished = false;
    public bool processTaskFinished {
        get { return _processTaskFinished; }
    }

    /// <summary>
    /// Processo asincrono behaviour
    /// </summary>
    /// <returns></returns>
    public virtual async Task runBehaviourAsyncProcess() {

        //Debug.Log(processIdName);
    }

    /// <summary>
    /// Inizializza behaviour
    /// </summary>
    public virtual void initBehaviourProcess() {

    }


    // Cambia a run time la _lastSeenFocusAlarmPosition
    public virtual void changeCurrentLastSeenFocusAlarmPosition(Vector3 newPos) {
        _lastSeenFocusAlarmPosition = newPos;
    }

    public virtual void resetProcess() {

    }
}
