using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnController : MonoBehaviour {


    [Header("Character spawnabili")]
    [SerializeField] public Role role;

    [Header("Spawn character nemici")]
    public List<CharacterSpawn> characterSpawns = new List<CharacterSpawn>();



    void Start()
    {
        
    }

    public void newCharacterSpawn(Role characterRole) {
        
        GameObject newCharacterSpawn = CharacterSpawn.addToGOCharacterSpawnComponent(new GameObject(), characterRole, this); // crea nuovo gameObject e inizializza lo spawn

        newCharacterSpawn.name = CharacterRole.GetCharacterRoleName(characterRole) + "Spawn"; // nome del gameobject

        
        characterSpawns.Add(newCharacterSpawn.GetComponent<CharacterSpawn>()); // aggiungi il nuovo spawn alla lista [characterSpawns]

        newCharacterSpawn.transform.SetParent(gameObject.transform); // setta come figlio del gamecontroller
    }

    public void removeCharacterSpawnByGOId(int instanceID) {

        for (int i = 0; i < characterSpawns.Count; i++) {

            if(characterSpawns[i].sceneSpawnGameObject.GetInstanceID() == instanceID) {
                GameObject characterSpawnGO = characterSpawns[i].sceneSpawnGameObject;

                characterSpawns.RemoveAt(i); // rimuovi istanza dalla lista degli spawn dei characters
                DestroyImmediate(characterSpawnGO); // distruggi gameobject dello spawn dalla scena
            }
        }
    }
}
