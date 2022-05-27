using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterAlertState {
    Unalert,
    SuspiciousAlert,
    HostilityAlert,
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
    /// comportamento SoundAlert1Behaviour da implementare nelle classi figlie
    /// </summary>
    abstract public void soundAlert1Behaviour();

    /// <summary>
    /// Verifica se un certo Character è sospetto
    /// </summary>
    /// <param name="characterManager">CharacterManager oggetto della verifica</param>
    abstract public void suspiciousCheck(CharacterManager characterManager);

    /// <summary>
    /// Verifica se un certo Character è ostile
    /// </summary>
    /// <param name="characterManager">CharacterManager oggetto della verifica</param>
    abstract public void hostilityCheck(CharacterManager characterManager);

    
}
