using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Questa classe si occupa dell'istanza del componente del character che 
/// gestisce la rilevazione e il riconosciemento degli altri character 
/// </summary>
public class CharacterFOV : MonoBehaviour
{
    // const
    private const int CHARACTER_LAYERS = 7;
    private const int ALL_LAYERS = -1;
    private const int RAGDOLL_BONE = 15;

    [Header("References")]
    [SerializeField] private Transform _recognitionTarget; // target partenza utilizzato per confermare dai campi visivi dei character che il character è stato rilevato
    public Transform recognitionTarget {
        get => _recognitionTarget;
    }

    [SerializeField] private Transform _reachableTarget; // target usato per verificare che il character sia raggiungibile
    public Transform reachableTarget {
        get => _reachableTarget;
    }

    [SerializeField] private BaseNPCBehaviour nPCBehaviour; // Reference Behaviour character

    [Header("Impostazioni")]
    [SerializeField] private LayerMask targetCharacterMask;
    [SerializeField] [Range(0.1f, 1f)] private float fovCheckFrequency = 0.2f; // frequenza check campi visivi
    


    [Header("Primo campo visivo(ravvicinato)")]
    [SerializeField] private float _defaultFirstFovRadius = 7f;
    public float defaultFirstFovRadius {
        get { return _defaultFirstFovRadius; }
    }
    private float _usedFirstFovRadius = 0;
    public float usedFirstFovRadius {
        get { return _usedFirstFovRadius; }
    }
    [Range(0, 360)]
    [SerializeField] private float _defaultFirstFovAngle = 90;
    private float _usedFirstFovAngle = 0;
    public float usedFirstFovAngle {
        get { return _usedFirstFovAngle; }
    }
    [SerializeField] private bool firstFOVCanSeeCharacter = false;
    [SerializeField] private bool secondFOVCanSeeCharacter = false;

    [SerializeField] private bool firstFOVCanSeeDeadCharacter = false;
    [SerializeField] private bool secondFOVCanSeeDeadCharacter = false;


    [Header("Secondo campo visivo(lontano)")]
    [SerializeField] private float _defaultSecondFovRadius = 18;
    public float defaultSecondFovRadius {
        get { return _defaultSecondFovRadius; }
    }
    private float _usedSecondFovRadius = 0;
    public float usedSecondFovRadius {
        get { return _usedSecondFovRadius; }
    }
    [Range(0, 360)]
    [SerializeField] private float _defaultSecondFovAngle = 145;
    private float _usedSecondFovAngle = 0;
    public float usedSecondFovAngle {
        get { return _usedSecondFovAngle; }
    }
    

    [Header("Area di allerta")]
    //L'area di allerta viene utilizzata per rilevare i Characters vicini all'NPC e nel caso informarli o aggiornali istantaneamente su eventuali eventi
    [SerializeField] private float _alertArea = 15;
    public float alertArea {
        get { return _alertArea; }
    }
    


    public void Start() {
        setFOVValuesToDefault();
        StartCoroutine(cFOVRoutine());
    }

    public void setFOVValuesToDefault() {
        _usedFirstFovRadius = _defaultFirstFovRadius;
        _usedFirstFovAngle = _defaultFirstFovAngle;

        _usedSecondFovRadius = _defaultSecondFovRadius;
        _usedSecondFovAngle = _defaultSecondFovAngle;
    }

    public void setFOVValues(
        float firstFovRadius,
        float firstFovAngle,
        float secondFovRadius,
        float secondFovAngle
        ) {
        _usedFirstFovRadius = firstFovRadius;
        _usedFirstFovAngle = firstFovAngle;

        _usedSecondFovRadius = secondFovRadius;
        _usedSecondFovAngle = secondFovAngle;
    }

    private IEnumerator cFOVRoutine() {

        while(true) {
            yield return new WaitForSeconds(fovCheckFrequency);
            characterFirstFOVCheck();
            characterSecondFOVCheck();
        }
    }

    public void stopAllCoroutines() {
        firstFOVCanSeeCharacter = false;
        secondFOVCanSeeCharacter = false;
        StopAllCoroutines();
    }

    /// <summary>
    /// check campo visivo ravvicinato
    /// </summary>
    private void characterFirstFOVCheck() {

        
        Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, _usedFirstFovRadius, targetCharacterMask);
        bool characterSeen = false;

        if (hitColliders.Length != 0) {

            foreach(Collider collider in hitColliders) {
                if (collider.gameObject.GetComponent<CharacterManager>().isPlayer) {

                    characterSeen = isCharactersVisibleInFirstFOV(collider.gameObject.GetComponent<CharacterFOV>().recognitionTarget.position);


                }
            }
            
        }

