using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Questa classe si occupa dell'istanza del componente del character che 
/// gestisce la rilevazione e il riconosciemento degli altri character 
/// </summary>
public class CharacterFOV : MonoBehaviour {
    // const
    private const int CHARACTER_LAYERS = 7;
    private const int ALL_LAYERS = -1; // except glasses ~(1 << 17)
    private const int RAGDOLL_BONE_LAYER = 15;
    private const int SHATTERABLE_GLASS_LAYER = 17;

    [Header("References")]
    [SerializeField] private Transform _recognitionTarget; // target partenza utilizzato per confermare dai campi visivi dei character che il character è stato rilevato
    public Transform recognitionTarget {
        get => _recognitionTarget;
    }

    [SerializeField] private Transform _reachableTarget; // target usato per verificare che il character sia raggiungibile
    public Transform reachableTarget {
        get => _reachableTarget;
    }

    [SerializeField] private BaseNPCBehaviourManager nPCBehaviour; // Reference Behaviour character


    [Header("Component as Colliders References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private CapsuleCollider characterCapsuleCollider;


    [Header("Impostazioni")]
    [SerializeField] private LayerMask targetCharacterMask;
    [SerializeField] [Range(0.1f, 1f)] private float fovCheckFrequency = 0.2f; // frequenza check campi visivi


    [Header("FOV States")]
    [SerializeField] private bool firstFOVCanSeeCharacter = false;
    [SerializeField] private bool secondFOVCanSeeCharacter = false;

    [SerializeField] private bool firstFOVCanSeeDeadCharacter = false;
    [SerializeField] private bool secondFOVCanSeeDeadCharacter = false;


    [Header("Primo campo visivo(ravvicinato)")]
    [SerializeField] private const float _defaultFirstFovRadius = 7f;
    private float defaultFirstFovRadius {
        get { return _defaultFirstFovRadius; }
    }
    [Range(0, 360)]
    [SerializeField] private const float _defaultFirstFovAngle = 90;
    


    [Header("Secondo campo visivo(lontano)")]
    [SerializeField] private float _defaultSecondFovRadius = 18;
    private float defaultSecondFovRadius {
        get { return _defaultSecondFovRadius; }
    }
    [Range(0, 360)]
    [SerializeField] private float _defaultSecondFovAngle = 145;





    [Header("FOV malus values")]
    [Range(0, 360)]
    [SerializeField] private float _firstMalusFovAngle = 60;

    [Range(0, 360)]
    [SerializeField] private float _secondMalusFovAngle = 90;
    [SerializeField] private float _FOVMalusDividerAreasValue = 3.5f;

    [Header("FOV bonus values")]
    [SerializeField] private float _FOVBonusMultiplierFlashLightAreasValue = 1.6f; // valore moltiplicatore fov quando torcia accesa
    [SerializeField] private float _FOVBonusMultiplierAlertAreaValue = 1.4f; // valore moltiplicatore fov quando in stato di allerta


    [Header("Area di allerta")]
    //L'area di allerta viene utilizzata per rilevare i Characters vicini all'NPC e nel caso informarli o aggiornali istantaneamente su eventuali eventi
    [SerializeField] private float _alertArea = 15;
    public float alertArea {
        get { return _alertArea; }
    }


    private Vector3 _unalertSeenCharacter = Vector3.zero; // character guardato durante lo stato di unalert
    public Vector3 unalertSeenCharacter {
        get { return _unalertSeenCharacter; }
    }

    private bool _nightMalus = false;
    private bool _flashLightBonus = false;
    private bool _alertBonus = false;

    public void setNightMalus(bool value) {
        _nightMalus = value;
    }

    public void setFlashLightBonus(bool value) {
        _flashLightBonus = value;
    }

    public void setAlertBonus(bool value) {
        _alertBonus = value;
    }

    public void Start() {
        StartCoroutine(cFOVRoutine());
    }

    private IEnumerator cFOVRoutine() {

        while(true) {
            yield return new WaitForSeconds(fovCheckFrequency);
            if(nPCBehaviour.characterBehaviourStopped) {
                break;
            }

            if(!nPCBehaviour.characterManager.isDead) {
                characterFirstFOVCheck();
                characterSecondFOVCheck();
            } else {
                break;
            }
            
        }
    }

    private float getInfluencedSecondFovRadius() {
        float value = _defaultSecondFovRadius;

        if (_nightMalus) {
            value = _defaultSecondFovRadius / _FOVMalusDividerAreasValue;

            if (_flashLightBonus) {
                value = value * _FOVBonusMultiplierFlashLightAreasValue;
            }
        } else {
            if (_alertBonus) {
                value = value * _FOVBonusMultiplierAlertAreaValue;
            }
        }

        
        return value;
    }
    private float getInfluencedFirstFovRadius() {
        float value = _defaultFirstFovRadius;

        if (_nightMalus) {
            value = _defaultFirstFovRadius / _FOVMalusDividerAreasValue;

            if (_flashLightBonus) {
                value = value * _FOVBonusMultiplierFlashLightAreasValue;
            }
        }
        return value; 
    }
    private float getInfluencedSecondFovAngle() {

        float value = _defaultSecondFovAngle;
        if (_nightMalus) {
            value = _secondMalusFovAngle;
        }

        return value;
    }
    private float getInfluencedFirstFovAngle() {

        float value = _defaultFirstFovAngle;
        if (_nightMalus) {
            value = _firstMalusFovAngle;
        }

        return value;
    }




    /// <summary>
    /// check campo visivo ravvicinato
    /// </summary>
    private void characterFirstFOVCheck() {

        
        Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, getInfluencedFirstFovRadius(), targetCharacterMask);
        bool characterSeen = false;
        bool deadCharacterVisible = false;

        if (hitColliders.Length != 0) {

            foreach(Collider collider in hitColliders) {
                if (collider.gameObject.GetComponent<CharacterManager>().isPlayer) {

                    characterSeen = isCharactersVisibleInFirstFOV(
                        collider.gameObject.GetComponent<CharacterManager>(),
                        collider.gameObject.GetComponent<CharacterFOV>().recognitionTarget.position);


                } else {


                    if (collider.gameObject.GetComponent<CharacterManager>().isDead) {
                        CharacterManager deadCharacterManager = collider.gameObject.GetComponent<CharacterManager>();
                        deadCharacterVisible = isDeadCharactersVisibleInFirstFOV(collider.gameObject.transform.position, deadCharacterManager);
                    }
                }
            }
            
        }

        if(!firstFOVCanSeeCharacter) {
            _unalertSeenCharacter = Vector3.zero;
        }
        

        firstFOVCanSeeDeadCharacter = deadCharacterVisible;
        firstFOVCanSeeCharacter = characterSeen;
    }

    /// <summary>
    /// check campo visivo lontananza
    /// </summary>
    private void characterSecondFOVCheck() {


        Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, getInfluencedSecondFovRadius(), targetCharacterMask);
        bool characterSeen = false;
        bool deadCharacterVisible = false;

        if (hitColliders.Length != 0) {
            
            foreach (Collider collider in hitColliders) {
                if (collider.gameObject.GetComponent<CharacterManager>().isPlayer) {

                    characterSeen = isCharactersVisibleInSecondFOV(
                        collider.gameObject.GetComponent<CharacterManager>(),
                        collider.gameObject.GetComponent<CharacterFOV>().recognitionTarget.position);
                } else {


                    if(collider.gameObject.GetComponent<CharacterManager>().isDead) {
                        CharacterManager deadCharacterManager = collider.gameObject.GetComponent<CharacterManager>();
                        deadCharacterVisible = isDeadCharactersVisibleInSecondFOV(collider.gameObject.transform.position, deadCharacterManager);
                    }
                }
            }

        }

        secondFOVCanSeeDeadCharacter = deadCharacterVisible;
        secondFOVCanSeeCharacter = characterSeen;

    }








