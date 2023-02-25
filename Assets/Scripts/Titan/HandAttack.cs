using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private TitanControl titanControl;

    [Header("Configurations")]
    [SerializeField]
    private int damage;
    [SerializeField]
    private float forceHit;

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.GetComponent<LifeControl>() && titanControl.shortAttacking)
        {
            other.gameObject.GetComponent<LifeControl>().TakeDamage(damage);
        }
        if (other.gameObject.GetComponent<Rigidbody>() && titanControl.shortAttacking)
        {
            Rigidbody playerRigidBody = other.gameObject.GetComponent<Rigidbody>();
            playerRigidBody.AddForce(Vector3.up * forceHit, ForceMode.Impulse);
        }
    }
}