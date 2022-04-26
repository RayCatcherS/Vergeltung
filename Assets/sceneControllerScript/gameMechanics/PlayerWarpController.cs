using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWarpController : MonoBehaviour
{
    [SerializeField] private List<CharacterManager> warpedCharacterManagerStach = new List<CharacterManager>();



    /// <summary>
    /// warp to a new character player and push to stack the new warped character
    /// </summary>
    public void warpPlayerToCharacter(CharacterManager character) {

        if(warpedCharacterManagerStach.Count == 0) {
            character.isPlayer = true;
            warpedCharacterManagerStach.Add(character);
        }
        
    }
}
