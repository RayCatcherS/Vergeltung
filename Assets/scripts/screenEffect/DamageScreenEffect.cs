using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageScreenEffect : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Animator damageAnimator;
    public void playDamageEffect() {
        damageAnimator.SetTrigger("damage");
    }
}
