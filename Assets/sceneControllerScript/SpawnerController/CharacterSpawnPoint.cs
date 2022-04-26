using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CharacterSpawnPoint : MonoBehaviour {
    [Header("Character ref")]
    [SerializeField] private CharacterSpawnController characterSpawnController;

    [Header("Character config")]
    [SerializeField] private Role characterRole;

    [Header("Character equipment config")]
    [SerializeField] private Equipment characterEquipment;
    [SerializeField] private int startSelectedEquipment = 0;
    

    // getter
    public Role getSpawnCharacterRole() {
        return this.characterRole;
    }
    public Equipment getCharacterEquipment() {
        return this.characterEquipment;
    }

    public int getStartSelectedEquipment() {
        return this.startSelectedEquipment;
    }


    /// <summary>
    /// Rimuove lo spawnPoint dalla lista dei character spawn della lista dell'istanza di CharacterSpawnController ed elimina l'oggetto dalla scena
    /// </summary>
    public void removeSpawnPoint() {

        // rimuovi e cancella tutte le character activity dallo spawner
        List<CharacterActivity> characterActivities = gameObject.GetComponent<CharacterActivityManager>().getCharacterActivities();
        for (int i = 0; i < characterActivities.Count; i++) {
            characterActivities[i].removeActivity();
        }

        characterSpawnController.removeCharacterSpawnByGOId(
            gameObject.GetInstanceID(),
            characterRole);
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
        characterSpawn.initCharacterSpawnPointComponent(role, spawnerController);

        return gameObject;
    }

    /// <summary>
    /// Metodo per inizializzare un CharacterSpawnPoint 
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="role"></param>
    /// <param name="spawner"></param>
    private void initCharacterSpawnPointComponent(Role role, CharacterSpawnController spawner) {
        characterRole = role;
        characterSpawnController = spawner;

        // add character activity manager
        gameObject.AddComponent<CharacterActivityManager>();
    }

#if UNITY_EDITOR
    void OnDrawGizmos() {
        


        SceneView sceneView = SceneView.lastActiveSceneView;

        // calcola distanza tra la camera e lo spawn point
        float scenViewCameraDistance = Vector3.Distance(sceneView.camera.transform.position, transform.position);
        
        Handles.color = Color.red;
        GUI.color = new Color(1, 0.8f, 0.4f, 1);

        Vector3 pos = transform.position;

        if (characterRole == Role.Enemy) {

            Handles.color = Color.red;
            
        } else if (characterRole == Role.Civilian) {
            Handles.color = Color.blue;

        } else if (characterRole == Role.Player) {
            Handles.color = Color.yellow;

        }



        // disegna nome dello spawn solo se la distanza
        // tra la camera della scena e l'oggetto è <20
        if (scenViewCameraDistance < 20f) { 

            if(characterRole == Role.Enemy) {

                Handles.Label(
                    pos,
                    "Spawn\nnemico"
                );
            } else if(characterRole == Role.Civilian) {
                Handles.Label(
                    pos,
                    "Spawn\ncivile"
                );
            } else if (characterRole == Role.Player) {
                Handles.Label(
                    pos,
                    "Spawn\nPlayer"
                );

            }

        }
        Handles.DrawLine(
            pos, 
            pos + new Vector3(
                Mathf.Sin((gameObject.transform.eulerAngles.y) * (Mathf.PI / 180)),
                0, 
                Mathf.Cos((gameObject.transform.eulerAngles.y) * (Mathf.PI / 180))
            ),
            5
        ); // indica la direzione dello spawn



        Handles.DrawWireDisc(pos, Vector3.up, 1f); // segnalatore spawn

        //gizmos selezionabile solo se la distanza
        // tra la camera della scena e l'oggetto è <10
        if (scenViewCameraDistance < 20f) {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, 0.2f);
        }
        

    }
#endif

}