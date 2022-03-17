using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ButtonInteraction : MonoBehaviour
{

    [SerializeField]
    private Text buttonTextComponent;

    private string buttonText = "ACTION 1";

    private Interaction interaction;

    /// <summary>
    /// La funzione inizializza il componente button e l'evento del bottone
    /// </summary>
    /// <param name="interaction">interaction che il bottone esegue e rappresenta</param>
    /// <param name="characterInteraction">Rappresenta l'istanza componente CharacterInteraction che effettua l'azione</param>
    public void initButton(Interaction interaction, CharacterInteraction characterInteraction) {

        string interactionName = interaction.getUnityEventName();
        setButtonText(interactionName);

        // aggiungi l'evento che il button deve eseguire
        GetComponent<Button>().onClick.AddListener(
            delegate { 
                interaction.getUnityEvent().Invoke(characterInteraction); // avvia interaction
                characterInteraction.buildListOfInteraction(); // rebuild della lista di eventi
            }
        );
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
