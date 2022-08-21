using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CharacterActivity : MonoBehaviour {

    [SerializeField] private CharacterActivityManager characterActivityManager;
    [SerializeField] private List<ActivityTask> tasks = new List<ActivityTask>();
    [SerializeField] private bool _loopActivity = false; // attività va in modalità loop 

    //getters
    public bool loopActivity {
        get {
            return _loopActivity;
        }
    }

    public bool isLastTask(int taskPos) {
        bool res = false;

        if(taskPos == tasks.Count - 1) {
            res = true;
        } else {
            res = false;
        }

        return res;
    }

    public ActivityTask getTask(int taskPos) {
       return tasks[taskPos];
    }

    public List<ActivityTask> getTasks() {
        return tasks;
    }


    // getter
    public CharacterActivityManager getCharacterActivityManager() {
        return characterActivityManager;
    }
    

    /// <summary>
    /// Aggiunge e inizializza uno CharacterActivity component al gameObject
    /// Infine restituisce il gameObject
    /// </summary>
    /// <param name="gameObject">gameObject a cui aggiungere il componente CharacterActivity</param>
    /// <param name="manager">CharacterActivityManager a cui la CharacterActivity è associata </param>
    /// <returns></returns>
    public static GameObject addToGOCharacterActivityComponent(GameObject gameObject, CharacterActivityManager manager) {
        gameObject.AddComponent<CharacterActivity>();

        CharacterActivity characterActivity = gameObject.GetComponent<CharacterActivity>();
        characterActivity.initCharacterActivityComponent(manager);

        return gameObject;
    }

    private void initCharacterActivityComponent(CharacterActivityManager manager) {
        characterActivityManager = manager;
    }

    /// <summary>
    /// Rimuovi un activity point dalla lista partendo dell'id del gameObject a cui è associato
    /// Viene rimosso dalla lista degli activity point e dalla scena
    /// </summary>
    /// <param name="instanceID"></param>
    public void removeActivityPointByIID(ActivityTask activityTask) {
        for (int i = 0; i < tasks.Count; i++) {


            if (tasks[i].GetInstanceID() == activityTask.GetInstanceID()) {


                GameObject characterActivityGO = tasks[i].gameObject;

                tasks.RemoveAt(i); // rimuovi istanza dalla lista degli activity points
                DestroyImmediate(characterActivityGO); // distruggi gameobject dello spawn dalla scena
            }
        }
    }

    /// <summary>
    /// Rimuove l'activity dalla lista delle activity del characterActivityManager ed elimina l'oggetto dalla scena
    /// </summary>
    public void removeActivity() {
        // rimuovi tutti i task
        for(int i = 0; i < tasks.Count; i++) {
            //tasks[i].removeActivityTask();
        }

        characterActivityManager.removeCharacterActivityByIID(this);
    }



    /// <summary>
    /// Crea una nuova activity point
    /// dopo che viene istanziato viene aggiunto alle lista activityPoints
    /// </summary>
    public GameObject newTask() {

        GameObject newActivityPoint = ActivityTask.addToGOActivityPointComponent(new GameObject(), this); // crea nuovo gameObject e aggiungi componente activity point

        newActivityPoint.name = "Task " + tasks.Count.ToString(); // nome del gameobject

        tasks.Add(newActivityPoint.GetComponent<ActivityTask>());

        newActivityPoint.gameObject.transform.position = transform.position; // setta posizione nuova activity Point


        newActivityPoint.transform.SetParent(gameObject.transform); // setta come figlio del gamecontroller

        return newActivityPoint;
    }

#if UNITY_EDITOR
    void OnDrawGizmos() {



        SceneView sceneView = SceneView.lastActiveSceneView;

        // calcola distanza tra la camera e lo spawn point
        float scenViewCameraDistance = Vector3.Distance(sceneView.camera.transform.position, transform.position);

        Handles.color = Color.green;
        GUI.color = Color.black;

        Vector3 pos = transform.position;




        if (scenViewCameraDistance < 20) {

            Handles.Label(
                pos,
                gameObject.name
            );

        }

        Handles.DrawWireDisc(pos, Vector3.up, 0.5f); // segnalatore spawn

        //gizmos selezionabile solo se la distanza
        // tra la camera della scena e l'oggetto è <10
        if (scenViewCameraDistance < 20f) {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position, 0.10f);
        }

        //gizmos selezionabile solo se la distanza
        // tra la camera della scena e l'oggetto è <100
        if (scenViewCameraDistance < 100f) {

            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, characterActivityManager.gameObject.transform.position);
        }

        //gizmos selezionabile solo se la distanza
        // tra la camera della scena e l'oggetto è <100
        if (scenViewCameraDistance < 100f && tasks.Count > 0) {

            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, tasks[0].gameObject.transform.position);
        }


        if(tasks.Count > 1) {

            Gizmos.color = Color.blue;

            for (int i = 1; i < tasks.Count; i++) {
                
                Gizmos.DrawLine(tasks[i - 1].gameObject.transform.position, tasks[i].gameObject.transform.position);
            }
        }
    }
#endif
}
