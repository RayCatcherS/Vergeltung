using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Questo componente gestisce l'area di appartenenza del character e
/// contiene i metodi per verificare se il character � in una area consentita
/// </summary>
public class CharacterAreaManager : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayerMask;
    private float _collideRadius = 0.5f;
    [SerializeField] private int belongingAreaId = -1; // area appartenenza character -1 significa area non assegnata
    void Start()
    {
        belongingAreaId = getIdArea(); // assegna area appartenenza
        
    }

    /// <summary>
    /// Ottieni trigger collider
    /// </summary>
    /// <returns></returns>
    private int getIdArea() {
        Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, _collideRadius, targetLayerMask);
        int result = -1;

        if (hitColliders.Length != 0) {

            result = hitColliders[0].gameObject.GetComponent<CharacterArea>().GetInstanceID();
        }

        return result;
    }

    /// <summary>
    /// Verifica se il character apparitiene o meno a una certa area
    /// </summary>
    /// <returns>[true] character appartiene, [false] non appartiene</returns>
    private bool characterBelongsArea() {
        bool result = false;

        int triggerArea = getIdArea();

        if(triggerArea == -1) { // nessuna area rilevata con cui verificare l'appartenenza
            result = true;

        } else {

            if(triggerArea == belongingAreaId) {
                result = true;
            } else {
                result = false;
            }

        }


        return result;
    }

    /// <summary>
    /// verifica se il character si trova in un area consentita.
    /// Il controllo viene fatto in base al tipo di ruolo del character e in base all'area di
    /// appartenenza di partenza "belongingAreaId"
    /// </summary>
    /// <returns>[true] se il character si trova in una area a lui proibita, altrimenti [false]</returns>
    public bool isCharacterInProhibitedAreaCheck() {
        bool result = true;

        Role role = gameObject.GetComponent<CharacterRole>().role;

        if(role == Role.EnemyGuard) {
            result = false;
        } else if(role == Role.Civilian || role == Role.Player) {

            if(characterBelongsArea()) {
                result = false;
            } else {
                result = true;
            }
        }

        return result;
    }
}
