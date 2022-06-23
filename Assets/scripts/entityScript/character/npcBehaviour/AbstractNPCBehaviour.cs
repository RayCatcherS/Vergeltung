using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterAlertState {
    Unalert,
    SuspiciousAlert,
    HostilityAlert,
    WarnOfSuspiciousAlert,
    SuspiciousCorpseFoundAlert,
    SuspiciousCorpseFoundConfirmedAlert,
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

    abstract public void suspiciousCorpseFoundAlertBehaviour();

    /// <summary>
    /// comportamento SoundAlert1Behaviour da implementare nelle classi figlie
    /// </summary>
    abstract public void soundAlert1Behaviour();

    /// <summary>
    /// Verifica se un certo Character è sospetto
    /// </summary>
    /// <param name="characterManager">CharacterManager oggetto della verifica</param>
    abstract public void suspiciousCheck(CharacterManager characterManager, Vector3 lastSeenCPosition, bool himselfCheck = false);

    /// <summary>
    /// Verifica se un certo Character è ostile
    /// </summary>
    /// <param name="characterManager">CharacterManager oggetto della verifica</param>
    abstract public void hostilityCheck(CharacterManager characterManager, Vector3 lastSeenCPosition, bool himselfCheck = false);

    /// <summary>
    /// ricevi e verifica il warn di un sospetto
    /// </summary>
    /// <param name="lastSeenCPosition"></param>
    abstract public void receiveWarnOfSouspiciousCheck(Vector3 lastSeenCPosition);

    /// <summary>
    /// Verifica se avviare entrare nello stato di "suspiciousCorpseFound"
    /// </summary>
    /// <param name="seenCharacterManager"></param>
    /// <param name="lastSeenCPosition"></param>
    abstract public void suspiciousCorpseFoundCheck(CharacterManager seenCharacterManager, Vector3 lastSeenCPosition);
}
