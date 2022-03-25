using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSpawnController : MonoBehaviour {

    [Header("Assets character enemy")]
    [SerializeField] GameObject enemyCharacterAsset;

    [Header("Assets character civilian")]
    [SerializeField] GameObject civilianCharacterAsset;

    [Header("Spawn character nemici")]
    public List<CharacterSpawnPoint> enemyCharacterSpawnPoints = new List<CharacterSpawnPoint>();

    [Header("Spawn character civili")]
    public List<CharacterSpawnPoint> civilianCharacterSpawnPoints = new List<CharacterSpawnPoint>();

    [Header("Character spawnabili")]
    [SerializeField] public Role role;

    void Start()
    {
        spawnCharacters();
    }

    /// <summary>
    /// Crea un nuovo characterSpawnPoint
    /// dopo che viene istanziato viene aggiunto alle liste spawn in base al ruolo
    /// esempio: enemyCharacterSpawnPoints
    /// </summary>
    /// <param name="characterRole">Ruolo del character da istanziare</param>
    public GameObject newCharacterSpawnPoint(Role characterRole) {
        
        GameObject newCharacterSpawn = CharacterSpawnPoint.addToGOCharacterSpawnPointComponent(new GameObject(), characterRole, this); // crea nuovo gameObject e inizializza lo spawn

        newCharacterSpawn.name = CharacterRole.GetCharacterRoleName(characterRole) + "SpawnPoint"; // nome del gameobject


        if(characterRole == Role.Enemy) {
            enemyCharacterSpawnPoints.Add(newCharacterSpawn.GetComponent<CharacterSpawnPoint>()); // aggiungi il nuovo spawn alla lista

        } else if(characterRole == Role.Civilian) {
            civilianCharacterSpawnPoints.Add(newCharacterSpawn.GetComponent<CharacterSpawnPoint>()); // aggiungi il nuovo spawn alla lista
        }
        

        newCharacterSpawn.transform.SetParent(gameObject.transform); // setta come figlio del gamecontroller

        return newCharacterSpawn;
    }

    /// <summary>
    /// Rimuovi un CharacterSpawnPoint partendo dell'id del gameObject a cui è associato
    /// Viene rimosso dalla lista dei characterSpawnPoints e dalla scena
    /// </summary>
    /// <param name="instanceID"></param>
    public void removeCharacterSpawnByGOId(int instanceID, Role characterRole) {


        if(characterRole == Role.Enemy) {
            for (int i = 0; i < enemyCharacterSpawnPoints.Count; i++) {

                if (enemyCharacterSpawnPoints[i].gameObject.GetInstanceID() == instanceID) {
                    GameObject characterSpawnGO = enemyCharacterSpawnPoints[i].gameObject;

                    enemyCharacterSpawnPoints.RemoveAt(i); // rimuovi istanza dalla lista degli spawn dei characters
                    DestroyImmediate(characterSpawnGO); // distruggi gameobject dello spawn dalla scena
                }
            }
        } else if(characterRole == Role.Civilian) {
            for (int i = 0; i < civilianCharacterSpawnPoints.Count; i++) {

                if (civilianCharacterSpawnPoints[i].gameObject.GetInstanceID() == instanceID) {
                    GameObject characterSpawnGO = civilianCharacterSpawnPoints[i].gameObject;

                    civilianCharacterSpawnPoints.RemoveAt(i); // rimuovi istanza dalla lista degli spawn dei characters
                    DestroyImmediate(characterSpawnGO); // distruggi gameobject dello spawn dalla scena
                }
            }
        }
    }

    /// <summary>
    /// Il metodo spawna i characters a partire dalla lista di characterSpawnPoints
    /// Aggiungi il ruolo stabilito nello spawn point
    /// Aggiungi il comportamento(NPCBehaviour) stabilito sempre dal ruolo dello spawn point
    /// </summary>
    void spawnCharacters() {

        SceneEntitiesController sceneEntities = gameObject.GetComponent<SceneEntitiesController>();

        // spawn character nemici
        for(int i = 0; i < enemyCharacterSpawnPoints.Count; i++) {


            // istanzia nella scena
            GameObject newCharacter = Instantiate(enemyCharacterAsset, enemyCharacterSpawnPoints[i].transform);


            CharacterRole.addToGOCharacterRoleComponent(newCharacter, enemyCharacterSpawnPoints[i].getSpawnCharacterRole()); // aggiungi componente ruolo character npc
            EnemyNPCBehaviour.addToGOEnemyNPComponent(newCharacter, enemyCharacterSpawnPoints[i]); // aggiungi componente EnemyNPCBehaviour all'npc(comportamento npc)


            InteractionUIController interactionUIController = gameObject.GetComponent<InteractionUIController>();
            CharacterInteractionManager.addToGOCharacterInteractionComponent(newCharacter, interactionUIController); // aggiungi componente CharacterInteraction all'npc(consente di gestire le interazioni dell'npc)


            // associa npc istanziato al componente sceneEntities
            SceneEntitiesController sceneEntitiesController = gameObject.GetComponent<SceneEntitiesController>();
            sceneEntitiesController.addNPCEnemyIstance(newCharacter.GetComponent<EnemyNPCBehaviour>());

        }


        // spawn character civili
        for (int i = 0; i < civilianCharacterSpawnPoints.Count; i++) {


            // istanzia nella scena
            GameObject newCharacter = Instantiate(civilianCharacterAsset, civilianCharacterSpawnPoints[i].transform);


            CharacterRole.addToGOCharacterRoleComponent(newCharacter, civilianCharacterSpawnPoints[i].getSpawnCharacterRole()); // aggiungi componente ruolo character npc
            CivilianNPCBehaviour.addToGOCivilianNPCComponent(newCharacter, civilianCharacterSpawnPoints[i]); // aggiungi componente CivilianNPCBehaviour all'npc(comportamento npc)


            InteractionUIController interactionUIController = gameObject.GetComponent<InteractionUIController>();
            CharacterInteractionManager.addToGOCharacterInteractionComponent(newCharacter, interactionUIController); // aggiungi componente CharacterInteraction all'npc(consente di gestire le interazioni dell'npc)


            // aggiungi npc istanziato al componente sceneEntities
            SceneEntitiesController sceneEntitiesController = gameObject.GetComponent<SceneEntitiesController>();
            sceneEntitiesController.addNPCEnemyIstance(newCharacter.GetComponent<EnemyNPCBehaviour>());
        }
    }
}
