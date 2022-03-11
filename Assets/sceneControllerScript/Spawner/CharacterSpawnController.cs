using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSpawnController : MonoBehaviour {

    [Header("Assets characters")]
    [SerializeField] GameObject enemycharactersAsset;

    [Header("Spawn character nemici")]
    public List<CharacterSpawnPoint> characterSpawnPoints = new List<CharacterSpawnPoint>();

    [Header("Character spawnabili")]
    [SerializeField] public Role role;

    void Start()
    {
        spawnCharacters();
    }

    /// <summary>
    /// Crea un nuovo characterSpawnPoint
    /// dopo che viene istanziato viene aggiunto alla lista CharacterSpawnPoint
    /// </summary>
    /// <param name="characterRole">Ruolo del character da istanziare</param>
    public GameObject newCharacterSpawnPoint(Role characterRole) {
        
        GameObject newCharacterSpawn = CharacterSpawnPoint.addToGOCharacterSpawnPointComponent(new GameObject(), characterRole, this); // crea nuovo gameObject e inizializza lo spawn

        newCharacterSpawn.name = CharacterRole.GetCharacterRoleName(characterRole) + "SpawnPoint"; // nome del gameobject

        
        characterSpawnPoints.Add(newCharacterSpawn.GetComponent<CharacterSpawnPoint>()); // aggiungi il nuovo spawn alla lista [characterSpawns]

        newCharacterSpawn.transform.SetParent(gameObject.transform); // setta come figlio del gamecontroller

        return newCharacterSpawn;
    }

    /// <summary>
    /// Rimuovi un CharacterSpawnPoint partendo dell'id del gameObject a cui è associato
    /// Viene rimosso dalla lista dei characterSpawnPoints e dalla scena
    /// </summary>
    /// <param name="instanceID"></param>
    public void removeCharacterSpawnByGOId(int instanceID) {

        for (int i = 0; i < characterSpawnPoints.Count; i++) {

            if(characterSpawnPoints[i].sceneSpawnGameObject.GetInstanceID() == instanceID) {
                GameObject characterSpawnGO = characterSpawnPoints[i].sceneSpawnGameObject;

                characterSpawnPoints.RemoveAt(i); // rimuovi istanza dalla lista degli spawn dei characters
                DestroyImmediate(characterSpawnGO); // distruggi gameobject dello spawn dalla scena
            }
        }
    }

    /// <summary>
    /// Il metodo spawna i characters a partire dalla lista di characterSpawnPoints
    /// </summary>
    void spawnCharacters() {
        for(int i = 0; i < characterSpawnPoints.Count; i++) {

            GameObject newCharacter = enemycharactersAsset;

            
            Instantiate(enemycharactersAsset, characterSpawnPoints[i].transform);
        }
    }
}
