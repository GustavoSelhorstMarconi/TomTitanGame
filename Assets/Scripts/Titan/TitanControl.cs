using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Events;

public class TitanControl : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private Transform player;
    [SerializeField]
    private Transform spawner;
    [SerializeField]
    private GameObject stone;
    [SerializeField]
    private Rig lookRig;
    [SerializeField]
    private Transform lookTarget;
    [SerializeField]
    private ChainIKConstraint attackRigLeftHand;
    [SerializeField]
    private Transform attackLeftHandTarget;
    [SerializeField]
    private ChainIKConstraint attackRigRightHand;
    [SerializeField]
    private Transform attackRightHandTarget;
    [SerializeField]
    private GameObject moveAttackEffect;
    [SerializeField]
    private UnityEvent<int, int> OnTakeDamage;
    [SerializeField]
    private UnityEvent OnWin;

    [Header("Configurations")]
    [SerializeField]
    private float timeBetweenThrow;
    [SerializeField]
    private float timeBetweenThrowInSameThrow;
    [SerializeField]
    private float timeBetweenShortAttack;
    [SerializeField]
    private int maxStoneCapacity;
    [SerializeField]
    private float distanceToStartLongAttacks;
    [SerializeField]
    private float forceThrow;
    [SerializeField]
    private float distanceToShortAttacks;
    [SerializeField]
    private float shortAttackSpeed;
    [SerializeField]
    private float rotationLookSpeed;
    [SerializeField]
    private int maxDamagePerRound;
    [SerializeField]
    private int radiusSphereMoveAttack;
    [SerializeField]
    private LayerMask moveAttackLayer;
    [SerializeField]
    private int attackMoveDamage;
    [SerializeField]
    private float forceMoveAttack;
    [SerializeField]
    private int maxLife;
    [SerializeField]
    private UnityEvent<bool> OnActive;
    private int currentLife;

    private int currentDamageRound;
    private bool canLongAttack;
    private bool longAttacking;
    private bool canShortAttack;
    private bool activated;
    public bool shortAttacking { get; private set; }

    private enum MovementState
    {
        looking,
        throwing,
        shortAttacking
    }

    private MovementState currentState;

    private enum RigAnimMode
    {
        off,
        increasing,
        decreasing
    }

    private RigAnimMode mode = RigAnimMode.off;
    private bool rightShortAttack;

    private void Start()
    {
        currentState = MovementState.looking;
        currentLife = maxLife;
        OnTakeDamage?.Invoke(maxLife, currentLife);
    }

    private void Update()
    {
        lookTarget.position = player.position;
        CheckForAttacks();
        DefineRigState();

        if (canLongAttack && !longAttacking)
        {
            LongAttacks();
        }
        else if (canShortAttack && !shortAttacking)
        {
            ShortAttacks();
        }
        if (canShortAttack || canLongAttack)
        {
            LookAtPlayer();
        }
        if ((canShortAttack || canLongAttack) && !activated)
        {
            ActiveLifeText();
        }
    }

    private void FixedUpdate()
    {
        switch (mode)
        {
            case RigAnimMode.increasing:
                if (rightShortAttack)
                {
                    attackRigRightHand.weight = Mathf.Lerp(attackRigRightHand.weight, 1, shortAttackSpeed * Time.deltaTime);
                    if (attackRigRightHand.weight > 0.95f)
                    {
                        attackRigRightHand.weight = 1;
                        mode = RigAnimMode.decreasing;
                    }
                }
                else
                {
                    attackRigLeftHand.weight = Mathf.Lerp(attackRigLeftHand.weight, 1, shortAttackSpeed * Time.deltaTime);
                    if (attackRigLeftHand.weight > 0.95f)
                    {
                        attackRigLeftHand.weight = 1;
                        mode = RigAnimMode.decreasing;
                    }
                }
                break;
            case RigAnimMode.decreasing:
                if (rightShortAttack)
                {
                    attackRigRightHand.weight = Mathf.Lerp(attackRigRightHand.weight, 0, shortAttackSpeed * Time.deltaTime);
                    if (attackRigRightHand.weight < 0.1f)
                    {
                        attackRigRightHand.weight = 0;
                        mode = RigAnimMode.off;
                    }
                }
                else
                {
                    attackRigLeftHand.weight = Mathf.Lerp(attackRigLeftHand.weight, 0, shortAttackSpeed * Time.deltaTime);
                    if (attackRigLeftHand.weight < 0.1f)
                    {
                        attackRigLeftHand.weight = 0;
                        mode = RigAnimMode.off;
                    }
                }
                break;
        }
    }

    public void SpawnStone()
    {
        GameObject stoneClone = Instantiate(stone, spawner.position, Quaternion.identity);
        stoneClone.transform.SetParent(spawner);
    }

    public IEnumerator ThrowStone()
    {
        Vector3 directionThrow = (player.position - spawner.position).normalized;

        GameObject stoneClone = spawner.GetChild(0).gameObject;
        stoneClone.transform.SetParent(null);
        stoneClone.GetComponent<Rigidbody>().isKinematic = false;
        stoneClone.GetComponent<Rigidbody>().AddForce(directionThrow * forceThrow, ForceMode.Impulse);
        yield return new WaitForSeconds(timeBetweenThrowInSameThrow);

        for (int i = 0; i < Random.Range(1, maxStoneCapacity); i++)
        {
            float x = Random.Range(-i, i);
            float y = Random.Range(0, i);
            Vector3 directionThrowWithSpread = directionThrow + new Vector3(x, y, 0f);

            GameObject stoneCloneMoreShots = Instantiate(stone, spawner.position, Quaternion.identity);
            stoneCloneMoreShots.transform.SetParent(null);
            stoneCloneMoreShots.GetComponent<Rigidbody>().isKinematic = false;
            stoneCloneMoreShots.GetComponent<Rigidbody>().AddForce(directionThrowWithSpread * forceThrow, ForceMode.Impulse);
            yield return new WaitForSeconds(timeBetweenThrowInSameThrow);
        }
    }

    public void SetIdleAnimation()
    {
        animator.SetBool("LongAttack", true);
        animator.SetBool("Throwing1", false);
        animator.SetBool("Throwing2", false);
    }

    private void CheckForAttacks()
    {
        Vector3 distanceToPlayer = player.position - transform.position;
        if (distanceToPlayer.magnitude < distanceToStartLongAttacks && distanceToPlayer.magnitude > distanceToShortAttacks)
        {
            animator.SetBool("LongAttack", true);
            canLongAttack = true;
            canShortAttack = false;
            currentState = MovementState.throwing;
        }
        else if (distanceToPlayer.magnitude < distanceToShortAttacks)
        {
            animator.SetBool("LongAttack", false);
            canLongAttack = false;
            canShortAttack = true;
            currentState = MovementState.shortAttacking;
        }
        else
        {
            SetIdleAnimation();
            canLongAttack = false;
            canShortAttack = false;
            currentState = MovementState.looking;
        }
    }

    private void LongAttacks()
    {
        longAttacking = true;
        int attack = Random.Range(0, 3);

        if (attack <= 1)
        {
            animator.SetBool("Throwing1", true);
            animator.SetBool("Throwing2", false);
        }
        else
        {
            animator.SetBool("Throwing1", false);
            animator.SetBool("Throwing2", true);
        }

        if (canLongAttack)
        {
            Invoke("LongAttacks", timeBetweenThrow);
        }
        else
        {
            longAttacking = false;
        }
    }

    private void ShortAttacks()
    {
        shortAttacking = true;

        attackRigLeftHand.weight = 0;
        attackRigRightHand.weight = 0;
        mode = RigAnimMode.increasing;

        int attack = Random.Range(0, 3);
        if (attack <= 1)
        {
            attackLeftHandTarget.position = player.position;
            rightShortAttack = false;
        }
        else
        {
            attackRightHandTarget.position = player.position;
            rightShortAttack = true;
        }

        if (canShortAttack)
        {
            Invoke("ShortAttacks", timeBetweenShortAttack);
        }
        else
        {
            shortAttacking = false;
        }
    }

    private void DefineRigState()
    {
        if (currentState == MovementState.looking || currentState == MovementState.throwing)
        {
            lookRig.weight = 1;
            attackRigLeftHand.weight = 0;
            attackRigRightHand.weight = 0;
        }
        else if (currentState == MovementState.shortAttacking)
        {
            lookRig.weight = 0;
        }
        else
        {
            lookRig.weight = 0;
            attackRigLeftHand.weight = 0;
            attackRigRightHand.weight = 0;
        }
    }

    private void LookAtPlayer()
    {
        Quaternion rotationToLook = Quaternion.LookRotation(player.position - transform.position);
        rotationToLook = new Quaternion(transform.rotation.x, rotationToLook.y + Quaternion.Euler(0f, 90f, 0f).y, transform.rotation.z, transform.rotation.w);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotationToLook, Time.deltaTime * rotationLookSpeed);
    }

    private void CheckDamagePerRound()
    {
        if (currentDamageRound >= maxDamagePerRound)
        {
            currentDamageRound = 0;
            MoveAttack();
        }
    }

    private void MoveAttack()
    {
        Instantiate(moveAttackEffect, transform.position, Quaternion.identity, null);
        Collider[] objectsCollide = Physics.OverlapSphere(transform.position, radiusSphereMoveAttack, moveAttackLayer);
        for (int i = 0; i < objectsCollide.Length; i++)
        {
            if (objectsCollide[i].transform.parent.gameObject.GetComponent<LifeControl>())
            {
                objectsCollide[i].transform.parent.gameObject.GetComponent<LifeControl>().TakeDamage(attackMoveDamage);
                objectsCollide[i].transform.parent.gameObject.GetComponent<Rigidbody>().AddExplosionForce(forceMoveAttack, transform.position, radiusSphereMoveAttack);
            }
        }
    }

    private void CheckDeath()
    {
        if (currentLife <= 0)
        {
            Destroy(gameObject);
            OnWin?.Invoke();
            OnActive?.Invoke(false);
        }
    }

    private void ActiveLifeText()
    {
        OnActive?.Invoke(true);
        activated = true;
    }

    public void TakeDamage(int damage)
    {
        currentDamageRound += damage;
        currentLife -= damage;
        CheckDamagePerRound();
        CheckDeath();
        OnTakeDamage?.Invoke(maxLife, currentLife);
    }
}