    public bool isCharactersVisibleInFirstFOV(CharacterManager characterToSee, Vector3 raycastPositionTargetToReach) {
        bool result = false;
        Vector3 fromPosition = _recognitionTarget.position;
        Vector3 toPosition = raycastPositionTargetToReach;

        Vector3 direction = toPosition - fromPosition;

        if (Vector3.Angle(transform.forward, direction) < (getInfluencedFirstFovAngle() / 2)) {
            //Debug.Log("angle player rilevato");



            disableCharacterCollider();
            RaycastHit hit;
            if (Physics.Raycast(fromPosition, direction, out hit, getInfluencedFirstFovRadius(), ALL_LAYERS, QueryTriggerInteraction.Ignore)) {

                //Debug.Log(hit.collider.gameObject.name);

                CharacterManager seenCharacter = hit.collider.gameObject.GetComponent<CharacterManager>();
                if (hit.collider.gameObject.layer == CHARACTER_LAYERS) {



                    

                    // il character hittato ha lo stesso id del characterToSee
                    if (characterToSee.GetInstanceID() == hit.collider.gameObject.GetComponent<CharacterManager>().GetInstanceID()) {


                        Debug.DrawLine(fromPosition, hit.point, Color.red, fovCheckFrequency);
                        result = true;
                        onFirstFOVCanSeeCharacter(seenCharacter);
                    }
                }

            }
            enableCharacterCollider();
        }
        return result;
    }
    public bool isCharactersVisibleInSecondFOV(CharacterManager characterToSee, Vector3 raycastPositionTargetToReach) {
        bool result = false;
        Vector3 fromPosition = _recognitionTarget.position;
        Vector3 toPosition = raycastPositionTargetToReach;
        Vector3 direction = toPosition - fromPosition;

        if (Vector3.Angle(transform.forward, direction) < (getInfluencedSecondFovAngle() / 2)) {
            //Debug.Log("angle player rilevato");



            disableCharacterCollider();
            RaycastHit hit;
            if (Physics.Raycast(fromPosition, direction, out hit, getInfluencedSecondFovRadius(), ALL_LAYERS, QueryTriggerInteraction.Ignore)) {


                //Debug.Log(hit.collider.gameObject.name);
                CharacterManager seenCharacter = hit.collider.gameObject.GetComponent<CharacterManager>();
                if (hit.collider.gameObject.layer == CHARACTER_LAYERS) {


                    // il character hittato ha lo stesso id del characterToSee
                    if (characterToSee.GetInstanceID() == hit.collider.gameObject.GetComponent<CharacterManager>().GetInstanceID()) {
                        if (!firstFOVCanSeeCharacter) {
                            Debug.DrawLine(fromPosition, hit.point, Color.magenta, fovCheckFrequency);
                        }

                        result = true;
                        onSecondFOVCanSeeCharacter(seenCharacter);
                    }
                }

            }
            enableCharacterCollider();
        }

        return result;
    }

