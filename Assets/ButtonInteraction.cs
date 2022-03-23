using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ButtonInteraction : MonoBehaviour, ISelectHandler, IDeselectHandler {

    [SerializeField]
    private Text buttonTextComponent;

    private string buttonText = "ACTION 1";

    private Interactable interactable;

    /// <summary>
    /// La funzione inizializza il componente button e l'evento del bottone
    /// </summary>
    /// <param name="interaction">interaction che il bottone esegue e rappresenta</param>
    /// <param name="characterInteraction">Rappresenta l'istanza componente CharacterInteraction che effettua l'azione</param>
    public void initButton(Interaction interaction, CharacterInteraction characterInteraction) {

        string interactionName = interaction.getUnityEventName();
        setButtonText(interactionName);

        interactable = interaction.getInteractable(); // oggetto che concede l'interazione

        // aggiungi l'evento che il button deve eseguire
        GetComponent<Button>().onClick.AddListener(
            delegate { 
                interaction.getUnityEvent().Invoke(characterInteraction); // avvia interaction

                interactable.unFocusInteractable(); // unfocus dell'oggetto dell'interactable a cui appartiene
                characterInteraction.buildListOfInteraction(); // rebuild della lista di eventi
            }
        );

        



    }

    public void OnSelect(BaseEventData eventData) {

        interactable.focusInteractable();
    }

    public void OnDeselect(BaseEventData eventData) {
        interactable.unFocusInteractable();
    }

    void Start()
    {
        setButtonText(buttonText);
    }

    public void setButtonText(string text) {
        buttonText = text;
        buttonTextComponent.text = buttonText;

    }

    public void buttonDebug() {
        Debug.Log("Button debug");
    }
}
