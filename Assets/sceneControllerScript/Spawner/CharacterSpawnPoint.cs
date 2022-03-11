using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CharacterSpawnPoint : MonoBehaviour {
    public Role characterRole;
    public CharacterSpawnController characterSpawnController;
    public GameObject sceneSpawnGameObject;



    /// <summary>
    /// Rimuove lo spawnPoint dalla lista dei character spawn della lista dell'istanza di CharacterSpawnController ed elimina l'oggetto dalla scena
    /// </summary>
    public void removeSpawnPoint() {
        characterSpawnController.removeCharacterSpawnByGOId(sceneSpawnGameObject.GetInstanceID());
    }


    /// <summary>
    /// Aggiunge e inizializza uno CharacterSpawnPoint component al gameObject
    /// Infine restituisce il gameObject
    /// </summary>
    /// <param name="gameObject">gameObject a cui aggiungere il componente CharacterSpawnPoint</param>
    /// <param name="role">ruolo character da attribuire al CharacterSpawnPoint</param>
    /// <param name="spawnerController">istanza del CharacterSpawnController a cui il CharacterSpawnPoint si assocerà</param>
    /// <returns></returns>
    public static GameObject addToGOCharacterSpawnPointComponent(GameObject gameObject, Role role, CharacterSpawnController spawnerController) {
        gameObject.AddComponent<CharacterSpawnPoint>();

        CharacterSpawnPoint characterSpawn = gameObject.GetComponent<CharacterSpawnPoint>();
        characterSpawn.initCharacterSpawnPointComponent(gameObject, role, spawnerController);

        return gameObject;
    }

    /// <summary>
    /// Metodo per inizializzare un CharacterSpawnPoint 
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="role"></param>
    /// <param name="spawner"></param>
    private void initCharacterSpawnPointComponent(GameObject gameObject, Role role, CharacterSpawnController spawner) {
        characterRole = role;
        characterSpawnController = spawner;
        sceneSpawnGameObject = gameObject;
    }

    void OnDrawGizmos() {
        


        SceneView sceneView = SceneView.lastActiveSceneView;

        // calcola distanza tra la camera e lo spawn point
        float scenViewCameraDistance = Vector3.Distance(sceneView.camera.transform.position, transform.position);
        
        Handles.color = Color.red;
        GUI.color = new Color(1, 0.8f, 0.4f, 1);

        Vector3 pos = transform.position;
        Handles.DrawWireDisc(pos, Vector3.up, 1f);




        // disegna nome dello spawn solo se la distanza
        // tra la camera della scena e l'oggetto è <20
        if (scenViewCameraDistance < 20f) { 
            Handles.Label(
                pos,
                "Spawn\nnemico"
            );
        }

        //gizmos selezionabile solo se la distanza
        // tra la camera della scena e l'oggetto è <10
        if (scenViewCameraDistance < 10f) {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, 0.2f);
        }
        

    }

}