using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletAction : MonoBehaviour
{
    public int damage;
    private Rigidbody bulletRigidBody;
    private bool targetHit;

    private void Start()
    {
        bulletRigidBody = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision other)
    {
        if (targetHit)
        {
            return;
        }
        else
        {
            targetHit = true;
        }

        if (other.gameObject.GetComponent<EnemyDamage>())
        {
            EnemyDamage enemy = other.gameObject.GetComponent<EnemyDamage>();

            enemy.TakeDamage(damage);

            Destroy(gameObject);
        }

        bulletRigidBody.isKinematic = true;

        transform.SetParent(other.transform);
    }
}