        if(characterSeen) {
            firstFOVCanSeeCharacter = true;
        } else {
            firstFOVCanSeeCharacter = false;
        }
    }

    /// <summary>
    /// check campo visivo lontananza
    /// </summary>
    private void characterSecondFOVCheck() {


        Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, _usedSecondFovRadius, targetCharacterMask);
        bool characterSeen = false;

        if (hitColliders.Length != 0) {
            
            foreach (Collider collider in hitColliders) {
                if (collider.gameObject.GetComponent<CharacterManager>().isPlayer) {

                    characterSeen = isCharactersVisibleInSecondFOV(collider.gameObject.GetComponent<CharacterFOV>().recognitionTarget.position);
                } else {

                    if(collider.gameObject.GetComponent<CharacterManager>().isDead) {


                        CharacterManager deadCharacterManager = collider.gameObject.GetComponent<CharacterManager>();
                        bool deadCharacterVisible = isDeadCharactersVisibleInSecondFOV(collider.gameObject.transform.position, deadCharacterManager);
                    }
                }
            }

        }

        if(characterSeen) {
            secondFOVCanSeeCharacter = true;
        } else {
            secondFOVCanSeeCharacter = false;
        }
    }


    public bool isCharactersVisibleInFirstFOV(Vector3 raycastPositionTargetToReach) {
        bool result = false;
        Vector3 fromPosition = _recognitionTarget.position;
        Vector3 toPosition = raycastPositionTargetToReach;

        Vector3 direction = toPosition - fromPosition;

        if (Vector3.Angle(transform.forward, direction) < (_usedFirstFovAngle / 2)) {
            //Debug.Log("angle player rilevato");



            
            RaycastHit hit;
            if (Physics.Raycast(fromPosition, direction, out hit, _usedFirstFovRadius, ALL_LAYERS, QueryTriggerInteraction.Ignore)) {

                //Debug.Log(hit.collider.gameObject.name);

                CharacterManager seenCharacter = hit.collider.gameObject.GetComponent<CharacterManager>();
                if (hit.collider.gameObject.layer == CHARACTER_LAYERS) {

                    Debug.DrawLine(fromPosition, hit.point, Color.red, fovCheckFrequency);
                    result = true;
                    onFirstFOVCanSeeCharacter(seenCharacter);
                }


            }
        }
        return result;
    }

    public bool isCharactersVisibleInSecondFOV(Vector3 raycastPositionTargetToReach) {
        bool result = false;
        Vector3 fromPosition = _recognitionTarget.position;
        Vector3 toPosition = raycastPositionTargetToReach;
        Vector3 direction = toPosition - fromPosition;

        if (Vector3.Angle(transform.forward, direction) < (_usedSecondFovAngle / 2)) {
            //Debug.Log("angle player rilevato");



            
            RaycastHit hit;
            if (Physics.Raycast(fromPosition, direction, out hit, _usedSecondFovRadius, ALL_LAYERS, QueryTriggerInteraction.Ignore)) {


                //Debug.Log(hit.collider.gameObject.name);
                CharacterManager seenCharacter = hit.collider.gameObject.GetComponent<CharacterManager>();
                if (hit.collider.gameObject.layer == CHARACTER_LAYERS) {


                    if (!firstFOVCanSeeCharacter) {
                        Debug.DrawLine(fromPosition, hit.point, Color.magenta, fovCheckFrequency);
                    }

                    result = true;
                    onSecondFOVCanSeeCharacter(seenCharacter);

                }


            }
        }

        return result;
    }

    public bool isDeadCharactersVisibleInSecondFOV(Vector3 raycastPositionTargetToReach, CharacterManager deadCharacter) {
        bool result = false;
        Vector3 fromPosition = _recognitionTarget.position;
        Vector3 toPosition = raycastPositionTargetToReach;
        Vector3 direction = toPosition - fromPosition;

        
        if (Vector3.Angle(transform.forward, direction) < (_usedSecondFovAngle / 2)) {


            // ottieni i transform per la conferma della visualizzazione del character
            List<Transform> deadCharacterTargetTransforms = deadCharacter.gameObject.GetComponent<RagdollManager>().deadCharacterTargetTransform;

            RaycastHit hit;

            foreach(Transform deadCharacterTarget in deadCharacterTargetTransforms) {

                if (Physics.Linecast(reachableTarget.position, deadCharacterTarget.position, out hit, ALL_LAYERS, QueryTriggerInteraction.Ignore)) {

                    if (hit.collider != null) {


                        if(hit.transform.gameObject.layer == RAGDOLL_BONE) {
                            result = true;
                            Debug.DrawLine(reachableTarget.position, hit.point, Color.magenta, fovCheckFrequency);
                        }
                    } else {
                        result = true;
                        Debug.Log("line not cast collide");
                        Debug.DrawLine(reachableTarget.position, hit.point, Color.magenta, fovCheckFrequency);
                    }
                } else {
                    Debug.Log("line not cast collide");
                    result = true;
                    Debug.DrawLine(reachableTarget.position, hit.point, Color.magenta, fovCheckFrequency);
                }
            }
            



            if (result)
                onSecondFOVCanSeeDeadCharacter(deadCharacter);
        }

        return result;
    }

    private void onFirstFOVCanSeeCharacter(CharacterManager seenCharacter) {

        float lastPosX = seenCharacter.transform.position.x;
        float lastPosY = seenCharacter.transform.position.y;
        float lastPosZ = seenCharacter.transform.position.z;
        Vector3 lastPos = new Vector3(lastPosX, lastPosY, lastPosZ);


        nPCBehaviour.hostilityCheck(seenCharacter, lastPos, true);
    }

    private void onSecondFOVCanSeeCharacter(CharacterManager seenCharacter) {

        if(!firstFOVCanSeeCharacter) {

            float lastPosX = seenCharacter.transform.position.x;
            float lastPosY = seenCharacter.transform.position.y;
            float lastPosZ = seenCharacter.transform.position.z;
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
            float lastPosX = seenDeadCharacter.transform.position.x;
            float lastPosY = seenDeadCharacter.transform.position.y;
            float lastPosZ = seenDeadCharacter.transform.position.z;
            Vector3 lastPos = new Vector3(lastPosX, lastPosY, lastPosZ);


            nPCBehaviour.suspiciousCorpseFoundCheck(seenDeadCharacter, lastPos);
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

        if (Physics.Linecast(reachableTarget.position, characterToReach.reachableTarget.position, out hit, ALL_LAYERS, QueryTriggerInteraction.Ignore)) {

            if (hit.collider != null) {
                res = false;

            } else {
                res = true;
            }
        } else {
            res = true;
        }

        return res;
    }

    
    /// <summary>
    /// Ottieni tutti i character nella sua area di allerta.
    /// Da usare per comunicare istantanemente un allarme
    /// </summary>
    /// <returns></returns>
    public Dictionary<int, BaseNPCBehaviour> getAlertAreaCharacters() {

        Dictionary<int, BaseNPCBehaviour> characters = new Dictionary<int, BaseNPCBehaviour>();

        Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, _alertArea, targetCharacterMask);

        if (hitColliders.Length != 0) {

            foreach (Collider collider in hitColliders) {

                if (!collider.gameObject.GetComponent<CharacterManager>().isPlayer) {

                    if (collider.gameObject.GetComponent<CharacterRole>().role == Role.EnemyGuard) {

                        // evita la creazione di doppioni
                        if (!characters.ContainsKey(collider.gameObject.GetComponent<EnemyNPCBehaviour>().GetInstanceID()))
                            characters.Add(collider.gameObject.GetComponent<EnemyNPCBehaviour>().GetInstanceID(), collider.gameObject.GetComponent<EnemyNPCBehaviour>());

                    } else if (collider.gameObject.GetComponent<CharacterRole>().role == Role.Civilian) {

                        // evita la creazione di doppioni
                        if (!characters.ContainsKey(collider.gameObject.GetComponent<CivilianNPCBehaviour>().GetInstanceID()))
                            characters.Add(collider.gameObject.GetComponent<CivilianNPCBehaviour>().GetInstanceID(), collider.gameObject.GetComponent<CivilianNPCBehaviour>());

                    }
                }

            }

        }

        // rimozione character che fa il controllo
        // se stesso non fa parte dei character rilevati dall'alarm area
        if (gameObject.GetComponent<CharacterRole>().role == Role.EnemyGuard) {

            characters.Remove(gameObject.GetComponent<EnemyNPCBehaviour>().GetInstanceID());
        } else if (gameObject.GetComponent<CharacterRole>().role == Role.Civilian) {

            characters.Remove(gameObject.GetComponent<CivilianNPCBehaviour>().GetInstanceID());
        }


        return characters;
    }

