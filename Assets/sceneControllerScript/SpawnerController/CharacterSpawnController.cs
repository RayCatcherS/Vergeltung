using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public enum Equipment {
    noWeaponEquipment,
    baseArmyWeaponEquipment,
    mediumArmyWeaponEquipment,
    advancedArmyWeaponEquipment
}

public class CharacterSpawnController : MonoBehaviour {

    [Header("Assets character Player")]
    [SerializeField] GameObject playerCharacterAsset;

    [Header("Assets character Enemy")]
    [SerializeField] GameObject enemyCharacterAsset;

    [Header("Assets character Civilian")]
    [SerializeField] GameObject civilianCharacterAsset;

    [Header("Spawn player")]
    [SerializeField] private CharacterSpawnPoint playerSpawnPoint;
    [SerializeField] private bool _playerSpawnPointSetted = false;

    


    [Header("Spawn character nemici")]
    public List<CharacterSpawnPoint> enemyCharacterSpawnPoints = new List<CharacterSpawnPoint>();

    [Header("Spawn character civili")]
    public List<CharacterSpawnPoint> civilianCharacterSpawnPoints = new List<CharacterSpawnPoint>();

    [Header("Prefab character equipment")]
    [SerializeField] private List<GameObject> noWeaponEquipment = new List<GameObject>();
    [SerializeField] private List<GameObject> baseArmyWeaponEquipment = new List<GameObject>();
    [SerializeField] private List<GameObject> mediumArmyWeaponEquipment = new List<GameObject>();
    [SerializeField] private List<GameObject> advancedArmyWeaponEquipment = new List<GameObject>();

    [Header("Character spawnabili")]
    [SerializeField] public Role role;


    /// <summary>
    /// flag che rappresenta se lo spawn point del player(utente) � stato posizionato
    /// </summary>
    public bool playerSpawnPointSetted {
        get { return _playerSpawnPointSetted; }
    }



    void Start()
    {
        spawnCharacters();
        spawnPlayerAsync();
    }


    public void removePlayerSpawnPoint() {
        DestroyImmediate(playerSpawnPoint.gameObject);
        playerSpawnPoint = null;
        _playerSpawnPointSetted = false;
    }

    public GameObject addPlayerSpawnPoint() {
        GameObject newPlayerSpawn = CharacterSpawnPoint.addToGOCharacterSpawnPointComponent(new GameObject(), Role.Player, this); // crea nuovo gameObject e inizializza lo spawn
        newPlayerSpawn.name = CharacterRole.GetCharacterRoleName(Role.Player) + "SpawnPoint"; // nome del gameobject

        playerSpawnPoint = newPlayerSpawn.GetComponent<CharacterSpawnPoint>(); // assegna ref dello spawn del player al Controller
        newPlayerSpawn.transform.SetParent(gameObject.transform); // setta come figlio del gamecontroller
        _playerSpawnPointSetted = true;

        return newPlayerSpawn;
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


        if(characterRole == Role.EnemyGuard) {
            enemyCharacterSpawnPoints.Add(newCharacterSpawn.GetComponent<CharacterSpawnPoint>()); // aggiungi il nuovo spawn alla lista

            newCharacterSpawn.name = newCharacterSpawn.name + enemyCharacterSpawnPoints.Count;

        } else if(characterRole == Role.Civilian) {
            civilianCharacterSpawnPoints.Add(newCharacterSpawn.GetComponent<CharacterSpawnPoint>()); // aggiungi il nuovo spawn alla lista

            newCharacterSpawn.name = newCharacterSpawn.name + civilianCharacterSpawnPoints.Count;
        }
        

        newCharacterSpawn.transform.SetParent(gameObject.transform); // setta come figlio del gamecontroller

        return newCharacterSpawn;
    }

    /// <summary>
    /// Rimuovi un CharacterSpawnPoint partendo dell'id del gameObject a cui � associato
    /// Viene rimosso dalla lista dei characterSpawnPoints e dalla scena
    /// </summary>
    /// <param name="instanceID"></param>
    public void removeCharacterSpawnByGOId(int instanceID, Role characterRole) {


        if(characterRole == Role.EnemyGuard) {
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
        } else if(characterRole == Role.Player) {
            removePlayerSpawnPoint();
        }
    }

