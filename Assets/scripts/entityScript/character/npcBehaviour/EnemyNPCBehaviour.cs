using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyNPCBehaviour : BaseNPCBehaviour {

    static public GameObject initEnemyNPComponent(GameObject gameObject, CharacterSpawnPoint spwanPoint) {

        EnemyNPCBehaviour enemyNPCNewComponent = gameObject.GetComponent<EnemyNPCBehaviour>();
        enemyNPCNewComponent.initNPCComponent(spwanPoint, gameObject.GetComponent<CharacterMovement>());

        return gameObject;
    }


    /// <summary>
    /// implementazione suspiciousAlertBehaviour
    /// </summary>
    public override void suspiciousAlertBehaviour() {
        stopAgent();
    }
    /// <summary>
    /// implementazione hostilityAlertBehaviour
    /// </summary>
    public override void hostilityAlertBehaviour() {
        stopAgent();
    }
    /// <summary>
    /// implementazione soundAlert1Behaviour
    /// </summary>
    public override void soundAlert1Behaviour() {

    }

    /// <summary>
    /// Per le guardie nemiche quando termina l'HostilityTimer viene aggiornato il dizionario a livello globale
    /// passando il dizionario del character (viene fatta l'unione)
    /// Inoltre tutti gli altri character nemici avranno il dizionario hostility aggiornato
    /// </summary>
    public override void onHostilityTimerEnd() {
        gameObject.GetComponent<CharacterManager>().globalGameState.updateGlobalWantedHostileCharacters(this._wantedHostileCharacters);
    }
}
