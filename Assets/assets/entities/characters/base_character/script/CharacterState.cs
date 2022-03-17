using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterState : MonoBehaviour
{
    [SerializeField] public bool isRunning = false;
    //[SerializeField] public bool jumping = false;
    //[SerializeField] public bool readyToJump = false;
    [SerializeField] public bool isBusy = false;

    [SerializeField] public bool isPlayer = false; // tiene conto se il character è attualmente controllato dal giocatore



    // Start is called before the first frame update
    void Start()
    {
        
    }
}