    public bool isDeadCharactersVisibleInSecondFOV(Vector3 raycastPositionTargetToReach, CharacterManager deadCharacter) {
        bool result = false;
        Vector3 fromPosition = _recognitionTarget.position;
        Vector3 toPosition = raycastPositionTargetToReach;
        Vector3 direction = toPosition - fromPosition;

        
        if (Vector3.Angle(transform.forward, direction) < (getInfluencedSecondFovAngle() / 2)) {


            // ottieni i transform per la conferma della visualizzazione del character
            List<Transform> deadCharacterTargetTransforms = deadCharacter.gameObject.GetComponent<RagdollManager>().deadCharacterTargetTransform;

            RaycastHit hit;

            foreach(Transform deadCharacterTarget in deadCharacterTargetTransforms) {


                disableCharacterCollider();
                if (Physics.Linecast(reachableTarget.position, deadCharacterTarget.position, out hit, ~(1 << RAGDOLL_BONE_LAYER), QueryTriggerInteraction.Ignore)) {

                    if (hit.collider != null) {


                    } else {
                        result = true;

                        Debug.DrawLine(reachableTarget.position, hit.point, Color.black, fovCheckFrequency);
                    }
                } else {

                    result = true;

                }
                enableCharacterCollider();
            }


            if (result) {
                onSecondFOVCanSeeDeadCharacter(deadCharacter);
            }
                
        }

        return result;
    }
    public bool isDeadCharactersVisibleInFirstFOV(Vector3 raycastPositionTargetToReach, CharacterManager deadCharacter) {
        bool result = false;
        Vector3 fromPosition = _recognitionTarget.position;
        Vector3 toPosition = raycastPositionTargetToReach;
        Vector3 direction = toPosition - fromPosition;


        if (Vector3.Angle(transform.forward, direction) < (getInfluencedFirstFovAngle() / 2)) {


            // ottieni i transform per la conferma della visualizzazione del character
            List<Transform> deadCharacterTargetTransforms = deadCharacter.gameObject.GetComponent<RagdollManager>().deadCharacterTargetTransform;

            RaycastHit hit;

            foreach (Transform deadCharacterTarget in deadCharacterTargetTransforms) {


                disableCharacterCollider();
                if (Physics.Linecast(reachableTarget.position, deadCharacterTarget.position, out hit, ~(1 << RAGDOLL_BONE_LAYER), QueryTriggerInteraction.Ignore)) {

                    if (hit.collider != null) {

                        //Debug.Log(hit.collider.gameObject.name);
                    } else {
                        result = true;

                        Debug.DrawLine(reachableTarget.position, hit.point, Color.magenta, fovCheckFrequency);
                    }
                } else {

                    result = true;
                }
                enableCharacterCollider();
            }




            if (result) {
                onFirstFOVCanSeeDeadCharacter(deadCharacter);
            }

        }

        return result;
    }










