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
    [SerializeField] private Role _role;
    public Role role {
        get { return _role; }
    }

    /// <summary>
    /// Aggiunge e inizializza uno CharacterRole component al gameObject
    /// Infine restituisce il gameObject
    /// </summary>
    /// <param name="gameObject">gameObject a cui aggiungere il componente CharacterRole</param>
    /// <param name="role">ruolo character da attribuire </param>
    /// <returns></returns>
    static public GameObject addToGOCharacterRoleComponent(GameObject gameObject, Role role) {
        gameObject.AddComponent<CharacterRole>();

        CharacterRole characterRole = gameObject.GetComponent<CharacterRole>();
        characterRole.initCharacterRoleComponent(role); // inizializzazione componente

        return gameObject;
    }

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

    public void initCharacterRoleComponent(Role role) {
        this._role = role;
    }
}