#if UNITY_EDITOR
    private void drawfirstFOVEditor() {

        
        Handles.color = Color.white;
        Handles.DrawWireDisc(gameObject.transform.position, Vector3.up, _usedFirstFovRadius == 0 ? _defaultFirstFovRadius : _usedFirstFovRadius); //debug radius fov


        // draw dell'angolo del fov range

        // forward rispetto al character
        Handles.color = Color.white;
        Handles.DrawLine(
            gameObject.transform.position,
            gameObject.transform.position + new Vector3(
                Mathf.Sin((transform.eulerAngles.y) * (Mathf.Deg2Rad)),
                0,
                Mathf.Cos((transform.eulerAngles.y) * (Mathf.Deg2Rad))
            ) * _usedFirstFovRadius,
            1
        );


        // forward + 20 gradi rispetto al character
        Handles.color = Color.yellow;
        Handles.DrawLine(
            gameObject.transform.position,
            gameObject.transform.position + new Vector3(
                Mathf.Sin((transform.eulerAngles.y + ((_usedFirstFovAngle == 0 ? _defaultFirstFovAngle : _usedFirstFovAngle) / 2)) * (Mathf.Deg2Rad)),
                0,
                Mathf.Cos((transform.eulerAngles.y + ((_usedFirstFovAngle == 0 ? _defaultFirstFovAngle : _usedFirstFovAngle) / 2)) * (Mathf.Deg2Rad))
            ) * (_usedFirstFovRadius == 0 ? _defaultFirstFovRadius : _usedFirstFovRadius),
            5
        );

        // forward - 20 gradi rispetto al character
        Handles.color = Color.yellow;
        Handles.DrawLine(
            gameObject.transform.position,
            gameObject.transform.position + new Vector3(
                Mathf.Sin((transform.eulerAngles.y + (-(_usedFirstFovAngle == 0 ? _defaultFirstFovAngle : _usedFirstFovAngle) / 2)) * (Mathf.Deg2Rad)),
                0,
                Mathf.Cos((transform.eulerAngles.y + (-(_usedFirstFovAngle == 0 ? _defaultFirstFovAngle : _usedFirstFovAngle) / 2)) * (Mathf.Deg2Rad))
            ) * (_usedFirstFovRadius == 0 ? _defaultFirstFovRadius : _usedFirstFovRadius),
            5
        );
    }

    private void drawSecondFOVEditor() {


        Handles.color = Color.white;
        Handles.DrawWireDisc(gameObject.transform.position, Vector3.up, _usedSecondFovRadius == 0 ? _defaultSecondFovRadius : _usedSecondFovRadius); //debug radius fov


        // draw dell'angolo del fov range

        // forward rispetto al character
        Handles.color = Color.white;
        Handles.DrawLine(
            gameObject.transform.position,
            gameObject.transform.position + new Vector3(
                Mathf.Sin((transform.eulerAngles.y) * (Mathf.Deg2Rad)),
                0,
                Mathf.Cos((transform.eulerAngles.y) * (Mathf.Deg2Rad))
            ) * (_usedSecondFovRadius == 0 ? _defaultSecondFovRadius : _usedSecondFovRadius),
            1
        );


        // forward + 20 gradi rispetto al character
        Handles.color = Color.cyan;
        Handles.DrawLine(
            gameObject.transform.position,
            gameObject.transform.position + new Vector3(
                Mathf.Sin((transform.eulerAngles.y + ((_usedSecondFovAngle == 0 ? _defaultSecondFovAngle : _usedSecondFovAngle) / 2)) * (Mathf.Deg2Rad)),
                0,
                Mathf.Cos((transform.eulerAngles.y + ((_usedSecondFovAngle == 0 ? _defaultSecondFovAngle : _usedSecondFovAngle) / 2)) * (Mathf.Deg2Rad))
            ) * (_usedSecondFovRadius == 0 ? _defaultSecondFovRadius : _usedSecondFovRadius),
            5
        );

        // forward - 20 gradi rispetto al character
        Handles.color = Color.cyan;
        Handles.DrawLine(
            gameObject.transform.position,
            gameObject.transform.position + new Vector3(
                Mathf.Sin((transform.eulerAngles.y + (-(_usedSecondFovAngle == 0 ? _defaultSecondFovAngle : _usedSecondFovAngle) / 2)) * (Mathf.Deg2Rad)),
                0,
                Mathf.Cos((transform.eulerAngles.y + (-(_usedSecondFovAngle == 0 ? _defaultSecondFovAngle : _usedSecondFovAngle) / 2)) * (Mathf.Deg2Rad))
            ) * (_usedSecondFovRadius == 0 ? _defaultSecondFovRadius : _usedSecondFovRadius),
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
