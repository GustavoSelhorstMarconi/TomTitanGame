using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class LifeControl : MonoBehaviour
{
    [Header("Configurations")]
    [SerializeField]
    private int maxLife;
    private int currentLife;

    [SerializeField]
    private UnityEvent<int, int> OnTakeDamage;
    [SerializeField]
    private UnityEvent OnLosePlayer;

    private void Start()
    {
        currentLife = maxLife;
        OnTakeDamage?.Invoke(maxLife, currentLife);
    }

    public void TakeDamage(int damage)
    {
        currentLife -= damage;
        if (currentLife < 0)
        {
            currentLife = 0;
        }
        OnTakeDamage?.Invoke(maxLife, currentLife);
        if (currentLife <= 0)
        {
            OnLosePlayer?.Invoke();
            gameObject.GetComponent<PlayerMovement>().enabled = false;
        }
    }
}
