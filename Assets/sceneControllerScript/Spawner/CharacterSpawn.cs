using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSpawn : MonoBehaviour {
    public Role characterRole;
    public SpawnController spawnerController;
    public GameObject sceneSpawnGameObject;

    public CharacterSpawn(Role role, SpawnController spawnerController) {
        this.characterRole = role;
        this.spawnerController = spawnerController;
    }

    

    /// <summary>
    /// Rimuove lo spawn dalla lista dei character spawn dello spawnController ed elimina l'oggetto dalla scena
    /// </summary>
    public void removeSpawn() {
        spawnerController.removeCharacterSpawnByGOId(sceneSpawnGameObject.GetInstanceID());
    }


    public static GameObject addToGOCharacterSpawnComponent(GameObject gameObject, Role role, SpawnController spawner) {
        gameObject.AddComponent<CharacterSpawn>();

        CharacterSpawn characterSpawn = gameObject.GetComponent<CharacterSpawn>();
        characterSpawn.initSpawn(gameObject, role, spawner);

        return gameObject;
    }
    private void initSpawn(GameObject gameObject, Role role, SpawnController spawner) {
        characterRole = role;
        spawnerController = spawner;
        sceneSpawnGameObject = gameObject;
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.5f);
    }
}