    private void onFirstFOVCanSeeCharacter(CharacterManager seenCharacter) {

        float lastPosX = seenCharacter.getCharacterPositionReachebleByAgents().x;
        float lastPosY = seenCharacter.getCharacterPositionReachebleByAgents().y;
        float lastPosZ = seenCharacter.getCharacterPositionReachebleByAgents().z;
        Vector3 lastPos = new Vector3(lastPosX, lastPosY, lastPosZ);

        // i caracter che entrano nel primo fov verranno fissati se il npcBehaviour è nello stato di unalert
        if(nPCBehaviour.characterAlertState == CharacterAlertState.Unalert) {

            
            float x = seenCharacter.transform.position.x;
            float y = seenCharacter.transform.position.y;
            float z = seenCharacter.transform.position.z;
            _unalertSeenCharacter = new Vector3(x, y, z);
            
        } else {
            _unalertSeenCharacter = Vector3.zero;
        }

        nPCBehaviour.hostilityCheck(seenCharacter, lastPos, true);
    }
    private void onSecondFOVCanSeeCharacter(CharacterManager seenCharacter) {

        if(!firstFOVCanSeeCharacter) {

            float lastPosX = seenCharacter.getCharacterPositionReachebleByAgents().x;
            float lastPosY = seenCharacter.getCharacterPositionReachebleByAgents().y;
            float lastPosZ = seenCharacter.getCharacterPositionReachebleByAgents().z;
            Vector3 lastPos = new Vector3(lastPosX, lastPosY, lastPosZ);

            

            if (nPCBehaviour.characterAlertState == CharacterAlertState.HostilityAlert) {
                nPCBehaviour.hostilityCheck(seenCharacter, lastPos, true);
            } else {
                nPCBehaviour.suspiciousCheck(seenCharacter, lastPos, true);
            }
            
        }
    }
    private void onSecondFOVCanSeeDeadCharacter(CharacterManager seenDeadCharacter) {
        


        if(!firstFOVCanSeeDeadCharacter) {
            float lastPosX = seenDeadCharacter.getCharacterPositionReachebleByAgents().x;
            float lastPosY = seenDeadCharacter.getCharacterPositionReachebleByAgents().y;
            float lastPosZ = seenDeadCharacter.getCharacterPositionReachebleByAgents().z;
            Vector3 lastPos = new Vector3(lastPosX, lastPosY, lastPosZ);

            if (nPCBehaviour.characterAlertState == CharacterAlertState.Unalert) {
                nPCBehaviour.suspiciousCorpseFoundCheck(seenDeadCharacter, lastPos);
            }
                
        }
        


    }
    private void onFirstFOVCanSeeDeadCharacter(CharacterManager seenDeadCharacter) {


        float lastPosX = seenDeadCharacter.getCharacterPositionReachebleByAgents().x;
        float lastPosY = seenDeadCharacter.getCharacterPositionReachebleByAgents().y;
        float lastPosZ = seenDeadCharacter.getCharacterPositionReachebleByAgents().z;
        Vector3 lastPos = new Vector3(lastPosX, lastPosY, lastPosZ);

        if (nPCBehaviour.characterAlertState == CharacterAlertState.Unalert ||
            nPCBehaviour.characterAlertState == CharacterAlertState.SuspiciousCorpseFoundAlert ||
            nPCBehaviour.characterAlertState == CharacterAlertState.WarnOfSuspiciousAlert
            ) {

            
            nPCBehaviour.corpseFoundConfirmedCheck(seenDeadCharacter, lastPos);
        }
    }



    

