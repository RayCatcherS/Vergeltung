using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Role {
    Enemy,
    Player,
    Civilian,
    Hostage
}

public class CharacterRole : MonoBehaviour
{
    public static string GetCharacterRoleName(Role role) {
        string res = "none";


        switch (role) {
            case Role.Enemy: {
                res = "Enemy";
            }break;

            case Role.Player: {
                res = "Player";
            }break;

            case Role.Civilian: {
                res = "Civilian";
            }break;

            case Role.Hostage: {
                res = "Hostage";
            }
            break;
        }
        return res;
    }

    [SerializeField] Role role;

    // Start is called before the first frame update
    void Start()
    {
        
    }
}
