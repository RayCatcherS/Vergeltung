using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CharacterInteraction : MonoBehaviour
{
    private const int INTERACTABLE_LAYER = 3;

    
    private Dictionary<int, Interactable> interactableObjects = new Dictionary<int, Interactable>(); // dizionario Interactable ottenuti dagli onTrigger

    
    private List<Interaction> interactions = new List<Interaction>(); // lista di tutte le Interaction disponibili per il player


    [SerializeField]
    private InteractionUIController interactionUIController; // controller per interagire con l'UI delle interazioni


    /// <summary>
    /// Aggiunge e inizializza un CharacterInteraction component al gameObject
    /// Infine restituisce il gameObject
    /// </summary>
    /// <param name="gameObject">gameObject a cui aggiungere il componente CharacterInteraction</param>
    /// <returns></returns>
    public static GameObject addToGOCharacterInteractionComponent(GameObject gameObject, InteractionUIController controller) {
        gameObject.AddComponent<CharacterInteraction>();
        
        CharacterInteraction characterInteraction = gameObject.GetComponent<CharacterInteraction>(); // aggiungi componente CharacterInteraction 
        characterInteraction.interactionUIController = controller; // assegna al interactionUIController al componente CharacterInteraction

        return gameObject;
    }

    public void setInteractionUIController(InteractionUIController controller) {
        interactionUIController = controller;
    }

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

    
    public void buildListOfInteraction() {
        interactions = new List<Interaction>(); // svuota lista


        // ottieni dal dizionario degli oggetti interabili tutte le interactions
        foreach (var item in interactableObjects) {

            List<Interaction> interactable = item.Value.getInteractable();
            
            for(int i = 0; i < interactable.Count; i++) {

                interactions.Add(interactable[i]);
            }
        }


        // se il character è giocato dal player
        if(gameObject.GetComponent<CharacterState>().isPlayer) {

            // inizializza lista di interazioni e i bottoni e la partendo dalla lista interactions
            // passa la lista di interactions per inizializzare la lista di interacion che potranno essere effettuate
            interactionUIController.buildUIinteractionList(interactions, this);
        }

    }
}