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
    private const int PLAYER_CHARACTER_LAYERS = 7;
    private const int ALL_LAYERS = -1;

    [SerializeField] private LayerMask targetCharacterMask;
    [SerializeField] private float fovCheckFrequency = 0.2f;
    [SerializeField] private Transform _recognitionTarget; // target utilizzato per confermare dai campi visivi dei character che il character è stato rilevato


    [Header("Primo campo visivo(ravvicinato)")]
    [SerializeField] private float _firstFovRadius = 9f;
    [Range(0, 360)]
    [SerializeField] private float _firstFovAngle = 55;
    [SerializeField] private bool firstFOVCanSeePlayer = false;

    [Header("Secondo campo visivo(lontano)")]
    [SerializeField] private float _secondFovRadius = 16;
    [Range(0, 360)]
    [SerializeField] private float _secondFovAngle = 125;
    [SerializeField] private bool secondFOVCanSeePlayer = false;

    [Header("Area di allerta")]
    [SerializeField] private float _areaAlert = 25;
    //L'area di allerta viene utilizzata per rilevare i Characters vicini all'NPC e nel caso informarli o aggiornali istantaneamente su eventuali eventi

    public float firstFovRadius {
        get => _firstFovRadius;
    }

    public float secondFovRadius {
        get => _secondFovRadius;
    }

    public float firstFovAngle {
        get => _firstFovAngle;
    }
    public float secondFovAngle {
        get => _secondFovAngle;
    }

    public Transform recognitionTarget {
        get => _recognitionTarget;
    }


    public void Start() {
        StartCoroutine(cFOVRoutine());
    }

    private IEnumerator cFOVRoutine() {

        while(true) {
            yield return new WaitForSeconds(fovCheckFrequency);
            firstFOVCheck();
            secondFOVCheck();
        }
    }

    public void stopAllCoroutines() {
        firstFOVCanSeePlayer = false;
        secondFOVCanSeePlayer = false;
        StopAllCoroutines();
    }


    private void firstFOVCheck() {

        
        Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, _firstFovRadius, targetCharacterMask);
        bool playerSeen = false;

        if (hitColliders.Length != 0) {

            foreach(Collider collider in hitColliders) {
                if (collider.gameObject.GetComponent<CharacterManager>().isPlayer) {

                    //Debug.Log("radius player rilevato");
                    Transform target = collider.gameObject.transform;
                    Vector3 directionToTarget = (target.position - transform.position).normalized;

                    if (Vector3.Angle(transform.forward, directionToTarget) < (firstFovAngle / 2)) {
                        //Debug.Log("angle player rilevato");



                        Vector3 fromPosition = _recognitionTarget.position;
                        Vector3 toPosition = collider.gameObject.GetComponent<CharacterFOV>().recognitionTarget.position;
                        Vector3 direction = toPosition - fromPosition;
                        RaycastHit hit;
                        if (Physics.Raycast(fromPosition, direction, out hit, _firstFovRadius, ALL_LAYERS, QueryTriggerInteraction.Ignore)) {

                            //Debug.Log(hit.collider.gameObject.name);
                            if(hit.collider.gameObject.layer == PLAYER_CHARACTER_LAYERS && hit.collider.gameObject.GetComponent<CharacterManager>().isPlayer) {

                                
                                Debug.DrawLine(fromPosition, hit.point, Color.red, fovCheckFrequency);
                                playerSeen = true;


                                //Debug.Log("Ehi sei il player");
                            }
                            

                        }
                    }
                }
            }
            
        }

        if(playerSeen) {
            firstFOVCanSeePlayer = true;
        } else {
            firstFOVCanSeePlayer = false;
        }
    }
    private void secondFOVCheck() {


        Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, _secondFovRadius, targetCharacterMask);
        bool playerSeen = false;

        if (hitColliders.Length != 0) {
            
            foreach (Collider collider in hitColliders) {
                if (collider.gameObject.GetComponent<CharacterManager>().isPlayer) {

                    //Debug.Log("radius player rilevato");
                    Transform target = collider.gameObject.transform;
                    Vector3 directionToTarget = (target.position - transform.position).normalized;

                    if (Vector3.Angle(transform.forward, directionToTarget) < (secondFovAngle / 2)) {
                        //Debug.Log("angle player rilevato");



                        Vector3 fromPosition = _recognitionTarget.position;
                        Vector3 toPosition = collider.gameObject.GetComponent<CharacterFOV>().recognitionTarget.position;
                        Vector3 direction = toPosition - fromPosition;
                        RaycastHit hit;
                        if (Physics.Raycast(fromPosition, direction, out hit, _secondFovRadius, ALL_LAYERS, QueryTriggerInteraction.Ignore)) {

                            //Debug.Log(hit.collider.gameObject.name);
                            if (hit.collider.gameObject.layer == PLAYER_CHARACTER_LAYERS && hit.collider.gameObject.GetComponent<CharacterManager>().isPlayer) {

                                if (!firstFOVCanSeePlayer)
                                Debug.DrawLine(fromPosition, hit.point, Color.magenta, fovCheckFrequency);
                                playerSeen = true;
                                

                                //Debug.Log("Ehi sei il player");
                            }


                        }
                    }
                }
            }

        }

        if(playerSeen) {
            secondFOVCanSeePlayer = true;
        } else {
            secondFOVCanSeePlayer = false;
        }
    }

