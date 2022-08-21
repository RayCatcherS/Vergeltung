using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarpUIController : MonoBehaviour {

    [Header("prefab Refs")]
    [SerializeField] private GameObject characterElementPrefab;

    [Header("Refs")]
    [SerializeField] private VerticalLayoutGroup characterListUI;


    private List<GameObject> characterElements = new List<GameObject>();

    public void rebuildWarpUI(List<CharacterManager> characters, CharacterManager usedCharacter) {

        List<CharacterManager> _characters = characters;

        // clear dei GO
        foreach(GameObject characterElement in characterElements) {
            Destroy(characterElement);
        }
        
        // genera UI
        for(int i = 0; i < characters.Count; i++) {

            // istanzia element
            GameObject cElement = Instantiate(characterElementPrefab);


            // inizializza
            if(characters[i].GetInstanceID() == usedCharacter.GetInstanceID()) {


                if(characters.Count == 1) {
                    cElement.GetComponent<WarpCharacterElement>().initWarpCharacterElement(true, false);
                } else {
                    cElement.GetComponent<WarpCharacterElement>().initWarpCharacterElement(true, true);
                }
            } else {
                cElement.GetComponent<WarpCharacterElement>().initWarpCharacterElement(false, false);
            }
            

            characterElements.Add(cElement);
            cElement.transform.SetParent(characterListUI.gameObject.transform); // setta transform bottone come figlio dell'interactionListPanel
        }


        
    }
}
