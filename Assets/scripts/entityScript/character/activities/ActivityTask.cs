using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;

public class ActivityTask : MonoBehaviour
{
    CharacterActivity characterActivity;
    [SerializeField] private Interactable taskEvent;
    [SerializeField] private float taskTiming = 0f; // rappresenta il tempo che gli npc dedicano al task

    /// <summary>
    /// Aggiunge e inizializza uno ActivityPoint component al gameObject
    /// Infine restituisce il gameObject
    /// </summary>
    /// <param name="gameObject">gameObject a cui aggiungere il componente ActivityPoint</param>
    /// <param name="activity">istanza del CharacterActivity a cui il ActivityPoint si assocerà</param>
    /// <returns></returns>
    public static GameObject addToGOActivityPointComponent(GameObject gameObject, CharacterActivity activity) {
        gameObject.AddComponent<ActivityTask>();

        ActivityTask activityPoint = gameObject.GetComponent<ActivityTask>();
        activityPoint.initActivityPointComponent(activity);

        return gameObject;
    }

    private void initActivityPointComponent(CharacterActivity activity) {
        characterActivity = activity;
    }

    public void removeActivityTask() {
        characterActivity.removeActivityPointByIID(this);
    }

    public async Task executeTask(CharacterInteractionManager characterInteraction, CharacterState characterState) {

        characterState.isBusy = true;
        

        if (taskEvent != null) {
            taskEvent.getMainInteracion().getUnityEvent().Invoke(characterInteraction);
        }


        float end = Time.time + taskTiming;
        while (Time.time < end) {
            await Task.Yield();
        }

        

        characterState.isBusy = false;

        // passa al task successivo
        //characterActivity.characterActivityManager.GetComponent<CharacterSpawnPoint>().spwanedNpc.GetComponents<BaseNPCBehaviour>().
    }

#if UNITY_EDITOR
    void OnDrawGizmos() {



        SceneView sceneView = SceneView.lastActiveSceneView;

        // calcola distanza tra la camera e lo spawn point
        float scenViewCameraDistance = Vector3.Distance(sceneView.camera.transform.position, transform.position);

        //gizmos selezionabile solo se la distanza
        // tra la camera della scena e l'oggetto è <10
        if (scenViewCameraDistance < 10f) {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.position, 0.10f);
        }




        //gizmos selezionabile solo se la distanza
        // tra la camera della scena e l'oggetto è <10
        if (scenViewCameraDistance < 40f) {

            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position, new Vector3(0.5f, 0.5f, 0.5f));
        }




    }
#endif
}
