using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneControl : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private GameObject effect;

    [Header("Configurations")]
    [SerializeField]
    private int damage;
    [SerializeField]
    private float explosionForce;
    [SerializeField]
    private float explosionRadius;
    [SerializeField]
    private float upwardExplosionForce;
    [SerializeField]
    private LayerMask canExplode;

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.GetComponent<LifeControl>())
        {
            other.gameObject.GetComponent<LifeControl>().TakeDamage(damage);
        }
        Explosion();
    }

    private void Explosion()
    {
        GameObject effectClone = Instantiate(effect, transform.position, Quaternion.identity);
        effectClone.GetComponent<ParticleSystemRenderer>().material = gameObject.GetComponent<Renderer>().material;

        Collider[] objectsToExplode = Physics.OverlapSphere(transform.position, explosionRadius, canExplode);
        for (int i = 0; i < objectsToExplode.Length; i++)
        {
            if (objectsToExplode[i].GetComponent<Rigidbody>())
            {
                objectsToExplode[i].GetComponent<Rigidbody>().AddExplosionForce(explosionForce, transform.position, explosionRadius, upwardExplosionForce);
            }
            if (objectsToExplode[i].transform.parent.gameObject.GetComponent<Rigidbody>())
            {
                objectsToExplode[i].transform.parent.gameObject.GetComponent<Rigidbody>().AddExplosionForce(explosionForce, transform.position, explosionRadius, upwardExplosionForce);
            }
        }

        Destroy(gameObject, 0.05f);
    }
}