    /// <summary>
    /// Il metodo controlla se un raggio riesce a raggiunge un altro character
    /// senza incontrare collisioni
    /// </summary>
    /// <returns></returns>
    public bool isCharacterReachableBy(CharacterFOV characterToReach) {
        bool res = false;
        RaycastHit hit;

        disableCharacterCollider();
        if (Physics.Linecast(reachableTarget.position, characterToReach.reachableTarget.position, out hit, ALL_LAYERS, QueryTriggerInteraction.Ignore)) {

            if (hit.collider != null) {
                res = false;

            } else {
                res = true;
            }
        } else {
            res = true;
        }
        enableCharacterCollider();

        return res;
    }

    
    /// <summary>
    /// Ottieni tutti i character nella sua area di allerta.
    /// Da usare per comunicare istantanemente un allarme
    /// </summary>
    /// <returns></returns>
    public Dictionary<int, BaseNPCBehaviourManager> getAlertAreaCharacters() {

        Dictionary<int, BaseNPCBehaviourManager> characters = new Dictionary<int, BaseNPCBehaviourManager>();

        Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, _alertArea, targetCharacterMask);

        if (hitColliders.Length != 0) {

            foreach (Collider collider in hitColliders) {

                if (!collider.gameObject.GetComponent<CharacterManager>().isPlayer) {

                    if (collider.gameObject.GetComponent<CharacterRole>().role == Role.EnemyGuard) {

                        // evita la creazione di doppioni
                        if (!characters.ContainsKey(collider.gameObject.GetComponent<EnemyNPCBehaviourManager>().GetInstanceID()))
                            characters.Add(collider.gameObject.GetComponent<EnemyNPCBehaviourManager>().GetInstanceID(), collider.gameObject.GetComponent<EnemyNPCBehaviourManager>());

                    } else if (collider.gameObject.GetComponent<CharacterRole>().role == Role.Civilian) {

                        // evita la creazione di doppioni
                        if (!characters.ContainsKey(collider.gameObject.GetComponent<CivilianNPCBehaviourManager>().GetInstanceID()))
                            characters.Add(collider.gameObject.GetComponent<CivilianNPCBehaviourManager>().GetInstanceID(), collider.gameObject.GetComponent<CivilianNPCBehaviourManager>());

                    }
                }

            }

        }

        // rimozione character che fa il controllo
        // se stesso non fa parte dei character rilevati dall'alarm area
        if (gameObject.GetComponent<CharacterRole>().role == Role.EnemyGuard) {

            characters.Remove(gameObject.GetComponent<EnemyNPCBehaviourManager>().GetInstanceID());
        } else if (gameObject.GetComponent<CharacterRole>().role == Role.Civilian) {

            characters.Remove(gameObject.GetComponent<CivilianNPCBehaviourManager>().GetInstanceID());
        }


        return characters;
    }


    /// <summary>
    /// metodo che disattiva i collider del character per consentire i check(linecast, raycast)
    /// </summary>
    private void disableCharacterCollider() {
        characterController.enabled = false;
        characterCapsuleCollider.enabled = false;
    }

    /// <summary>
    /// metodo riattiva i collider del character
    /// </summary>
    private void enableCharacterCollider() {
        characterController.enabled = true;
        characterCapsuleCollider.enabled = true;

    }

