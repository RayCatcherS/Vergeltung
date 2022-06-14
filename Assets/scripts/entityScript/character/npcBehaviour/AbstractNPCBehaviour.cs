using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterAlertState {
    Unalert,
    SuspiciousAlert,
    HostilityAlert,
    WarnOfSouspiciousAlert,
    SoundAlert1,
    SoundAlert2
}

public abstract class AbstractNPCBehaviour : MonoBehaviour
{

    /// <summary>
    /// comportamento unalertBehaviour da implementare nelle classi figlie
    /// </summary>
    abstract public void unalertBehaviour();

    /// <summary>
    /// comportamento suspiciousAlertBehaviour da implementare nelle classi figlie
    /// </summary>
    abstract public void suspiciousAlertBehaviour();
    /// <summary>
    /// comportamento HostilityAlertBehaviour da implementare nelle classi figlie
    /// </summary>
    abstract public void hostilityAlertBehaviour();
    /// <summary>
    /// comportamento warnOfSouspiciousAlertBehaviour da implementare nelle classi figlie
    /// </summary>
    abstract public void warnOfSouspiciousAlertBehaviour();
    /// <summary>
    /// comportamento SoundAlert1Behaviour da implementare nelle classi figlie
    /// </summary>
    abstract public void soundAlert1Behaviour();

    /// <summary>
    /// Verifica se un certo Character � sospetto
    /// </summary>
    /// <param name="characterManager">CharacterManager oggetto della verifica</param>
    abstract public void suspiciousCheck(CharacterManager characterManager, Vector3 lastSeenCPosition);

    /// <summary>
    /// Verifica se un certo Character � ostile
    /// </summary>
    /// <param name="characterManager">CharacterManager oggetto della verifica</param>
    abstract public void hostilityCheck(CharacterManager characterManager, Vector3 lastSeenCPosition);

    /// <summary>
    /// Ricevi il warn di un sospetto
    /// </summary>
    /// <param name="lastSeenCPosition"></param>
    abstract public void receiveWarnOfSouspiciousCheck(Vector3 lastSeenCPosition);
}
