using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletControl : MonoBehaviour
{
    [Header("References")]
    public Rigidbody bulletRigidBody;
    public GameObject explosion;
    public LayerMask whatIsEnemies;
    public GameObject effect;

    [Header("Configurations")]
    [Range(0f, 1f)]
    public float bounciness;
    public bool useGravity;

    [Header("Damage")]
    public int normalDamage;
    public int explosionDamage;
    public float explosionRange;
    public float explosionForce;

    [Header("Explosions")]
    public int maxCollisions;
    public float maxLifeTime;
    public bool explodeOnTouch = true;
    public bool canExplode;

    private int collisions;
    private PhysicMaterial physicsMaterial;
    private bool exploded = false;

    private void Start()
    {
        Setup();
        Destroy(gameObject, 5f);
    }

    private void Update()
    {
        if (collisions > maxCollisions && canExplode && !exploded)
        {
            Explode();
        }

        maxLifeTime -= Time.deltaTime;
        if (maxLifeTime <= 0 && canExplode && !exploded)
        {
            Explode();
        }
    }

    private void Explode()
    {
        exploded = true;

        if (explosion != null)
        {
            Instantiate(explosion, transform.position, Quaternion.identity);
        }

        Collider[] enemies = Physics.OverlapSphere(transform.position, explosionRange, whatIsEnemies);
        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].GetComponent<EnemyDamage>().TakeDamage(explosionDamage);
            if (enemies[i].GetComponent<Rigidbody>())
            {
                enemies[i].GetComponent<Rigidbody>().AddExplosionForce(explosionForce, transform.position, explosionRange);
            }
        }

        Invoke("Delay", 0.05f);
    }

    private void Delay()
    {
        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.collider.GetComponent<BulletGun>() || other.collider.GetComponent<BulletControl>())
        {
            return;
        }
        if (other.gameObject.CompareTag("Titan"))
        {
            Destroy(gameObject);
        }

        collisions++;

        if (other.collider.GetComponent<EnemyDamage>())
        {
            other.collider.GetComponent<EnemyDamage>().TakeDamage(normalDamage);
            if (explodeOnTouch && canExplode && !exploded)
            {
                Explode();
            }
        }
        if (other.gameObject.GetComponent<TakeDamageTitanControl>())
        {
            other.gameObject.GetComponent<TakeDamageTitanControl>().TakeDamage(normalDamage);
            if (effect != null)
            {
                Instantiate(effect, transform.position, Quaternion.identity);
            }
            if (explodeOnTouch && canExplode && !exploded)
            {
                Explode();
                other.gameObject.GetComponent<TakeDamageTitanControl>().TakeDamage(explosionDamage);
            }
            Destroy(gameObject, 0.05f);
        }
    }

    private void Setup()
    {
        physicsMaterial = new PhysicMaterial();
        physicsMaterial.bounciness = bounciness;
        physicsMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
        physicsMaterial.bounceCombine = PhysicMaterialCombine.Maximum;

        GetComponent<Collider>().material = physicsMaterial;

        bulletRigidBody.useGravity = useGravity;
    }
}