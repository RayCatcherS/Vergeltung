using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class BehaviourProcess {
    protected BaseNPCBehaviourManager _baseNPCBehaviour;
    protected NavMeshAgent _behaviourAgent;

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
}