    /// <summary>
    /// Il metodo spawna i characters a partire dalla lista di characterSpawnPoints
    /// Aggiungi il ruolo stabilito nello spawn point
    /// Aggiungi il comportamento(NPCBehaviour) stabilito sempre dal ruolo dello spawn point
    /// </summary>
    void spawnCharacters() {

        // spawn character nemici
        for(int i = 0; i < enemyCharacterSpawnPoints.Count; i++) {

            if(enemyCharacterSpawnPoints[i].gameObject.activeSelf == true) {
                // istanzia nella scena
                GameObject newCharacter = Instantiate(enemyCharacterAsset, enemyCharacterSpawnPoints[i].transform);


                CharacterRole.addToGOCharacterRoleComponent(newCharacter, enemyCharacterSpawnPoints[i].getSpawnCharacterRole()); // aggiungi componente ruolo character npc
                EnemyNPCBehaviourManager.initEnemyNPComponent(newCharacter, enemyCharacterSpawnPoints[i]); // aggiungi componente EnemyNPCBehaviour all'npc(comportamento npc)


                InteractionUIController interactionUIController = gameObject.GetComponent<InteractionUIController>();
                GameState gameState = gameObject.GetComponent<GameState>();
                PlayerWarpController playerWarpController = gameObject.GetComponent<PlayerWarpController>();
                SceneEntitiesController sceneEntitiesController = gameObject.GetComponent<SceneEntitiesController>();
                CharacterManager.initCharacterManagerComponent(
                    newCharacter,
                    interactionUIController,
                    gameState,
                    playerWarpController,
                    sceneEntitiesController,
                    enemyCharacterSpawnPoints[i].isTarget
               ); // aggiungi componente CharacterInteraction all'npc(consente di gestire le interazioni dell'npc)


                // associa npc istanziato al componente sceneEntities
                sceneEntitiesController.addNPCEnemyIstance(newCharacter.GetComponent<EnemyNPCBehaviourManager>());

                // inizializza equipaggiamento
                initializeInventory(
                    enemyCharacterSpawnPoints[i].getCharacterEquipment(),
                    newCharacter.GetComponent<CharacterManager>().inventoryManager,
                    enemyCharacterSpawnPoints[i].getStartSelectedEquipment(),
                    enemyCharacterSpawnPoints[i].flashlightTaken,
                    enemyCharacterSpawnPoints[i].weaponPuttedAwayOnStart
                );
            }
            

        }


        // spawn character civili
        for (int i = 0; i < civilianCharacterSpawnPoints.Count; i++) {

            if (civilianCharacterSpawnPoints[i].gameObject.activeSelf == true) {
                // istanzia nella scena
                GameObject newCharacter = Instantiate(civilianCharacterAsset, civilianCharacterSpawnPoints[i].transform);

                
                CharacterRole.addToGOCharacterRoleComponent(newCharacter, civilianCharacterSpawnPoints[i].getSpawnCharacterRole()); // aggiungi componente ruolo character npc
                CivilianNPCBehaviourManager.initCivilianNPCComponent(newCharacter, civilianCharacterSpawnPoints[i]); // aggiungi componente CivilianNPCBehaviour all'npc(comportamento npc)


                InteractionUIController interactionUIController = gameObject.GetComponent<InteractionUIController>();
                GameState gameState = gameObject.GetComponent<GameState>();
                PlayerWarpController playerWarpController = gameObject.GetComponent<PlayerWarpController>();
                SceneEntitiesController sceneEntitiesController = gameObject.GetComponent<SceneEntitiesController>();
                CharacterManager.initCharacterManagerComponent(
                    newCharacter,
                    interactionUIController,
                    gameState,
                    playerWarpController,
                    sceneEntitiesController,
                    civilianCharacterSpawnPoints[i].isTarget
               ); // aggiungi componente CharacterInteraction all'npc(consente di gestire le interazioni dell'npc)


                // aggiungi npc istanziato al componente sceneEntities
                sceneEntitiesController.addNPCCivilianIstance(newCharacter.GetComponent<CivilianNPCBehaviourManager>());

                // inizializza equipaggiamento
                initializeInventory(
                    civilianCharacterSpawnPoints[i].getCharacterEquipment(),
                    newCharacter.GetComponent<CharacterManager>().inventoryManager,
                    civilianCharacterSpawnPoints[i].getStartSelectedEquipment(),
                    civilianCharacterSpawnPoints[i].flashlightTaken,
                    civilianCharacterSpawnPoints[i].weaponPuttedAwayOnStart
                );
            }
            
        }
    }

