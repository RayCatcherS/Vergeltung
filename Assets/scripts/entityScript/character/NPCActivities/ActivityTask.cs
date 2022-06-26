using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;
using UnityEngine.AI;
using System;

public class ActivityTask : MonoBehaviour
{
    [SerializeField] CharacterActivity characterActivity;
    [SerializeField] private Interactable taskEvent;
    [SerializeField] private float taskTiming = 0f; // rappresenta il tempo che gli npc dedicano al task

    [SerializeField] private Vector3 taskDestination = Vector3.zero;

    public void Start() {
        initTaskDestination();
    }

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

    /// <summary>
    /// Viene eseguito il task attendendo il tempo necessario per portarlo a termine.
    /// Se lo stato di allerta dell'nPCBehaviour che esegue il task è != da CharacterAlertState.Unalert allora
    /// il task viene interrotto immediatamente
    /// </summary>
    /// <param name="character">CharacterManager che avvia l'interaction e che ne subisce l'influenza</param>
    /// <param name="nPCBehaviour">BaseNPCBehaviour viene usato per monitorare lo stato di allerta durante il task</param>
    /// <returns></returns>
    public async Task executeTask(CharacterManager character, BaseNPCBehaviourManager nPCBehaviour, CharacterMovement characterMovement, Action executeDuringTask = null) {

        character.isBusy = true;
        

        if (taskEvent != null) {
            taskEvent.getMainInteraction().getUnityEvent().Invoke(character);
        }




        // setta tempo fine timing
        float end = Time.time + taskTiming;
        while (Time.time < end) {

            if(executeDuringTask != null) {
                executeDuringTask();
            }

            

            // interrompi il task se lo stato di allerta non è più [CharacterAlertState.Unalert]
            if (nPCBehaviour.characterAlertState == CharacterAlertState.Unalert) {
                await Task.Yield();
            } else {

                Debug.Log("Interruzione task, allerta!");
                break;
            }

            if(nPCBehaviour.stopCharacterBehaviour) {
                Debug.Log("Interruzione task, stop character beahviour!");
                break;
            }

            
        }
        


        character.isBusy = false;
        return;

    }


    private void initTaskDestination() {

        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 5.0f, NavMesh.AllAreas)) {

            taskDestination = hit.position;
        }
    }

    public Vector3 getTaskDestination() {
        return taskDestination;
    }

    public Vector2 getTaskDirection() {
       return new Vector2(
            Mathf.Sin((gameObject.transform.eulerAngles.y) * (Mathf.PI / 180)),
            Mathf.Cos((gameObject.transform.eulerAngles.y) * (Mathf.PI / 180))
        );
    }

#if UNITY_EDITOR
    void OnDrawGizmos() {



        SceneView sceneView = SceneView.lastActiveSceneView;

        // calcola distanza tra la camera e lo spawn point
        float scenViewCameraDistance = Vector3.Distance(sceneView.camera.transform.position, transform.position);

        //gizmos selezionabile solo se la distanza
        // tra la camera della scena e l'oggetto è <10
        if (scenViewCameraDistance < 20f) {

            Gizmos.color = Color.blue;

            Gizmos.DrawSphere(transform.position, 0.10f);
        }




        //gizmos selezionabile solo se la distanza
        // tra la camera della scena e l'oggetto è <10
        if (scenViewCameraDistance < 40f) {

            Gizmos.color = Color.blue;

            Gizmos.DrawWireCube(transform.position, new Vector3(1f, 1f, 1f));


            if(taskDestination != Vector3.zero) {
                Gizmos.color = Color.black;
                Gizmos.DrawLine(transform.position, taskDestination);
                Gizmos.DrawSphere(taskDestination, 0.25f);
            }


            // indica la direzione dello spawn
            Handles.DrawLine(
                transform.position,
                transform.position + new Vector3(
                    Mathf.Sin((gameObject.transform.eulerAngles.y) * (Mathf.PI / 180)),
                    0,
                    Mathf.Cos((gameObject.transform.eulerAngles.y) * (Mathf.PI / 180))
                ),
                5
            );
        }

    }
#endif
}
