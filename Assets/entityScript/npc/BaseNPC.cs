using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AlertState {
    Unalert,
    Alert1,
    Alert2,
    Alert3
}


abstract public class BaseNPC : MonoBehaviour
{
    private AlertState alert = AlertState.Unalert;
    public CharacterSpawnPoint spwnPoint;
    Activity npcBaseActivity;

    public void Start() {
        
    }

    void setAlert(AlertState alertState) {
        alert = alertState;
    }

    
    void baseActivity() {

    }

    public void Update() {
        switch(alert) {
            case AlertState.Unalert: {
                baseActivity();
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
    abstract public void alertBehaviour1();
    /// <summary>
    /// comportamento di allerta 2 da implementare nelle classi figlie
    /// </summary>
    abstract public void alertBehaviour2();
    /// <summary>
    /// comportamento di allerta 3 da implementare nelle classi figlie
    /// </summary>
    abstract public void alertBehaviour3();
}