    private void initializeInventory(Equipment equip, InventoryManager inventoryM, int selectedWeapon, bool flashLightTaken, bool weaponPuttedAway) {
        
        if(equip == Equipment.noWeaponEquipment) {
            for(int i = 0; i < noWeaponEquipment.Count; i++) {
                GameObject weaponGO = Instantiate(noWeaponEquipment[i]);
                WeaponItem weapon = weaponGO.GetComponent<WeaponItem>();
                inventoryM.addWeapon(weapon);
            }
            
        } else if (equip == Equipment.baseArmyWeaponEquipment) {
            for (int i = 0; i < baseArmyWeaponEquipment.Count; i++) {
                GameObject weaponGO = Instantiate(baseArmyWeaponEquipment[i]);
                WeaponItem weapon = weaponGO.GetComponent<WeaponItem>();
                inventoryM.addWeapon(weapon);
            }

        } else if (equip == Equipment.mediumArmyWeaponEquipment) {
            for (int i = 0; i < mediumArmyWeaponEquipment.Count; i++) {
                GameObject weaponGO = Instantiate(mediumArmyWeaponEquipment[i]);
                WeaponItem weapon = weaponGO.GetComponent<WeaponItem>();
                inventoryM.addWeapon(weapon);
            }

        } else if (equip == Equipment.advancedArmyWeaponEquipment) {
            for (int i = 0; i < advancedArmyWeaponEquipment.Count; i++) {
                GameObject weaponGO = Instantiate(advancedArmyWeaponEquipment[i]);
                WeaponItem weapon = weaponGO.GetComponent<WeaponItem>();
                inventoryM.addWeapon(weapon);
            }

        }

        inventoryM.isFlashlightTaken = flashLightTaken;

        inventoryM.selectWeapon(selectedWeapon);

        if(weaponPuttedAway) {
            inventoryM.putAwayWeapon();
        } else {
            inventoryM.extractWeapon();
        }
    }

    async Task spawnPlayerAsync() {

        if (playerSpawnPoint.gameObject.activeSelf == true) {
            // istanzia nella scena
            GameObject newPlayer = Instantiate(playerCharacterAsset, playerSpawnPoint.transform);


            CharacterRole.addToGOCharacterRoleComponent(newPlayer, Role.Player); // aggiungi componente ruolo character npc


            InteractionUIController interactionUIController = gameObject.GetComponent<InteractionUIController>();
            GameState gameState = gameObject.GetComponent<GameState>();
            PlayerWarpController playerWarpController = gameObject.GetComponent<PlayerWarpController>();
            SceneEntitiesController sceneEntitiesController = gameObject.GetComponent<SceneEntitiesController>();
            CharacterManager.initCharacterManagerComponent(
                newPlayer,
                interactionUIController,
                gameState,
                playerWarpController,
                sceneEntitiesController,
                false
            ); // aggiungi componente CharacterInteraction all'npc(consente di gestire le interazioni dell'npc)


            // associa npc istanziato al componente sceneEntities
            sceneEntitiesController.setPlayerEntity(newPlayer.GetComponent<CharacterManager>());


            // aggiungi player al player warp controller
            playerWarpController.addCharacterToWarpStack(newPlayer.GetComponent<CharacterManager>());
            playerWarpController.warpToCharacter(newPlayer.GetComponent<CharacterManager>(), isPlayer: true);


            // inizializza equipaggiamento
            initializeInventory(
                playerSpawnPoint.getCharacterEquipment(),
                newPlayer.GetComponent<CharacterManager>().inventoryManager,
                playerSpawnPoint.getStartSelectedEquipment(),
                playerSpawnPoint.flashlightTaken,
                playerSpawnPoint.weaponPuttedAwayOnStart
            );
        }
    }
}
