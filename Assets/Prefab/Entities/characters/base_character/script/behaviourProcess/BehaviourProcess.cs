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
    protected Delegate onProcessEnd; // evento da eseguire una volta che il processo � completo

    protected string processIdName = "";


    // global states
    protected bool _processTaskFinished = false;
    public bool processTaskFinished {
        get { return _processTaskFinished; }
    }

    /// <summary>
    /// Verifica che il processo possa ancora girare: Play attivo e componenti non distrutti.
    /// Da controllare dopo ogni await per evitare che una continuazione async acceda a
    /// oggetti distrutti all'uscita dal Play (MissingReferenceException).
    /// </summary>
    protected bool isProcessAlive() {
        return Application.isPlaying && _baseNPCBehaviour != null && _behaviourAgent != null;
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
