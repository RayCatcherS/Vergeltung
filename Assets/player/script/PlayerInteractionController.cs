using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInteractionController : MonoBehaviour
{
    private const int INTERACTABLE_LAYER = 3;

    [SerializeField]
    Dictionary<int, Interactable> interactableObjects = new Dictionary<int, Interactable>(); // dizionario Interactable ottenuti dagli onTrigger

    [SerializeField]
    List<Interaction> interactions = new List<Interaction>(); // lista di tutte le Interaction disponibili per il player


    private void OnTriggerEnter(Collider collision) {

        if (collision.gameObject.layer == INTERACTABLE_LAYER) {

            InteractableObject interactableObject = collision.gameObject.GetComponent<InteractableObject>();


            // aggiungi interazione al dizionario delle interazioni
            interactableObjects.Add(interactableObject.GetInstanceID(), interactableObject.interactableObject);

            // rebuild lista interactions
            buildListOfInteraction();
        }
    }


    private void OnTriggerExit(Collider collision) {

        if (collision.gameObject.layer == INTERACTABLE_LAYER) {

            InteractableObject interactableObject = collision.gameObject.GetComponent<InteractableObject>();


            // aggiungi interazione al dizionario delle interazioni
            interactableObjects.Remove(interactableObject.GetInstanceID());

            // rebuild lista interactions
            buildListOfInteraction();
        }
    }

    
    void buildListOfInteraction() {
        interactions = new List<Interaction>(); // svuota lista


        // ottieni dal dizionario degli oggetti interabili tutte le interactions
        foreach (var item in interactableObjects) {

            List<Interaction> interactable = item.Value.getInteractable();
            
            for(int i = 0; i < interactable.Count; i++) {

                interactions.Add(interactable[i]);
            }
        }


        // debug
        if(interactions.Count == 1) {
            interactions[0].getUnityEvent().Invoke(this);
        }
    }
}