#if UNITY_EDITOR
    private void drawfirstFOVEditor() {

        
        Handles.color = Color.white;
        Handles.DrawWireDisc(gameObject.transform.position, Vector3.up, getInfluencedFirstFovRadius()); //debug radius fov


        // draw dell'angolo del fov range

        // forward rispetto al character
        Handles.color = Color.white;
        Handles.DrawLine(
            gameObject.transform.position,
            gameObject.transform.position + new Vector3(
                Mathf.Sin((transform.eulerAngles.y) * (Mathf.Deg2Rad)),
                0,
                Mathf.Cos((transform.eulerAngles.y) * (Mathf.Deg2Rad))
            ) * getInfluencedFirstFovRadius(),
            1
        );


        // forward + 20 gradi rispetto al character
        Handles.color = Color.yellow;
        Handles.DrawLine(
            gameObject.transform.position,
            gameObject.transform.position + new Vector3(
                Mathf.Sin((transform.eulerAngles.y + (getInfluencedFirstFovAngle() / 2)) * (Mathf.Deg2Rad)),
                0,
                Mathf.Cos((transform.eulerAngles.y + (getInfluencedFirstFovAngle() / 2)) * (Mathf.Deg2Rad))
            ) * (getInfluencedFirstFovRadius()),
            5
        );

        // forward - 20 gradi rispetto al character
        Handles.color = Color.yellow;
        Handles.DrawLine(
            gameObject.transform.position,
            gameObject.transform.position + new Vector3(
                Mathf.Sin((transform.eulerAngles.y + (-(getInfluencedFirstFovAngle()) / 2)) * (Mathf.Deg2Rad)),
                0,
                Mathf.Cos((transform.eulerAngles.y + (-(getInfluencedFirstFovAngle()) / 2)) * (Mathf.Deg2Rad))
            ) * (getInfluencedFirstFovRadius()),
            5
        );
    }

    private void drawSecondFOVEditor() {


        Handles.color = Color.white;
        Handles.DrawWireDisc(gameObject.transform.position, Vector3.up, getInfluencedSecondFovRadius()); //debug radius fov


        // draw dell'angolo del fov range

        // forward rispetto al character
        Handles.color = Color.white;
        Handles.DrawLine(
            gameObject.transform.position,
            gameObject.transform.position + new Vector3(
                Mathf.Sin((transform.eulerAngles.y) * (Mathf.Deg2Rad)),
                0,
                Mathf.Cos((transform.eulerAngles.y) * (Mathf.Deg2Rad))
            ) * (getInfluencedSecondFovRadius()),
            1
        );


        // forward + 20 gradi rispetto al character
        Handles.color = Color.cyan;
        Handles.DrawLine(
            gameObject.transform.position,
            gameObject.transform.position + new Vector3(
                Mathf.Sin((transform.eulerAngles.y + (getInfluencedSecondFovAngle() / 2)) * (Mathf.Deg2Rad)),
                0,
                Mathf.Cos((transform.eulerAngles.y + (getInfluencedSecondFovAngle() / 2)) * (Mathf.Deg2Rad))
            ) * (getInfluencedSecondFovRadius()),
            5
        );

        // forward - 20 gradi rispetto al character
        Handles.color = Color.cyan;
        Handles.DrawLine(
            gameObject.transform.position,
            gameObject.transform.position + new Vector3(
                Mathf.Sin((transform.eulerAngles.y + (-(getInfluencedSecondFovAngle()) / 2)) * (Mathf.Deg2Rad)),
                0,
                Mathf.Cos((transform.eulerAngles.y + (-(getInfluencedSecondFovAngle()) / 2)) * (Mathf.Deg2Rad))
            ) * (getInfluencedSecondFovRadius()),
            5
        );
    }
    
    private void drawAreaAlert() {
        Handles.color = Color.gray;
        Handles.DrawWireDisc(gameObject.transform.position, Vector3.up, _alertArea);
    }

    void OnDrawGizmos() {
        if(!gameObject.GetComponent<CharacterManager>().isPlayer) {

            // debugga campi visivi solo se i character rilevano altri character player
            if(firstFOVCanSeeCharacter || secondFOVCanSeeCharacter) {
                drawfirstFOVEditor();
                drawSecondFOVEditor();
                drawAreaAlert();
            }
        }
    }

    void OnDrawGizmosSelected() {
        if (!gameObject.GetComponent<CharacterManager>().isPlayer) {

            drawfirstFOVEditor();
            drawSecondFOVEditor();
            drawAreaAlert();
        }
    }
#endif
}
