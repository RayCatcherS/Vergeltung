using UnityEngine.AI;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWarpController : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] private Camera gameCamera;

    [Header("Warp state")]
    [SerializeField] private List<CharacterManager> warpedCharacterManagerStach = new List<CharacterManager>();
    


    /// <summary>
    /// warp to a new character player and push to stack the new warped character
    /// </summary>
    public void warpPlayerToCharacter(CharacterManager character) {

        if(warpedCharacterManagerStach.Count == 0) {

            // aggiungi ref
            warpedCharacterManagerStach.Add(character);

            // configura character
            character.resetCharacterMovmentState();

            //disabilita componenti non necessari
            if(character.gameObject.GetComponent<Outline>() != null)
                character.gameObject.GetComponent<Outline>().enabled = false;
            if (character.gameObject.GetComponent<NavMeshAgent>() != null)
                character.gameObject.GetComponent<NavMeshAgent>().enabled = false;

            // configurazione character controllato dall'utente
            character.isPlayer = true;
            character.interactionUIController = gameObject.GetComponent<InteractionUIController>();

            // configurazione comandi
            gameObject.GetComponent<PlayerInputController>().characterMovement = character.GetComponent<CharacterMovement>();
            gameObject.GetComponent<PlayerInputController>().inventoryManager = character.GetComponent<InventoryManager>();

            // configurazione camera
            gameCamera.GetComponent<CoutoutObject>().targetObject = character.occlusionTargetTransform;
            gameCamera.GetComponent<FollowPlayer>().objectToFollow = character.occlusionTargetTransform;
        }

    }
}
