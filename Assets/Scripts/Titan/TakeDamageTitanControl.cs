using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakeDamageTitanControl : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private TitanControl titanControl;

    public void TakeDamage(int damage)
    {
        titanControl.TakeDamage(damage);
    }
}