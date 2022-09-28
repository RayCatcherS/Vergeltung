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

    [SerializeField] private Image buttonIconSprite;

    private Interactable interactable;

    /// <summary>
    /// La funzione inizializza il componente button e l'evento del bottone
    /// </summary>
    /// <param name="interaction">interaction che il bottone esegue e rappresenta</param>
    /// <param name="characterInteraction">Rappresenta l'istanza componente CharacterInteraction che effettua l'azione</param>
    public void initButton(Interaction interaction, CharacterManager characterInteraction) {

        string interactionName = interaction.getUnityEventName();
        setButtonText(interactionName);

        interactable = interaction.getInteractable(); // oggetto che concede l'interazione

        // aggiungi l'evento che il button deve eseguire
        GetComponent<Button>().onClick.AddListener(
            delegate { 
                interaction.getUnityEvent().Invoke(characterInteraction); // avvia interaction

                interactable.unFocusInteractableOutline(); // unfocus dell'oggetto dell'interactable a cui appartiene
                characterInteraction.buildListOfInteraction(); // rebuild della lista di eventi
            }
        );


        buttonNotSelected();
    }

    private void buttonNotSelected() {
        buttonIconSprite.enabled = false;


        Color color = Color.white;
        color.a = 0.75f;

        buttonTextComponent.color = color;
    }

    private void buttonSelected() {
        buttonIconSprite.enabled = true;


        Color color = Color.white;
        color.a = 1f;

        buttonTextComponent.color = color;
    }

    public void OnSelect(BaseEventData eventData) {

        interactable.focusInteractableOutline();
        buttonSelected();
    }

    public void OnDeselect(BaseEventData eventData) {
        interactable.unFocusInteractableOutline();
        buttonNotSelected();
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
