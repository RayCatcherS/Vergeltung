using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AlertState {
    Unalert,
    Alert1,
    Alert2,
    Alert3
}

public abstract class AbstractNPCBehaviour : MonoBehaviour
{

    /// <summary>
    /// comportamento di allerta 1 da implementare nelle classi figlie
    /// </summary>
    abstract public void unalertBehaviour1();

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
