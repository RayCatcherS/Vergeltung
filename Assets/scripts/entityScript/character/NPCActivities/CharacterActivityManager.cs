using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CharacterActivityManager : MonoBehaviour
{
    [SerializeField] private List<CharacterActivity> characterActivities = new List<CharacterActivity>();

    [SerializeField] private int selectedCharacterActivityPos = 0;
    [SerializeField] private int selectedTaskPos = 0;

    // getter
    public List<CharacterActivity> getCharacterActivities() {
        return characterActivities;
    }

    public void setNextTaskPosOfActualActivity() {

        if(!characterActivities[selectedCharacterActivityPos].isLastTask(selectedTaskPos)) { // se non è l'ultimo task
            selectedTaskPos = selectedTaskPos + 1;

        } else {
            Debug.LogError("Fuori index settaggio nuovo task");
        }
    }

    public bool isActualActivityLastTask() {
        return characterActivities[selectedCharacterActivityPos].isLastTask(selectedTaskPos);
    }

    public ActivityTask getCurrentTask() {
        return characterActivities[selectedCharacterActivityPos].getTask(selectedTaskPos);
    }


    public CharacterActivity getCurrentCharacterActivity() {
        return characterActivities[selectedCharacterActivityPos];
    }

    public void Start() {
        if(characterActivities.Count > 0) {
            randomCharacterActivity();
        }
    }

    public void resetSelectedTaskPos() {
        selectedTaskPos = 0;
    }

    /// <summary>
    /// Scegli un nuovo character activity casuale e parti dal primo task
    /// </summary>
    public void randomCharacterActivity() {
        if(characterActivities.Count > 0) {
            randomizeSelectedActivity();
            selectedTaskPos = 0;
        } else if(characterActivities.Count == 0) {
            Debug.LogError("Nessuna activity da inizializzare");
        }
    }
    
    /// <summary>
    /// seleziona una activity in modo casuale
    /// </summary>
    private void randomizeSelectedActivity() {
        int randomizeActivity = Random.Range(0, characterActivities.Count);
        selectedCharacterActivityPos = randomizeActivity;
    }


    /// <summary>
    /// Crea un nuova activity
    /// dopo che viene istanziato viene aggiunto alle lista activity
    /// </summary>
    public GameObject newCharacterActivity() {

        GameObject newActivity = CharacterActivity.addToGOCharacterActivityComponent(new GameObject(), this); // crea nuovo gameObject e aggiungi componente character activity

        newActivity.name = "Activity " + characterActivities.Count.ToString(); // nome del gameobject

        characterActivities.Add(newActivity.GetComponent<CharacterActivity>());

        newActivity.gameObject.transform.position = transform.position; // setta posizione activity


        newActivity.transform.SetParent(gameObject.transform); // setta come figlio del gamecontroller

        return newActivity;
    }


    /// <summary>
    /// Rimuovi una activity dalla lista partendo dall'id del gameObject a cui è associato
    /// Viene rimosso dalla lista delle activity e dalla scena
    /// </summary>
    /// <param name="characterActivity"></param>
    public void removeCharacterActivityByIID(CharacterActivity characterActivity) {

        
        for (int i = 0; i < characterActivities.Count; i++) {

            
            if (characterActivities[i].GetInstanceID() == characterActivity.GetInstanceID()) {

                
                GameObject characterActivityGO = characterActivities[i].gameObject;

                characterActivities.RemoveAt(i); // rimuovi istanza dalla lista delle activities dei characters
                DestroyImmediate(characterActivityGO); // distruggi gameobject dello spawn dalla scena
            }
        }
    }
}