using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Comportamento dell'npc base classe padre, implementazione astrazione AbstractNPCBehaviour
/// </summary>
public class BaseNPCBehaviour : AbstractNPCBehaviour {
    protected AlertState alert = AlertState.Unalert;
    protected CharacterSpawnPoint spawnPoint; // gli spawn point contengono le activities che l'NPC dovrà eseguire

    public void Start() {
        
    }

    void setAlert(AlertState alertState) {
        alert = alertState;
    }


    public override void unalertBehaviour1() {

    }

    public void Update() {
        switch(alert) {
            case AlertState.Unalert: {
                unalertBehaviour1();
            }
            break;
            case AlertState.Alert1: {
                alertBehaviour1();
            }
            break;
            case AlertState.Alert2: {
                alertBehaviour2();
            }
            break;
            case AlertState.Alert3: {
                alertBehaviour3();
            }
            break;
        }
    }

    /// <summary>
    /// comportamento di allerta 1 da implementare nelle classi figlie
    /// </summary>
    public override void alertBehaviour1() {

    }
    /// <summary>
    /// comportamento di allerta 2 da implementare nelle classi figlie
    /// </summary>
    public override void alertBehaviour2() {

    }
    /// <summary>
    /// comportamento di allerta 3 da implementare nelle classi figlie
    /// </summary>
    public override void alertBehaviour3() {

    }
}
