using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Classe che si occupa della costruzione dell'UI delle interazioni del player
/// </summary>
public class InteractionUIController : MonoBehaviour
{
    [SerializeField]
    GameObject interactionButtonPrefab;

    [SerializeField]
    GameObject interactionListPanel;

    [SerializeField]
    List<GameObject> interactionButtons = new List<GameObject>();

    [SerializeField]
    private EventSystem eventSystem;

    /// <summary>
    /// Builda lista di bottoni corrisponenti alle interazioni che il giocatore
    /// può effettuare
    /// </summary>
    /// <param name="interactionList">Lista di interaction</param>
    /// <param name="characterInteraction">Rappresenta l'istanza componente CharacterInteraction che può effettuare le azioni</param>
    public void buildUIinteractionList(List<Interaction> interactionList, CharacterManager characterInteraction) {

        

        for (int i = 0; i < interactionButtons.Count; i++) {
            GameObject button = interactionButtons[i];
            Destroy(button); // distruggi istanza bottone
        }
        interactionButtons = new List<GameObject>(); // rimuovi ref istanza bottone dalla lista dei bottonu


        for (int i = 0; i < interactionList.Count; i++) {

            interactionList[i].getInteractable().unFocusInteractable(); // unfocus dell'oggetto buildato

            // istanzia bottone interaction
            GameObject newButton = Instantiate(interactionButtonPrefab);
            newButton.transform.SetParent(interactionListPanel.gameObject.transform); // setta transform bottone come figlio dell'interactionListPanel
            newButton.transform.localScale = new Vector3(1, 1, 1); //setta scala corretta indipendente dal padre


            newButton.GetComponent<ButtonInteraction>().initButton(interactionList[i], characterInteraction); // inizializza ButtonInteraction

            interactionButtons.Add(newButton); // aggiungi nuova istanza bottone alla lista di bottoni
        }
        
        // se la lista di bottoni interaction non è vuota
        if(interactionList.Count > 0) {
            eventSystem.SetSelectedGameObject(interactionButtons[0]); // setta il primo bottone come selezionato
        }
        
    }
}
