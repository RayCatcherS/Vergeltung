using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterAlertState {
    Unalert,
    SuspiciousAlert,
    HostilityAlert,
    WarnOfSuspiciousAlert,
    SuspiciousCorpseFoundAlert,
    CorpseFoundConfirmedAlert,
    SuspiciousHitReceivedAlert,
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
    abstract public void hostilityAlertBehaviourAsync();
    /// <summary>
    /// comportamento warnOfSouspiciousAlertBehaviour da implementare nelle classi figlie
    /// </summary>
    abstract public void warnOfSuspiciousAlertBehaviour();
    /// <summary>
    /// comportamento suspiciousCorpseFoundAlertBehaviour da implementare nelle classi figlie
    /// </summary>
    abstract public void suspiciousCorpseFoundAlertBehaviour();
    /// <summary>
    /// comportamento corpseFoundConfirmedAlertBehaviour da implementare nelle classi figlie
    /// </summary>
    abstract public void corpseFoundConfirmedAlertBehaviour();
    /// <summary>
    /// comportamento corpseFoundConfirmedAlertBehaviour da implementare nelle classi figlie
    /// </summary>
    abstract public void soundAlert1Behaviour();
    /// <summary>
    /// comportamento SuspiciousHitReceived da implementare nelle classi figlie
    /// </summary>
    abstract public void suspiciousHitReceivedAlertBehaviour();

    /// <summary>
    /// Verifica se un certo Character è sospetto e quindi entrare nello stato di suspicious
    /// </summary>
    /// <param name="characterManager">CharacterManager oggetto della verifica</param>
    abstract public void suspiciousCheck(CharacterManager characterManager, Vector3 lastSeenCPosition, bool himselfCheck = false);

    /// <summary>
    /// Verifica se un certo Character è ostile e quindi entrare nello stato di hostility
    /// </summary>
    /// <param name="characterManager">CharacterManager oggetto della verifica</param>
    abstract public void hostilityCheck(CharacterManager characterManager, Vector3 lastSeenCPosition, bool himselfCheck = false);

    /// <summary>
    /// ricevi e verifica il warn di un sospetto
    /// </summary>
    /// <param name="lastSeenCPosition"></param>
    abstract public void receiveWarnOfSouspiciousCheck(Vector3 lastSeenCPosition);

    /// <summary>
    /// Verifica se entrare nello stato di "suspiciousCorpseFound"
    /// </summary>
    /// <param name="seenCharacterManager"></param>
    /// <param name="lastSeenCPosition"></param>
    abstract public void suspiciousCorpseFoundCheck(CharacterManager seenDeadCharacter, Vector3 lastSeenCPosition);


    /// <summary>
    /// Verifica se entrare nello stato di "corpseFoundConfirmed"
    /// </summary>
    /// <param name="lastSeenCPosition"></param>
    abstract public void corpseFoundConfirmedCheck(CharacterManager seenDeadCharacter, Vector3 lastSeenCPosition);
    /// <summary>
    /// Verifica se entrare nello stato di "suspiciousHitReceived"
    /// </summary>
    /// <param name="lastSeenCPosition"></param>
    abstract public void suspiciousHitReceivedCheck();
}