#if UNITY_EDITOR
    private void drawfirstFOVEditor() {

        
        Handles.color = Color.white;
        Handles.DrawWireDisc(gameObject.transform.position, Vector3.up, firstFovRadius); //debug radius fov


        // draw dell'angolo del fov range

        // forward rispetto al character
        Handles.color = Color.white;
        Handles.DrawLine(
            gameObject.transform.position,
            gameObject.transform.position + new Vector3(
                Mathf.Sin((transform.eulerAngles.y) * (Mathf.Deg2Rad)),
                0,
                Mathf.Cos((transform.eulerAngles.y) * (Mathf.Deg2Rad))
            ) * firstFovRadius,
            1
        );


        // forward + 20 gradi rispetto al character
        Handles.color = Color.yellow;
        Handles.DrawLine(
            gameObject.transform.position,
            gameObject.transform.position + new Vector3(
                Mathf.Sin((transform.eulerAngles.y + (firstFovAngle / 2)) * (Mathf.Deg2Rad)),
                0,
                Mathf.Cos((transform.eulerAngles.y + (firstFovAngle / 2)) * (Mathf.Deg2Rad))
            ) * firstFovRadius,
            5
        );

        // forward - 20 gradi rispetto al character
        Handles.color = Color.yellow;
        Handles.DrawLine(
            gameObject.transform.position,
            gameObject.transform.position + new Vector3(
                Mathf.Sin((transform.eulerAngles.y + (-firstFovAngle / 2)) * (Mathf.Deg2Rad)),
                0,
                Mathf.Cos((transform.eulerAngles.y + (-firstFovAngle / 2)) * (Mathf.Deg2Rad))
            ) * firstFovRadius,
            5
        );
    }

    private void drawSecondFOVEditor() {


        Handles.color = Color.white;
        Handles.DrawWireDisc(gameObject.transform.position, Vector3.up, secondFovRadius); //debug radius fov


        // draw dell'angolo del fov range

        // forward rispetto al character
        Handles.color = Color.white;
        Handles.DrawLine(
            gameObject.transform.position,
            gameObject.transform.position + new Vector3(
                Mathf.Sin((transform.eulerAngles.y) * (Mathf.Deg2Rad)),
                0,
                Mathf.Cos((transform.eulerAngles.y) * (Mathf.Deg2Rad))
            ) * secondFovRadius,
            1
        );


        // forward + 20 gradi rispetto al character
        Handles.color = Color.cyan;
        Handles.DrawLine(
            gameObject.transform.position,
            gameObject.transform.position + new Vector3(
                Mathf.Sin((transform.eulerAngles.y + (secondFovAngle / 2)) * (Mathf.Deg2Rad)),
                0,
                Mathf.Cos((transform.eulerAngles.y + (secondFovAngle / 2)) * (Mathf.Deg2Rad))
            ) * secondFovRadius,
            5
        );

        // forward - 20 gradi rispetto al character
        Handles.color = Color.cyan;
        Handles.DrawLine(
            gameObject.transform.position,
            gameObject.transform.position + new Vector3(
                Mathf.Sin((transform.eulerAngles.y + (-secondFovAngle / 2)) * (Mathf.Deg2Rad)),
                0,
                Mathf.Cos((transform.eulerAngles.y + (-secondFovAngle / 2)) * (Mathf.Deg2Rad))
            ) * secondFovRadius,
            5
        );
    }
    
    private void drawAreaAlert() {
        Handles.color = Color.gray;
        Handles.DrawWireDisc(gameObject.transform.position, Vector3.up, _areaAlert);
    }

    void OnDrawGizmos() {
        if(!gameObject.GetComponent<CharacterManager>().isPlayer) {

            // debugga campi visivi solo se i character rilevano altri character player
            if(firstFOVCanSeePlayer || secondFOVCanSeePlayer) {
                drawfirstFOVEditor();
                drawSecondFOVEditor();
                drawAreaAlert();
            }
        }
    }
#endif
}
